import { useAuth } from '../context/AuthContext.jsx'
import { navSections } from '../components/navConfig.js'

export default function DashboardPage() {
  const { user, hasAccess } = useAuth()

  const quickLinks = navSections
    .filter((s) => s.label !== 'Overview')
    .flatMap((s) => s.items)
    .filter((item) => hasAccess(item.resource))

  return (
    <div className="max-w-5xl">
      <p className="text-sm text-steel-500 mb-1">Welcome back{user?.username ? `, ${user.username}` : ''}</p>
      <h2 className="font-display font-semibold text-2xl text-steel-900 mb-6">Production overview</h2>

      <div className="rounded-lg border border-panel-300 bg-white p-4 mb-6 text-sm text-steel-600">
        This dashboard is a placeholder. Once we wire up live data, this is where we'll surface
        line status, open production plans, and pending quality holds at a glance.
      </div>

      <h3 className="text-xs uppercase tracking-wider text-steel-500 font-medium mb-3">Jump to a module</h3>
      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-3">
        {quickLinks.map(({ to, label, icon: Icon }) => (
          <a
            key={to}
            href={to}
            className="rounded-lg border border-panel-300 bg-white p-4 hover:border-amber-500 hover:shadow-sm transition-all"
          >
            <Icon size={18} className="text-line-600 mb-2" />
            <p className="text-sm font-medium text-steel-800">{label}</p>
          </a>
        ))}
      </div>
    </div>
  )
}
