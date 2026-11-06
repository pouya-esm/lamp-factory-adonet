import { useState, useMemo } from 'react'
import { Search } from 'lucide-react'
import Modal from './ui/Modal.jsx'
import Pagination from './ui/Pagination.jsx'
import SortableTh from './ui/SortableTh.jsx'
import { inputClass } from './ui/Field.jsx'
import usePagedList from '../hooks/usePagedList.js'
import useSortable from '../hooks/useSortable.js'
import { useProducts } from '../lib/useProducts.js'

function formatDate(isoString) {
  if (!isoString) return '—'
  return isoString.slice(0, 10)
}

function productLabel(id, products) {
  const p = products?.find((p) => p.productID === id)
  if (!p) return `Product ${id}`
  return p.standardNumber ? `${p.standardNumber} — ${p.material || ''}`.trim() : p.material || `Product ${id}`
}

// Modal selector for Production Documents (used by Line Stop / Quality Control
// forms). Searchable, sortable, paginated list mirroring the Production
// Documents module columns (#, Date, Product, Line, Produced, Rejected).
export default function ProductionDocumentPicker({ open, onClose, documents, selectedId, onSelect }) {
  const [search, setSearch] = useState('')
  const { products } = useProducts()

  const loading = documents == null
  const filtered = useMemo(() => {
    if (!documents) return []
    const q = search.trim().toLowerCase()
    if (!q) return documents
    return documents.filter((doc) =>
      [
        String(doc.productionDocumentID),
        doc.productionDate,
        productLabel(doc.productID, products),
        `Line ${doc.productionLineID}`
      ]
        .filter(Boolean)
        .some((v) => String(v).toLowerCase().includes(q))
    )
  }, [documents, search, products])

  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    id: (doc) => doc.productionDocumentID,
    date: (doc) => doc.productionDate,
    product: (doc) => productLabel(doc.productID, products),
    line: (doc) => doc.productionLineID,
    produced: (doc) => doc.producedQuantity,
    rejected: (doc) => doc.rejectedQuantity
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    sort
  ])

  function handleSelect(doc) {
    onSelect(doc)
    onClose()
  }

  return (
    <Modal open={open} onClose={onClose} title="Select a production document" maxWidth="max-w-5xl">
      <div className="mb-3">
        <div className="relative">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-steel-500" />
          <input
            className={`${inputClass} pl-9`}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search by #, date, product or line…"
          />
        </div>
      </div>

      <div className="border border-panel-300 rounded-md overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                <SortableTh label="#" sortKey="id" sort={sort} onSort={toggleSort} />
                <SortableTh label="Date" sortKey="date" sort={sort} onSort={toggleSort} />
                <SortableTh label="Product" sortKey="product" sort={sort} onSort={toggleSort} />
                <SortableTh label="Line" sortKey="line" sort={sort} onSort={toggleSort} />
                <SortableTh label="Produced" sortKey="produced" sort={sort} onSort={toggleSort} />
                <SortableTh label="Rejected" sortKey="rejected" sort={sort} onSort={toggleSort} />
                <th className="px-4 py-3 font-medium text-right w-24">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {loading &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={7} className="px-4 py-2">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {!loading && filtered.length === 0 && (
                <tr>
                  <td colSpan={7} className="px-4 py-8 text-center text-steel-500">
                    No production documents match your search.
                  </td>
                </tr>
              )}

              {!loading &&
                paged.map((doc) => {
                  const isSelected = selectedId != null && Number(doc.productionDocumentID) === Number(selectedId)
                  return (
                    <tr key={doc.productionDocumentID} className="hover:bg-panel-50">
                      <td className="px-4 py-2 font-mono text-steel-800">{doc.productionDocumentID}</td>
                      <td className="px-4 py-2 text-steel-700">{formatDate(doc.productionDate)}</td>
                      <td className="px-4 py-2 font-mono text-steel-700">{productLabel(doc.productID, products)}</td>
                      <td className="px-4 py-2 text-steel-700">Line {doc.productionLineID}</td>
                      <td className="px-4 py-2 text-steel-700">{doc.producedQuantity ?? '—'}</td>
                      <td className="px-4 py-2 text-steel-700">{doc.rejectedQuantity ?? '—'}</td>
                      <td className="px-4 py-2 text-right">
                        <button
                          type="button"
                          onClick={() => handleSelect(doc)}
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
