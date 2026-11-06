import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext.jsx'
import { allNavItems } from './navConfig.js'

export default function ProtectedRoute({ children }) {
  const { isAuthenticated, hasAccess, restoring } = useAuth()
  const location = useLocation()

  // A stored refresh token may still be good even though the access token has
  // lapsed. While that swap is in flight, hold here rather than bouncing to
  // /login and back — otherwise every reload after ~5 minutes would flash the
  // login screen.
  if (restoring) {
    return <div className="min-h-screen bg-steel-950" aria-busy="true" />
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  // Direct-URL guard: find the nav item backing this path and, when it is tied to
  // a resource the user can't reach, send them to the first module they can.
  // This is UI hygiene only — the API still enforces [HasPermission] on every
  // endpoint, so this can never grant access to data.
  const current = allNavItems.find((item) =>
    item.end ? location.pathname === item.to : location.pathname.startsWith(item.to)
  )

  if (current && !hasAccess(current.resource)) {
    const fallback = allNavItems.find((item) => hasAccess(item.resource))
    return <Navigate to={fallback ? fallback.to : '/login'} replace />
  }

  return children
}
