import { useState, useMemo } from 'react'
import { Search } from 'lucide-react'
import Modal from './ui/Modal.jsx'
import Pagination from './ui/Pagination.jsx'
import SortableTh from './ui/SortableTh.jsx'
import { inputClass } from './ui/Field.jsx'
import usePagedList from '../hooks/usePagedList.js'
import useSortable from '../hooks/useSortable.js'
import { WAREHOUSE_TYPE_LABELS } from '../pages/warehouses/warehouseTypes.js'

function typeLabel(type) {
  return WAREHOUSE_TYPE_LABELS[type] ?? 'Unknown'
}

// Modal selector for Warehouses. Searchable, sortable, paginated list mirroring
// the Warehouses module columns (ID, Type, Name, Address). Used by the voucher
// forms (Charge / Request / Sales / Purchase) in place of a plain <select>.
export default function WarehousePicker({ open, onClose, warehouses, selectedId, onSelect, hideType }) {
  const [search, setSearch] = useState('')

  const loading = warehouses == null
  const filtered = useMemo(() => {
    if (!warehouses) return []
    const q = search.trim().toLowerCase()
    if (!q) return warehouses
    return warehouses.filter((w) =>
      [
        String(w.warehouseID),
        typeLabel(w.warehouseType),
        w.name,
        w.address
      ]
        .filter(Boolean)
        .some((v) => String(v).toLowerCase().includes(q))
    )
  }, [warehouses, search])

  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    id: (w) => w.warehouseID,
    type: (w) => typeLabel(w.warehouseType),
    name: (w) => w.name,
    address: (w) => w.address
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    sort
  ])

  const colSpan = hideType ? 4 : 5

  function handleSelect(w) {
    onSelect(w)
    onClose()
  }

  return (
    <Modal open={open} onClose={onClose} title="Select a warehouse" maxWidth="max-w-4xl">
      <div className="mb-3">
        <div className="relative">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-steel-500" />
          <input
            className={`${inputClass} pl-9`}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search by ID, name or address…"
          />
        </div>
      </div>

      <div className="border border-panel-300 rounded-md overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                <SortableTh label="#" sortKey="id" sort={sort} onSort={toggleSort} />
                {!hideType && <SortableTh label="Type" sortKey="type" sort={sort} onSort={toggleSort} />}
                <SortableTh label="Name" sortKey="name" sort={sort} onSort={toggleSort} />
                <SortableTh label="Address" sortKey="address" sort={sort} onSort={toggleSort} />
                <th className="px-4 py-3 font-medium text-right w-24">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {loading &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={colSpan} className="px-4 py-2">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {!loading && filtered.length === 0 && (
                <tr>
                  <td colSpan={colSpan} className="px-4 py-8 text-center text-steel-500">
                    No warehouses match your search.
                  </td>
                </tr>
              )}

              {!loading &&
                paged.map((w) => {
                  const isSelected = selectedId != null && Number(w.warehouseID) === Number(selectedId)
                  return (
                    <tr key={w.warehouseID} className="hover:bg-panel-50">
                      <td className="px-4 py-2 font-mono text-steel-800">{w.warehouseID}</td>
                      {!hideType && <td className="px-4 py-2 text-steel-700">{typeLabel(w.warehouseType)}</td>}
                      <td className="px-4 py-2 text-steel-700">{w.name || '—'}</td>
                      <td className="px-4 py-2 text-steel-700">{w.address || '—'}</td>
                      <td className="px-4 py-2 text-right">
                        <button
                          type="button"
                          onClick={() => handleSelect(w)}
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
