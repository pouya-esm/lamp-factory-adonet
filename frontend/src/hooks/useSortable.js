import { useState } from 'react'

// Three-state sort cycle per column: unsorted -> descending -> ascending -> unsorted.
// toggleSort(key) advances the cycle; applySort(items, accessors) returns the
// array sorted by the active column (accessors: { [sortKey]: (item) => value }).
// Sorting is stable enough for our lists: nulls always sink to the bottom.
export default function useSortable() {
  const [sort, setSort] = useState({ key: null, dir: null })

  function toggleSort(key) {
    setSort((prev) => {
      if (prev.key !== key || prev.dir === null) return { key, dir: 'desc' }
      if (prev.dir === 'desc') return { key, dir: 'asc' }
      return { key: null, dir: null }
    })
  }

  function applySort(items, accessors) {
    if (!items || !sort.key) return items
    const get = accessors[sort.key]
    if (!get) return items
    const sorted = [...items].sort((a, b) => {
      const va = get(a)
      const vb = get(b)
      if (va == null && vb == null) return 0
      if (va == null) return 1
      if (vb == null) return -1
      if (typeof va === 'number' && typeof vb === 'number') return va - vb
      return String(va).localeCompare(String(vb), undefined, { numeric: true, sensitivity: 'base' })
    })
    return sort.dir === 'desc' ? sorted.reverse() : sorted
  }

  return { sort, toggleSort, applySort }
}
