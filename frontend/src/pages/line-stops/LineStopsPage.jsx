import { useEffect, useState, Fragment, useMemo } from 'react'
import { Plus, Pencil, Trash2, ChevronRight, ChevronDown } from 'lucide-react'
import { lineStopsApi } from '../../api/lineStopsApi.js'
import { useProductionDocuments } from '../../lib/useProductionDocuments.js'
import { useProducts } from '../../lib/useProducts.js'
import Button from '../../components/ui/Button.jsx'
import Modal from '../../components/ui/Modal.jsx'
import ConfirmDialog from '../../components/ui/ConfirmDialog.jsx'
import ErrorBanner from '../../components/ui/ErrorBanner.jsx'
import FilterBar from '../../components/FilterBar.jsx'
import Pagination from '../../components/ui/Pagination.jsx'
import SortableTh from '../../components/ui/SortableTh.jsx'
import usePagedList from '../../hooks/usePagedList.js'
import useSortable from '../../hooks/useSortable.js'
import AuditStamp from '../../components/AuditStamp.jsx'
import ProductDetailsModal from '../../components/ProductDetailsModal.jsx'
import LineStopForm from './LineStopForm.jsx'

function formatDate(isoString) {
  if (!isoString) return '—'
  return isoString.slice(0, 10)
}

function ProductionDocDetail({ doc, products, onViewProduct }) {
  if (!doc) return <p className="py-4 text-sm text-steel-500">Production document not found.</p>
  const product = products?.find((p) => p.productID === doc.productID)
  return (
    <div className="border border-panel-300 rounded-md overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
              <th className="px-4 py-2 font-medium">Field</th>
              <th className="px-4 py-2 font-medium">Value</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-panel-200">
            <tr>
              <td className="px-4 py-2 font-medium text-steel-500">Document ID</td>
              <td className="px-4 py-2 font-mono text-steel-800">{doc.productionDocumentID}</td>
            </tr>
            <tr>
              <td className="px-4 py-2 font-medium text-steel-500">Production Date</td>
              <td className="px-4 py-2 text-steel-700">{formatDate(doc.productionDate)}</td>
            </tr>
            <tr>
              <td className="px-4 py-2 font-medium text-steel-500">Produced Quantity</td>
              <td className="px-4 py-2 font-mono text-steel-700">{doc.producedQuantity ?? '—'}</td>
            </tr>
            <tr>
              <td className="px-4 py-2 font-medium text-steel-500">Rejected Quantity</td>
              <td className="px-4 py-2 font-mono text-steel-700">{doc.rejectedQuantity ?? '—'}</td>
            </tr>
            <tr>
              <td className="px-4 py-2 font-medium text-steel-500">Product</td>
              <td className="px-4 py-2 font-mono text-steel-700">
                <button
                  type="button"
                  onClick={() => onViewProduct?.(doc.productID)}
                  className="font-mono text-amber-600 hover:text-amber-700 hover:underline"
                  title="View product details"
                >
                  {product?.standardNumber || `Product ${doc.productID}`}
                </button>
              </td>
            </tr>
            <tr>
              <td className="px-4 py-2 font-medium text-steel-500">Production Line</td>
              <td className="px-4 py-2 font-mono text-steel-700">Line {doc.productionLineID}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  )
}

export default function LineStopsPage() {
  const [lineStops, setLineStops] = useState(null)
  const [loadError, setLoadError] = useState(null)
  const [search, setSearch] = useState('')
  const [filters, setFilters] = useState({ dateFrom: '', dateTo: '' })

  const [editingLineStop, setEditingLineStop] = useState(null)
  const [deletingLineStop, setDeletingLineStop] = useState(null)
  const [deleteLoading, setDeleteLoading] = useState(false)
  const [deleteError, setDeleteError] = useState(null)

  const [expandedId, setExpandedId] = useState(null)
  const [viewingProductId, setViewingProductId] = useState(null)
  const { documents } = useProductionDocuments()
  const { products } = useProducts()

  async function loadLineStops() {
    setLoadError(null)
    try {
      const data = await lineStopsApi.getAll()
      setLineStops(Array.isArray(data) ? data : [])
    } catch (err) {
      setLoadError(err.message || 'Failed to load line stops.')
    }
  }

  useEffect(() => {
    loadLineStops()
  }, [])

  function findDoc(id) {
    return documents?.find((d) => d.productionDocumentID === id)
  }

  function formatProductionDoc(id) {
    const doc = findDoc(id)
    if (!doc) return `PD #${id}`
    const product = products?.find((p) => p.productID === doc.productID)
    const date = doc.productionDate ? doc.productionDate.slice(0, 10) : '—'
    const std = product?.standardNumber || '—'
    return `${std} — ${date} (Line ${doc.productionLineID})`
  }

  function toggleExpand(id) {
    setExpandedId((prev) => (prev === id ? null : id))
  }

  const filtered = useMemo(() => {
    if (!lineStops) return []
    let list = lineStops
    const q = search.trim().toLowerCase()
    if (q) {
      list = list.filter((ls) =>
         [String(ls.lineStopID), String(ls.productionDocumentID), ls.reason, formatProductionDoc(ls.productionDocumentID)]
          .filter(Boolean)
          .some((v) => v.toLowerCase().includes(q))
      )
    }
    const date = (ls) => (ls.lineStopDate || '').slice(0, 10)
    if (filters.dateFrom) list = list.filter((ls) => date(ls) >= filters.dateFrom)
    if (filters.dateTo) list = list.filter((ls) => date(ls) <= filters.dateTo)
    return list
  }, [lineStops, search, filters, documents, products])

  const isFiltered = search.trim() !== '' || filters.dateFrom !== '' || filters.dateTo !== ''
  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    id: (ls) => ls.lineStopID,
    date: (ls) => ls.lineStopDate,
    doc: (ls) => ls.productionDocumentID,
    reason: (ls) => ls.reason
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    filters,
    sort,
    documents,
    products
  ])

  async function handleSave(payload) {
    if (editingLineStop?.lineStopID) {
      await lineStopsApi.update(editingLineStop.lineStopID, payload)
    } else {
      await lineStopsApi.create(payload)
    }
    setEditingLineStop(null)
    await loadLineStops()
  }

  async function handleDelete() {
    setDeleteLoading(true)
    setDeleteError(null)
    try {
      await lineStopsApi.remove(deletingLineStop.lineStopID)
      setDeletingLineStop(null)
      await loadLineStops()
    } catch (err) {
      setDeleteError(err.message || 'Failed to delete line stop.')
    } finally {
      setDeleteLoading(false)
    }
  }

  return (
    <div>
      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search by #, document or reason…"
        filters={[
          { name: 'dateFrom', label: 'Date from', type: 'date', value: filters.dateFrom },
          { name: 'dateTo', label: 'Date to', type: 'date', value: filters.dateTo }
        ]}
        onFilterChange={(name, value) => setFilters((prev) => ({ ...prev, [name]: value }))}
        onReset={() => {
          setSearch('')
          setFilters({ dateFrom: '', dateTo: '' })
        }}
        actions={
          <Button onClick={() => setEditingLineStop({})}>
            <Plus size={16} />
            New line stop
          </Button>
        }
      />

      <ErrorBanner message={loadError} />

      <div className="rounded-lg border border-panel-300 bg-white overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                <th className="px-4 py-3 font-medium w-10" />
                <SortableTh label="#" sortKey="id" sort={sort} onSort={toggleSort} />
                <SortableTh label="Date" sortKey="date" sort={sort} onSort={toggleSort} />
                <SortableTh label="Production Doc" sortKey="doc" sort={sort} onSort={toggleSort} />
                <SortableTh label="Reason" sortKey="reason" sort={sort} onSort={toggleSort} />
                <th className="px-4 py-3 font-medium">Last change</th>
                <th className="px-4 py-3 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {lineStops === null &&
                 [1, 2, 3].map((i) => (
                   <tr key={i}>
                     <td colSpan={7} className="px-4 py-3">
                       <div className="h-4 rounded bg-panel-100 animate-pulse" />
                     </td>
                   </tr>
                 ))}

              {lineStops !== null && filtered.length === 0 && (
                 <tr>
                   <td colSpan={7} className="px-4 py-10 text-center text-steel-500">
                     {isFiltered ? 'No line stops match your filters.' : 'No line stops recorded yet.'}
                   </td>
                 </tr>
               )}

               {paged?.map((ls) => {
                 const id = ls.lineStopID
                 const isExpanded = expandedId === id
                 return (
                   <Fragment key={id}>
                     <tr
                       onClick={() => toggleExpand(id)}
                       className="hover:bg-panel-50 cursor-pointer"
                     >
                       <td className="px-4 py-3">
                         <span className="text-steel-400">
                           {isExpanded ? <ChevronDown size={16} /> : <ChevronRight size={16} />}
                         </span>
                       </td>
                       <td className="px-4 py-3 font-mono text-steel-800">{id}</td>
                       <td className="px-4 py-3 text-steel-700">{formatDate(ls.lineStopDate)}</td>
                       <td className="px-4 py-3 font-mono text-steel-700">{formatProductionDoc(ls.productionDocumentID)}</td>
                       <td className="px-4 py-3 text-steel-700 max-w-md truncate" title={ls.reason || ''}>
                         {ls.reason || '—'}
                       </td>
                       <AuditStamp
                         createdAt={ls.createdAt}
                         createdBy={ls.createdBy}
                         modifiedAt={ls.modifiedAt}
                         modifiedBy={ls.modifiedBy}
                       />
                       <td className="px-4 py-3">
                         <div className="flex items-center justify-end gap-1" onClick={(e) => e.stopPropagation()}>
                           <button
                             onClick={() => setEditingLineStop(ls)}
                             className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
                             aria-label={`Edit line stop ${ls.lineStopID}`}
                             title="Edit"
                           >
                             <Pencil size={16} />
                           </button>
                           <button
                             onClick={() => setDeletingLineStop(ls)}
                             className="p-1.5 rounded-md text-steel-500 hover:bg-stop-500/10 hover:text-stop-500"
                             aria-label={`Delete line stop ${ls.lineStopID}`}
                             title="Delete"
                           >
                             <Trash2 size={16} />
                           </button>
                         </div>
                       </td>
                     </tr>

                     {isExpanded && (
                       <tr className="bg-panel-50">
                         <td colSpan={7} className="px-4 pb-4 pt-0">
                           <ProductionDocDetail
                             doc={findDoc(ls.productionDocumentID)}
                             products={products}
                             onViewProduct={setViewingProductId}
                           />
                         </td>
                       </tr>
                     )}
                   </Fragment>
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
        className="mt-4"
      />

      <Modal
        open={!!editingLineStop}
        onClose={() => setEditingLineStop(null)}
        title={editingLineStop?.lineStopID ? 'Edit line stop' : 'New line stop'}
      >
        {editingLineStop && (
          <LineStopForm
            lineStop={editingLineStop.lineStopID ? editingLineStop : null}
            onSubmit={handleSave}
            onCancel={() => setEditingLineStop(null)}
          />
        )}
      </Modal>

      <ConfirmDialog
        open={!!deletingLineStop}
        title="Delete line stop"
        message={`Delete line stop #${deletingLineStop?.lineStopID}? This can't be undone.`}
        onCancel={() => {
          setDeletingLineStop(null)
          setDeleteError(null)
        }}
        onConfirm={handleDelete}
        loading={deleteLoading}
      />
      {deleteError && <p className="mt-2 text-sm text-stop-500">{deleteError}</p>}

      <ProductDetailsModal
        productId={viewingProductId}
        open={viewingProductId != null}
        onClose={() => setViewingProductId(null)}
      />
    </div>
  )
}
