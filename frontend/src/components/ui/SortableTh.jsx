import { ChevronsUpDown, ChevronDown, ChevronUp } from 'lucide-react'

// Sortable table header cell. Render cycle:
//   unsorted  -> stacked arrows (ChevronsUpDown), tooltip "Sort descending"
//   click ->  -> arrow down, tooltip "Sort ascending"
//   click ->  -> arrow up,   tooltip "Sort initial"
//   click ->  -> back to unsorted (no sorting).
// Pair with the useSortable hook: sort={sort} onSort={toggleSort}.
export default function SortableTh({ label, sortKey, sort, onSort, className = '' }) {
  const state = sort?.key === sortKey ? sort.dir : null
  const tooltip =
    state === null ? 'Sort descending' : state === 'desc' ? 'Sort ascending' : 'Sort initial'
  const Icon = state === 'desc' ? ChevronDown : state === 'asc' ? ChevronUp : ChevronsUpDown

  return (
    <th className={`px-4 py-3 font-medium ${className}`}>
      <button
        type="button"
        onClick={() => onSort(sortKey)}
        title={tooltip}
        className="inline-flex items-center gap-1 uppercase tracking-wider text-xs text-steel-500 hover:text-steel-800"
      >
        {label}
        <Icon size={13} className={state ? 'text-amber-600' : 'text-steel-400'} />
      </button>
    </th>
  )
}
