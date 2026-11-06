import { NavLink } from 'react-router-dom'
import { LogOut, Lightbulb } from 'lucide-react'
import { navSections } from './navConfig.js'
import { useAuth } from '../context/AuthContext.jsx'
import LineStatusStrip from './LineStatusStrip.jsx'

export default function Sidebar({ open, onClose }) {
  const { user, logout, hasAccess } = useAuth()

  // Drop items the user holds no permission on, then drop any section left empty.
  const visibleSections = navSections
    .map((section) => ({
      ...section,
      items: section.items.filter((item) => hasAccess(item.resource))
    }))
    .filter((section) => section.items.length > 0)

  return (
    <>
      {/* Mobile scrim */}
      {open && (
        <div
          className="fixed inset-0 z-30 bg-steel-950/60 lg:hidden"
          onClick={onClose}
          aria-hidden="true"
        />
      )}

      <aside
        className={`fixed z-40 inset-y-0 left-0 w-64 bg-steel-900 text-panel-100 flex flex-col
        transform transition-transform duration-200 lg:translate-x-0 lg:static lg:z-0
        ${open ? 'translate-x-0' : '-translate-x-full'}`}
      >
        <div className="flex items-center gap-2 px-4 h-16 border-b border-steel-700/60 shrink-0">
          <span className="flex h-8 w-8 items-center justify-center rounded bg-amber-500 text-steel-950">
            <Lightbulb size={18} strokeWidth={2.5} />
          </span>
          <div className="leading-tight">
            <p className="font-display font-semibold text-sm text-white">Lamp Factory</p>
            <p className="text-[11px] text-steel-500">SKD Production ERP</p>
          </div>
        </div>

        <nav className="flex-1 overflow-y-auto py-3">
          {visibleSections.map((section) => (
            <div key={section.label} className="mb-4">
              <p className="px-4 mb-1 text-[11px] uppercase tracking-wider text-steel-500 font-medium">
                {section.label}
              </p>
              <ul>
                {section.items.map(({ to, label, icon: Icon, end }) => (
                  <li key={to}>
                    <NavLink
                      to={to}
                      end={end}
                      onClick={onClose}
                      className={({ isActive }) =>
                        `mx-2 my-0.5 flex items-center gap-2.5 rounded-md px-2.5 py-2 text-sm transition-colors ${
                          isActive
                            ? 'bg-steel-700 text-white'
                            : 'text-steel-300 hover:bg-steel-800 hover:text-white'
                        }`
                      }
                    >
                      <Icon size={16} strokeWidth={2} className="shrink-0" />
                      <span>{label}</span>
                    </NavLink>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </nav>

        <LineStatusStrip />

        <div className="px-4 py-3 border-t border-steel-700/60 flex items-center justify-between">
          <div className="min-w-0">
            <p className="text-sm font-medium text-white truncate">{user?.username ?? 'Unknown user'}</p>
          </div>
          <button
            onClick={logout}
            title="Log out"
            className="p-2 rounded-md text-steel-400 hover:text-white hover:bg-steel-800 transition-colors"
          >
            <LogOut size={16} />
          </button>
        </div>
      </aside>
    </>
  )
}
