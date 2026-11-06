import { createContext, useContext, useEffect, useState, useCallback } from 'react'
import { jwtDecode } from 'jwt-decode'
import { apiClient, refreshTokens } from '../api/client.js'
import { rbacApi } from '../api/rbacApi.js'
import {
  tokenStorage,
  getAccessTokenExpiry,
  isAccessTokenExpired,
  REFRESH_LEEWAY_SECONDS
} from '../api/tokenStorage.js'

const RESOURCES_KEY = 'lampFactory.resources'
const AuthContext = createContext(null)

// The JWT only carries id + name; permissions and roles stay server-side and
// are resolved per request. Decoding here deliberately reads nothing else.
function decodeUser(token) {
  try {
    const claims = jwtDecode(token)
    return {
      id: claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ?? claims.nameid ?? claims.sub,
      username: claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ?? claims.unique_name ?? claims.name
    }
  } catch {
    return null
  }
}

function readStoredResources() {
  try {
    const raw = localStorage.getItem(RESOURCES_KEY)
    const parsed = raw ? JSON.parse(raw) : null
    return Array.isArray(parsed) ? parsed : []
  } catch {
    return []
  }
}

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => tokenStorage.getAccessToken())
  const [user, setUser] = useState(() => {
    const existing = tokenStorage.getAccessToken()
    return existing ? decodeUser(existing) : null
  })
  // Seeded from storage so a page refresh doesn't flash the full sidebar before
  // the live query lands; refreshed on every login.
  const [resources, setResources] = useState(readStoredResources)
  // Tracks whether we've already attempted the post-login resource query, so a
  // user with legitimately zero accessible resources doesn't re-fetch forever.
  const [resourcesLoaded, setResourcesLoaded] = useState(() => localStorage.getItem(RESOURCES_KEY) !== null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)
  // True only during the silent restore that runs on startup, while we're
  // trading a stored refresh token in for a fresh access token. Lets
  // ProtectedRoute hold instead of bouncing to /login prematurely. Seeded
  // synchronously — if the access token is already dead but a refresh token is
  // stored, the restore below will run, and holding from the first render
  // avoids a login-page flash on every reload.
  const [restoring, setRestoring] = useState(() => {
    const accessToken = tokenStorage.getAccessToken()
    const refreshToken = tokenStorage.getRefreshToken()
    return !!refreshToken && (!accessToken || isAccessTokenExpired(accessToken))
  })

  // The axios interceptor renews and clears tokens from outside React (silent
  // 401 retry, forced logout on a reused refresh token). Subscribe so this
  // context tracks those writes instead of drifting from storage.
  useEffect(() => {
    return tokenStorage.subscribe(() => {
      const current = tokenStorage.getAccessToken()
      setToken((prev) => (prev !== current ? current : prev))

      // A forced logout carries the backend's explanation (expired vs. detected
      // reuse). Surface it on the login screen instead of failing silently.
      const reason = tokenStorage.consumeLogoutReason()
      if (reason) setError(reason)
    })
  }, [])

  useEffect(() => {
    if (token) {
      setUser(decodeUser(token))
    } else {
      localStorage.removeItem(RESOURCES_KEY)
      setUser(null)
      setResources([])
      setResourcesLoaded(false)
    }
  }, [token])

  // Startup restore. An access token lives ~5 minutes, so on most reloads the
  // stored one is already dead while the 7-day refresh token is still good.
  // Trade it in silently before the first protected request goes out.
  useEffect(() => {
    const accessToken = tokenStorage.getAccessToken()
    const refreshToken = tokenStorage.getRefreshToken()

    // Still on the clock, or nothing to trade in — nothing to do.
    if (!refreshToken || (accessToken && !isAccessTokenExpired(accessToken))) return

    let cancelled = false
    setRestoring(true)
    refreshTokens()
      .then((freshAccessToken) => {
        if (cancelled) return
        setToken(freshAccessToken)
      })
      .catch(() => {
        // Refresh was rejected (expired, rotated, or flagged as reused). The
        // interceptor already cleared storage; staying logged out is correct.
      })
      .finally(() => {
        if (!cancelled) setRestoring(false)
      })

    return () => {
      cancelled = true
    }
  }, [])

  // Proactive renewal: schedule a refresh shortly before the access token
  // lapses so an active user never sees a request fail. The 401 retry in
  // client.js is the backstop when this timer loses the race.
  useEffect(() => {
    if (!token) return
    const expiry = getAccessTokenExpiry(token)
    // No readable exp — leave it to the interceptor.
    if (expiry === null) return

    const delayMs = (expiry - REFRESH_LEEWAY_SECONDS) * 1000 - Date.now()
    if (delayMs <= 0) return

    const id = setTimeout(() => {
      refreshTokens().catch(() => {
        // A failed proactive renewal clears storage via the interceptor; the
        // subscription above then logs the UI out.
      })
    }, delayMs)

    return () => clearTimeout(id)
  }, [token])

  // A token can outlive the cached resource list — e.g. a session restored from
  // localStorage after the backend gained the /resource/my-accessible endpoint,
  // or a token issued before the user's roles changed. Re-query once whenever we
  // have a token but no cached resources to go with it.
  useEffect(() => {
    if (!token || resourcesLoaded) return
    let cancelled = false
    rbacApi.resources
      .getAccessible()
      .then((accessible) => {
        if (cancelled) return
        const names = (accessible ?? []).map((r) => r.name ?? r.Name).filter(Boolean)
        setResources(names)
        setResourcesLoaded(true)
        localStorage.setItem(RESOURCES_KEY, JSON.stringify(names))
      })
      .catch(() => {
        // Leave the previous state untouched; the login form surfaces its own errors.
        setResourcesLoaded(true)
      })
    return () => {
      cancelled = true
    }
  }, [token, resourcesLoaded])

  const login = useCallback(async (username, password) => {
    setLoading(true)
    setError(null)
    try {
      const data = await apiClient.post('/auth/login', { Username: username, Password: password })
      const accessToken = data?.accessToken ?? data?.AccessToken
      const refreshToken = data?.refreshToken ?? data?.RefreshToken

      if (!accessToken) throw new Error('The API did not return an access token.')

      // Persist both tokens before any further request goes out. The axios
      // request interceptor reads the access token from tokenStorage, and
      // setState alone does not flush it — only the subscription and the
      // effects above do, which run after re-render. Querying
      // /resource/my-accessible before this point would go out unauthenticated
      // and 401.
      tokenStorage.setTokens({ accessToken, refreshToken })
      setToken(accessToken)

      // Query the modules this account can actually see. The response interceptor
      // in client.js already unwrapped the APIResponse envelope, so this is the
      // bare resource list.
      const accessible = await rbacApi.resources.getAccessible()
      const names = (accessible ?? []).map((r) => r.name ?? r.Name).filter(Boolean)
      setResources(names)
      setResourcesLoaded(true)
      localStorage.setItem(RESOURCES_KEY, JSON.stringify(names))

      return true
    } catch (err) {
      setError(err.message || 'Login failed.')
      return false
    } finally {
      setLoading(false)
    }
  }, [])

  const logout = useCallback(() => {
    // Client-side only. The backend has no logout endpoint, so the refresh
    // token stays valid server-side until it expires or is rotated; see the
    // note in the summary about adding one.
    tokenStorage.clear()
  }, [])

  const hasAccess = useCallback(
    (resourceName) => (resourceName ? resources.includes(resourceName) : true),
    [resources]
  )

  const value = {
    token,
    user,
    resources,
    // An expired access token is not a session, even before the restore resolves.
    isAuthenticated: !!token && !isAccessTokenExpired(token),
    restoring,
    login,
    logout,
    loading,
    error,
    hasAccess
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within an AuthProvider')
  return ctx
}
