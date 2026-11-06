export default function Field({ label, required, error, children }) {
  return (
    <div className="mb-3.5">
      <label className="block text-sm font-medium text-steel-700 mb-1.5">
        {label} {required && <span className="text-stop-500">*</span>}
      </label>
      {children}
      {error && <p className="mt-1 text-xs text-stop-500">{error}</p>}
    </div>
  )
}

export const inputClass =
  'w-full rounded-md border border-panel-300 px-3 py-2 text-sm text-steel-900 ' +
  'placeholder:text-steel-500 focus:border-amber-500 focus:outline-none disabled:bg-panel-100'
