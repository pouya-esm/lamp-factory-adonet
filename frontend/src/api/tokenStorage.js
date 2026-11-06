import { jwtDecode } from 'jwt-decode'

const ACCESS_TOKEN_KEY = 'lampFactory.token'
const REFRESH_TOKEN_KEY = 'lampFactory.refreshToken'

// The axios interceptor writes tokens from outside React (silent refresh,
// forced logout). React state stays in sync by subscribing rather than by
// trusting its own setState alone.
const listeners = new Set()

// When the backend forces a session out (rotated or reused refresh token), the
// reason is stashed here so the login screen can explain it. A plain logout
// passes no reason and stays silent.
let logoutReason = null

function notify() {
  listeners.forEach((listener) => {
    try {
      listener()
    } catch {
      // A listener throwing must never break a token write.
    }
  })
}

export const tokenStorage = {
  getAccessToken: () => localStorage.getItem(ACCESS_TOKEN_KEY),
  getRefreshToken: () => localStorage.getItem(REFRESH_TOKEN_KEY),

  setTokens: ({ accessToken, refreshToken }) => {
    if (accessToken) localStorage.setItem(ACCESS_TOKEN_KEY, accessToken)
    if (refreshToken) localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken)
    logoutReason = null
    notify()
  },

  clear: (reason) => {
    localStorage.removeItem(ACCESS_TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    if (reason) logoutReason = reason
    notify()
  },

  // Read-and-forget: the reason is only relevant to whoever just got logged out.
  consumeLogoutReason: () => {
    const reason = logoutReason
    logoutReason = null
    return reason
  },

  subscribe: (listener) => {
    listeners.add(listener)
    return () => listeners.delete(listener)
  }
}

// The access token is a JWT, so its expiry is readable client-side without a
// round trip. The refresh token is an opaque server-generated string and is
// only ever validated by the API.
export function getAccessTokenExpiry(token) {
  try {
    const decoded = jwtDecode(token)
    return typeof decoded.exp === 'number' ? decoded.exp : null
  } catch {
    return null
  }
}

export function isAccessTokenExpired(token) {
  const expiry = getAccessTokenExpiry(token)
  // No exp claim means the token isn't a JWT we can reason about — leave it
  // alone and let the API be the authority.
  if (expiry === null) return false
  return Date.now() / 1000 >= expiry
}

// Renew ahead of the server's deadline so an active user never sees a request
// fail on timing. The 401 retry in client.js is the backstop when this timer
// loses the race (backgrounded tab, sleep, clock skew).
export const REFRESH_LEEWAY_SECONDS = 30

export function isAccessTokenExpiringSoon(token, leeway = REFRESH_LEEWAY_SECONDS) {
  const expiry = getAccessTokenExpiry(token)
  if (expiry === null) return false
  return Date.now() / 1000 >= expiry - leeway
}
