import { TriangleAlert } from 'lucide-react'

export default function ErrorBanner({ message }) {
  if (!message) return null
  return (
    <div className="flex items-start gap-2 rounded-md bg-stop-500/10 border border-stop-500/40 px-3 py-2 text-sm text-stop-500 mb-4">
      <TriangleAlert size={16} className="shrink-0 mt-0.5" />
      <span>{message}</span>
    </div>
  )
}
