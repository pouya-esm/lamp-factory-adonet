import { Construction } from 'lucide-react'

export default function PlaceholderPage({ title }) {
  return (
    <div className="flex flex-col items-center justify-center text-center py-24 rounded-lg border border-dashed border-panel-300 bg-white">
      <span className="flex h-12 w-12 items-center justify-center rounded-full bg-panel-100 text-steel-500 mb-4">
        <Construction size={22} />
      </span>
      <h2 className="font-display font-semibold text-lg text-steel-800 mb-1">{title}</h2>
      <p className="text-sm text-steel-500 max-w-sm">
        This module's screen hasn't been built yet. It'll follow the same layout,
        table, and form pattern as the rest of the app once we get to it.
      </p>
    </div>
  )
}
