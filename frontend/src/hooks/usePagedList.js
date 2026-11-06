import { useEffect, useMemo, useState } from 'react'

// Client-side pagination over an already-filtered array. The backend
// returns full lists, so pages are sliced here; resetDeps should contain
// every value that, when changed, must send the user back to page 1
// (search text, filter selections, reloaded data length, ...).
export default function usePagedList(items, resetDeps = [], defaultPageSize = 10) {
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(defaultPageSize)

  useEffect(() => {
    setPage(1)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, resetDeps)

  const totalItems = items?.length ?? 0
  const totalPages = Math.max(1, Math.ceil(totalItems / pageSize))
  const safePage = Math.min(page, totalPages)

  const paged = useMemo(() => {
    if (!items) return []
    const start = (safePage - 1) * pageSize
    return items.slice(start, start + pageSize)
  }, [items, safePage, pageSize])

  return {
    paged,
    page: safePage,
    setPage,
    pageSize,
    setPageSize: (size) => {
      setPageSize(size)
      setPage(1)
    },
    totalPages,
    totalItems
  }
}
