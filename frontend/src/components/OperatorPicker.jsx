import { useState, useMemo } from 'react'
import { Search } from 'lucide-react'
import Modal from './ui/Modal.jsx'
import Pagination from './ui/Pagination.jsx'
import SortableTh from './ui/SortableTh.jsx'
import { inputClass } from './ui/Field.jsx'
import usePagedList from '../hooks/usePagedList.js'
import useSortable from '../hooks/useSortable.js'
import { operatorName } from '../lib/useOperators.js'

function formatDate(isoString) {
  if (!isoString) return '—'
  return isoString.slice(0, 10)
}

export default function OperatorPicker({ open, onClose, operators, selectedId, onSelect, multiSelect = false }) {
  const [search, setSearch] = useState('')

  const filtered = useMemo(() => {
    if (!operators) return []
    const q = search.trim().toLowerCase()
    if (!q) return operators
    return operators.filter((op) =>
      [String(op.operatorID), operatorName(op), op.nationalID]
        .filter(Boolean)
        .some((v) => String(v).toLowerCase().includes(q))
    )
  }, [operators, search])

  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    id: (op) => op.operatorID,
    name: (op) => operatorName(op),
    nationalID: (op) => op.nationalID ?? '',
    hiringDate: (op) => op.hiringDate ?? ''
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    sort
  ])

  function handleSelect(op) {
    onSelect(op)
    onClose()
  }

  return (
    <Modal open={open} onClose={onClose} title="Select an operator" maxWidth="max-w-3xl">
      <div className="mb-3">
        <div className="relative">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-steel-500" />
          <input
            className={`${inputClass} pl-9`}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search by #, name or national ID…"
          />
        </div>
      </div>

      <div className="border border-panel-300 rounded-md overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                <SortableTh label="#" sortKey="id" sort={sort} onSort={toggleSort} />
                <SortableTh label="Name" sortKey="name" sort={sort} onSort={toggleSort} />
                <SortableTh label="National ID" sortKey="nationalID" sort={sort} onSort={toggleSort} />
                <SortableTh label="Hiring Date" sortKey="hiringDate" sort={sort} onSort={toggleSort} />
                <th className="px-4 py-3 font-medium text-right w-24">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {!operators &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={5} className="px-4 py-2">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {operators && filtered.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-8 text-center text-steel-500">
                    No operators match your search.
                  </td>
                </tr>
              )}

              {operators &&
                paged.map((op) => {
                  const isSelected = selectedId != null && Number(op.operatorID) === Number(selectedId)
                  return (
                    <tr key={op.operatorID} className="hover:bg-panel-50">
                      <td className="px-4 py-2 font-mono text-steel-800">{op.operatorID}</td>
                      <td className="px-4 py-2 text-steel-700">{operatorName(op) || '—'}</td>
                      <td className="px-4 py-2 text-steel-700">{op.nationalID || '—'}</td>
                      <td className="px-4 py-2 text-steel-700">{formatDate(op.hiringDate)}</td>
                      <td className="px-4 py-2 text-right">
                        <button
                          type="button"
                          onClick={() => handleSelect(op)}
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
