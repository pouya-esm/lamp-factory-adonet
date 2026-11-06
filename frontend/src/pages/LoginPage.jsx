import { useState } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { Lightbulb, TriangleAlert } from 'lucide-react'
import { useAuth } from '../context/AuthContext.jsx'

export default function LoginPage() {
  const { isAuthenticated, login, loading, error } = useAuth()
  const location = useLocation()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')

  if (isAuthenticated) {
    const redirectTo = location.state?.from?.pathname ?? '/'
    return <Navigate to={redirectTo} replace />
  }

  async function handleSubmit(e) {
    e.preventDefault()
    if (!username || !password) return
    await login(username, password)
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-steel-950 px-4">
      <div className="w-full max-w-sm">
        <div className="flex flex-col items-center mb-8">
          <span className="flex h-12 w-12 items-center justify-center rounded-lg bg-amber-500 text-steel-950 mb-3">
            <Lightbulb size={26} strokeWidth={2.5} />
          </span>
          <h1 className="font-display font-semibold text-xl text-white">Lamp Factory</h1>
          <p className="text-sm text-steel-500">SKD Production ERP</p>
        </div>

        <form onSubmit={handleSubmit} className="bg-steel-900 rounded-xl p-6 space-y-4 border border-steel-700/60">
          <div>
            <label htmlFor="username" className="block text-sm font-medium text-steel-100 mb-1.5">
              Username
            </label>
            <input
              id="username"
              type="text"
              autoComplete="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="w-full rounded-md bg-steel-800 border border-steel-600 px-3 py-2 text-sm text-white
                placeholder:text-steel-300 focus:border-amber-500 focus:outline-none"
              placeholder="jane.operator"
            />
          </div>

          <div>
            <label htmlFor="password" className="block text-sm font-medium text-steel-100 mb-1.5">
              Password
            </label>
            <input
              id="password"
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full rounded-md bg-steel-800 border border-steel-600 px-3 py-2 text-sm text-white
                placeholder:text-steel-300 focus:border-amber-500 focus:outline-none"
              placeholder="••••••••"
            />
          </div>

          {error && (
            <div className="flex items-start gap-2 rounded-md bg-stop-500/10 border border-stop-500/40 px-3 py-2 text-sm text-stop-400">
              <TriangleAlert size={16} className="shrink-0 mt-0.5" />
              <span>{error}</span>
            </div>
          )}

          <button
            type="submit"
            disabled={loading}
            className="w-full rounded-md bg-amber-500 hover:bg-amber-600 disabled:opacity-60
              disabled:cursor-not-allowed text-steel-950 font-medium text-sm py-2.5 transition-colors"
          >
            {loading ? 'Signing in…' : 'Sign in'}
          </button>
        </form>
      </div>
    </div>
  )
}
