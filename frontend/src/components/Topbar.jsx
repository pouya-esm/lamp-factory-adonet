import { Menu } from 'lucide-react'

export default function Topbar({ title, onMenuClick }) {
  return (
    <header className="h-16 shrink-0 bg-white border-b border-panel-300 flex items-center gap-3 px-4 lg:px-6">
      <button
        onClick={onMenuClick}
        className="p-2 -ml-2 rounded-md text-steel-600 hover:bg-panel-100 lg:hidden"
        aria-label="Open navigation"
      >
        <Menu size={20} />
      </button>
      <h1 className="font-display font-semibold text-lg text-steel-900">{title}</h1>
    </header>
  )
}
