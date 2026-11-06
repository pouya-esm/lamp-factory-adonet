import { Search, RotateCcw } from 'lucide-react'

// Reusable filter toolbar rendered at the top of every list page.
// - `search` / `onSearchChange`: free-text search box (omit to hide it)
// - `filters`: [{ name, label?, type: 'select'|'date', value, options?, placeholder }]
//     select options: [{ value, label }] — values are compared as strings by pages
//     date filters use `from`/`to` pairs supplied as two entries
// - `onFilterChange(name, value)`: called for every filter input
// - `actions`: right-aligned slot (usually the "New ..." button)
export default function FilterBar({
  search,
  onSearchChange,
  searchPlaceholder = 'Search…',
  filters = [],
  onFilterChange,
  onReset,
  actions = null
}) {
  const inputClass =
    'rounded-md border border-panel-300 bg-white py-2 text-sm text-steel-700 ' +
    'placeholder:text-steel-500 focus:border-amber-500 focus:outline-none'
  const hasValue = (f) => f.value !== '' && f.value != null
  const anyActive = (search && search.trim() !== '') || filters.some(hasValue)

  return (
    <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-3 mb-4">
      <div className="flex flex-col sm:flex-row sm:flex-wrap sm:items-center gap-2">
        {search !== undefined && (
          <div className="relative w-full sm:w-64">
            <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-steel-500" />
            <input
              value={search}
              onChange={(e) => onSearchChange(e.target.value)}
              placeholder={searchPlaceholder}
              title={searchPlaceholder}
              className={`w-full sm:w-64 transition-[width] duration-200 sm:hover:w-96 sm:focus-within:w-96 sm:focus:w-96 pl-9 pr-3 ${inputClass}`}
            />
          </div>
        )}

        {filters.map((f) =>
          f.type === 'select' ? (
            <select
              key={f.name}
              value={f.value}
              onChange={(e) => onFilterChange(f.name, e.target.value)}
              aria-label={f.label || f.name}
              className={`${inputClass} py-2 pl-3 pr-8`}
            >
              <option value="">{f.placeholder || 'All'}</option>
              {(f.options || []).map((opt) => (
                <option key={String(opt.value)} value={String(opt.value)}>
                  {opt.label}
                </option>
              ))}
            </select>
          ) : (
            <input
              key={f.name}
              type="date"
              value={f.value}
              onChange={(e) => onFilterChange(f.name, e.target.value)}
              aria-label={f.label || f.name}
              title={f.label || f.name}
              className={`${inputClass} px-3 ${hasValue(f) ? '' : 'text-steel-500'}`}
            />
          )
        )}

        {anyActive && onReset && (
          <button
            onClick={onReset}
            className="inline-flex items-center gap-1.5 rounded-md border border-panel-300 bg-white px-3 py-2
              text-sm text-steel-500 hover:bg-panel-100 hover:text-steel-800"
          >
            <RotateCcw size={14} />
            Reset
          </button>
        )}
      </div>

      {actions && <div className="flex items-center gap-2">{actions}</div>}
    </div>
  )
}
