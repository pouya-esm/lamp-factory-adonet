import { useState } from 'react'
import { Outlet, useLocation } from 'react-router-dom'
import Sidebar from './Sidebar.jsx'
import Topbar from './Topbar.jsx'
import { allNavItems } from './navConfig.js'
import { useAuth } from '../context/AuthContext.jsx'

export default function Layout() {
  const [menuOpen, setMenuOpen] = useState(false)
  const location = useLocation()
  const { hasAccess } = useAuth()

  const current = allNavItems.find(
    (item) =>
      (item.end ? location.pathname === item.to : location.pathname.startsWith(item.to)) &&
      hasAccess(item.resource)
  )

  return (
    <div className="h-screen flex bg-panel-50">
      <Sidebar open={menuOpen} onClose={() => setMenuOpen(false)} />
      <div className="flex-1 flex flex-col min-w-0">
        <Topbar title={current?.label ?? 'Lamp Factory'} onMenuClick={() => setMenuOpen(true)} />
        <main className="flex-1 overflow-y-auto p-4 lg:p-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
