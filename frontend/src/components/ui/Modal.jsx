import { X } from 'lucide-react'

export default function Modal({ open, onClose, title, children, footer, wide = false, maxWidth = '' }) {
  if (!open) return null

  return (
    <div className="fixed inset-0 z-50 flex items-start sm:items-center justify-center p-4 overflow-y-auto">
      <div className="fixed inset-0 bg-steel-950/60" onClick={onClose} aria-hidden="true" />
      <div
        role="dialog"
        aria-modal="true"
        className={`relative w-full ${maxWidth || (wide ? 'max-w-2xl' : 'max-w-md')} bg-white rounded-xl shadow-xl my-8`}
      >
        <div className="flex items-center justify-between px-5 py-4 border-b border-panel-200">
          <h2 className="font-display font-semibold text-base text-steel-900">{title}</h2>
          <button
            onClick={onClose}
            className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
            aria-label="Close"
          >
            <X size={18} />
          </button>
        </div>
        <div className="px-5 py-4">{children}</div>
        {footer && <div className="flex items-center justify-end gap-2 px-5 py-4 border-t border-panel-200">{footer}</div>}
      </div>
    </div>
  )
}
