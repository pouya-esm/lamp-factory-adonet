import { ChevronLeft, ChevronRight } from 'lucide-react'

function buildPageList(current, total) {
  if (total <= 7) {
    return Array.from({ length: total }, (_, i) => i + 1)
  }
  const wanted = new Set([1, 2, current - 1, current, current + 1, total - 1, total])
  const sorted = [...wanted].filter((p) => p >= 1 && p <= total).sort((a, b) => a - b)
  const out = []
  let prev = 0
  for (const p of sorted) {
    if (p - prev > 1) out.push('…')
    out.push(p)
    prev = p
  }
  return out
}

// Numbered pagination bar: "Showing x–y of z", page-size selector,
// prev/next and clickable page numbers with ellipsis for long ranges.
export default function Pagination({
  page,
  totalPages,
  totalItems,
  pageSize,
  onPageChange,
  onPageSizeChange,
  className = ''
}) {
  if (!totalItems) return null

  const first = (page - 1) * pageSize + 1
  const last = Math.min(page * pageSize, totalItems)
  const pages = buildPageList(page, totalPages)

  const btnBase =
    'inline-flex h-8 min-w-8 items-center justify-center rounded-md border text-sm ' +
    'border-panel-300 bg-white text-steel-700 hover:bg-panel-100 disabled:opacity-40 disabled:hover:bg-white'

  return (
    <div className={`flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 ${className}`}>
      <div className="flex items-center gap-3 text-sm text-steel-500">
        <span>
          Showing <span className="font-medium text-steel-700">{first}–{last}</span> of{' '}
          <span className="font-medium text-steel-700">{totalItems}</span>
        </span>
        <select
          value={pageSize}
          onChange={(e) => onPageSizeChange(Number(e.target.value))}
          className="rounded-md border border-panel-300 bg-white py-1.5 pl-2 pr-7 text-sm text-steel-700
            focus:border-amber-500 focus:outline-none"
          aria-label="Rows per page"
        >
          {[10, 25, 50, 100].map((size) => (
            <option key={size} value={size}>
              {size} / page
            </option>
          ))}
        </select>
      </div>

      {totalPages > 1 && (
        <div className="flex items-center gap-1">
          <button className={btnBase} onClick={() => onPageChange(page - 1)} disabled={page <= 1} aria-label="Previous page">
            <ChevronLeft size={16} />
          </button>
          {pages.map((p, i) =>
            p === '…' ? (
              <span key={`gap-${i}`} className="px-1 text-steel-400">
                …
              </span>
            ) : (
              <button
                key={p}
                onClick={() => onPageChange(p)}
                aria-label={`Page ${p}`}
                aria-current={p === page ? 'page' : undefined}
                className={`${btnBase} ${p === page ? 'border-amber-500 bg-amber-500 text-white hover:bg-amber-500' : ''}`}
              >
                {p}
              </button>
            )
          )}
          <button
            className={btnBase}
            onClick={() => onPageChange(page + 1)}
            disabled={page >= totalPages}
            aria-label="Next page"
          >
            <ChevronRight size={16} />
          </button>
        </div>
      )}
    </div>
  )
}
