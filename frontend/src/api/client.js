import axios from 'axios'
import { tokenStorage } from './tokenStorage.js'

// Set this to the Lamp_Factory API base, e.g. https://localhost:7263/api
const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7263/api'

export const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' }
})

// The backend rotates the refresh token on every /auth/refresh call, and it
// treats a rotated (revoked) token showing up again as token theft — it revokes
// every session for that user. Two refreshes in flight at once would therefore
// burn the whole session, so overlapping attempts are collapsed into one.
let refreshPromise = null

// Exported so AuthContext can share the same single-flight refresh for its
// proactive renewals and initial session restore.
export function refreshTokens() {
  if (refreshPromise) return refreshPromise

  const refreshToken = tokenStorage.getRefreshToken()
  if (!refreshToken) return Promise.reject(new Error('No refresh token to renew with.'))

  // _isRefreshCall marks this request so its own 401 is never answered with
  // another refresh — that would loop forever.
  refreshPromise = apiClient
    .post('/auth/refresh', { refreshToken }, { _isRefreshCall: true })
    .then((data) => {
      const accessToken = data?.accessToken ?? data?.AccessToken
      const nextRefreshToken = data?.refreshToken ?? data?.RefreshToken
      if (!accessToken) throw new Error('The API did not return a new access token.')
      tokenStorage.setTokens({ accessToken, refreshToken: nextRefreshToken })
      return accessToken
    })
    .finally(() => {
      refreshPromise = null
    })

  return refreshPromise
}

// Attach the JWT (if present) to every request
apiClient.interceptors.request.use((config) => {
  const token = tokenStorage.getAccessToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Every controller returns Lamp_Factory's APIResponse envelope. The C# class
// is { IsSuccess, StatusCode, Result, ErrorMessage } but System.Text.Json's
// default camelCasing means the actual JSON on the wire is
// { isSuccess, statusCode, result, errorMessage }. This unwraps that
// envelope so callers just get `result` back, or throw a normalized Error
// carrying the backend's errorMessage list.
apiClient.interceptors.response.use(
  (response) => {
    const body = response.data
    if (body && typeof body === 'object' && 'isSuccess' in body) {
      if (body.isSuccess) {
        return body.result
      }
      const message = (body.errorMessage && body.errorMessage.join(' ')) || 'Request failed.'
      return Promise.reject(new Error(message))
    }
    return body
  },
  async (error) => {
    const original = error.config
    const status = error.response?.status

    // A 401 here means the access token is missing or expired. Trade the
    // refresh token in for a fresh pair once, then replay the original
    // request with the new access token. This must never run for the auth
    // endpoints themselves: a 401 from /auth/login or /auth/refresh means the
    // credentials/refresh token are no longer good and the user must sign in
    // again.
    const isAuthCall =
      original?._isRefreshCall || (original?.url ?? '').includes('/auth/login')

    if (status === 401 && original && !original._retried && !isAuthCall && tokenStorage.getRefreshToken()) {
      try {
        const accessToken = await refreshTokens()
        original._retried = true
        original.headers = original.headers || {}
        original.headers.Authorization = `Bearer ${accessToken}`
        return apiClient(original)
      } catch (refreshError) {
        // The refresh token was rejected (expired, already rotated, or flagged
        // as reused) and the backend has killed every session for this user.
        // Drop the local session too and let the reason surface on the login
        // screen.
        tokenStorage.clear(refreshError.message)
        return Promise.reject(refreshError)
      }
    }

    const data = error.response?.data
    let backendMessage
    if (data) {
      if (Array.isArray(data.errorMessage)) {
        backendMessage = data.errorMessage.join(' ')
      } else if (data.errorMessage) {
        backendMessage = data.errorMessage
      } else if (data.message) {
        backendMessage = data.message
      } else if (data.title) {
        const errors = data.errors
        if (errors && typeof errors === 'object') {
          const errorList = Object.entries(errors)
            .map(([field, msgs]) => `${field}: ${Array.isArray(msgs) ? msgs.join(' ') : msgs}`)
            .join('; ')
          backendMessage = errorList ? `${data.title} — ${errorList}` : data.title
        } else {
          backendMessage = data.title
        }
      }
    }
    const detail = backendMessage || error.message || 'Request failed.'
    const message = backendMessage ? `${detail}` : `Request failed with status ${status || 'unknown'}.`

    if (error.response?.status === 401) {
      // An unrecoverable 401 (no refresh token, or the auth endpoints rejecting
      // us) leaves no session to keep.
      tokenStorage.clear()
    }

    return Promise.reject(new Error(message))
  }
)
