import { useState, useMemo } from 'react'
import { Search } from 'lucide-react'
import Modal from './ui/Modal.jsx'
import Pagination from './ui/Pagination.jsx'
import SortableTh from './ui/SortableTh.jsx'
import { inputClass } from './ui/Field.jsx'
import usePagedList from '../hooks/usePagedList.js'
import useSortable from '../hooks/useSortable.js'
import { lineStatusMeta } from '../pages/production-lines/lineStatus.js'

export default function ProductionLinePicker({ open, onClose, lines, selectedId, onSelect }) {
  const [search, setSearch] = useState('')

  const filtered = useMemo(() => {
    if (!lines) return []
    const q = search.trim().toLowerCase()
    if (!q) return lines
    return lines.filter((l) =>
      [String(l.productionLineID), lineStatusMeta(l.status).label]
        .filter(Boolean)
        .some((v) => String(v).toLowerCase().includes(q))
    )
  }, [lines, search])

  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    id: (l) => l.productionLineID,
    // Map the boolean to 0/1 so numeric sorting keeps Stopped before Running.
    status: (l) => (l.status === true ? 1 : 0)
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    sort
  ])

  function handleSelect(l) {
    onSelect(l)
    onClose()
  }

  return (
    <Modal open={open} onClose={onClose} title="Select a production line" maxWidth="max-w-3xl">
      <div className="mb-3">
        <div className="relative">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-steel-500" />
          <input
            className={`${inputClass} pl-9`}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search by # or status…"
          />
        </div>
      </div>

      <div className="border border-panel-300 rounded-md overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                <SortableTh label="#" sortKey="id" sort={sort} onSort={toggleSort} />
                <SortableTh label="Line" sortKey="id" sort={sort} onSort={toggleSort} />
                <SortableTh label="Status" sortKey="status" sort={sort} onSort={toggleSort} />
                <th className="px-4 py-3 font-medium text-right w-24">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {!lines &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={4} className="px-4 py-2">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {lines && filtered.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-4 py-8 text-center text-steel-500">
                    No production lines match your search.
                  </td>
                </tr>
              )}

              {lines &&
                paged.map((l) => {
                  const isSelected = selectedId != null && Number(l.productionLineID) === Number(selectedId)
                  const meta = lineStatusMeta(l.status)
                  return (
                    <tr key={l.productionLineID} className="hover:bg-panel-50">
                      <td className="px-4 py-2 font-mono text-steel-800">{l.productionLineID}</td>
                      <td className="px-4 py-2 font-mono text-steel-700">Line {l.productionLineID}</td>
                      <td className="px-4 py-2">
                        <span
                          className={`inline-flex items-center gap-1.5 rounded px-2 py-0.5 text-xs font-medium ${meta.badge}`}
                        >
                          <span className={`h-1.5 w-1.5 rounded-full ${meta.dot}`} />
                          {meta.label}
                        </span>
                      </td>
                      <td className="px-4 py-2 text-right">
                        <button
                          type="button"
                          onClick={() => handleSelect(l)}
                          disabled={isSelected}
                          className="px-3 py-1.5 rounded-md border border-panel-300 bg-white text-sm text-steel-700 hover:bg-panel-100 disabled:opacity-40 disabled:hover:bg-white"
                        >
                          {isSelected ? 'Selected' : 'Select'}
                        </button>
                      </td>
                    </tr>
                  )
                })}
            </tbody>
          </table>
        </div>
      </div>

      <Pagination
        page={page}
        totalPages={totalPages}
        totalItems={totalItems}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        className="mt-3"
      />
    </Modal>
  )
}
