import { useEffect, useState, Fragment, useMemo } from 'react'
import { Plus, Pencil, Trash2, Eye, ChevronRight, ChevronDown } from 'lucide-react'
import { qualityControlApi } from '../../api/qualityControlApi.js'
import { QC_TYPE_LABELS, QC_TYPE_OPTIONS } from './qualityControlTypes.js'
import { useProductionDocuments } from '../../lib/useProductionDocuments.js'
import { useProducts } from '../../lib/useProducts.js'
import { shiftLabel } from '../production-documents/shifts.js'
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
import ProductionPlanDetailsModal from '../../components/ProductionPlanDetailsModal.jsx'
import QualityControlForm from './QualityControlForm.jsx'

function formatDate(isoString) {
  if (!isoString) return '—'
  return isoString.slice(0, 10)
}

function TypeBadge({ type }) {
  const label = QC_TYPE_LABELS[type] ?? 'Unknown'
  const isFinal = type === 0
  return (
    <span
      className={`inline-flex items-center rounded px-2 py-0.5 text-xs font-medium ${
        isFinal ? 'bg-line-500/10 text-line-600' : 'bg-amber-500/10 text-amber-600'
      }`}
    >
      {label}
    </span>
  )
}

// Backend stores QualityControlAcceptance as a nullable BIT: 1 = true, 0 = false.
// NULL (no verdict yet) renders as an em dash.
function AcceptanceBadge({ acceptance }) {
  if (acceptance === null || acceptance === undefined) {
    return <span className="text-steel-400">—</span>
  }
  const accepted = acceptance === true
  return (
    <span
      className={`inline-flex items-center rounded px-2 py-0.5 text-xs font-medium ${
        accepted ? 'bg-line-500/10 text-line-600' : 'bg-stop-500/10 text-stop-500'
      }`}
    >
      {accepted ? 'True' : 'False'}
    </span>
  )
}

function ProductionDocDetail({ doc, products, onViewProduct, onViewPlan }) {
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
              <td className="px-4 py-2 font-medium text-steel-500">Production Shift</td>
              <td className="px-4 py-2 text-steel-700">{shiftLabel(doc.productionShift)}</td>
            </tr>
            <tr>
              <td className="px-4 py-2 font-medium text-steel-500">Production Plan ID</td>
              <td className="px-4 py-2 font-mono text-steel-700">
                <button
                  type="button"
                  onClick={() => onViewPlan?.(doc.productionPlanID)}
                  className="font-mono text-amber-600 hover:text-amber-700 hover:underline"
                  title="View production plan details"
                >
                  {doc.productionPlanID}
                </button>
              </td>
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

export default function QualityControlPage() {
  const [records, setRecords] = useState(null)
  const [loadError, setLoadError] = useState(null)
  const [search, setSearch] = useState('')
  const [filters, setFilters] = useState({ type: '', dateFrom: '', dateTo: '' })

  const [editingRecord, setEditingRecord] = useState(null)
  const [deletingRecord, setDeletingRecord] = useState(null)
  const [deleteLoading, setDeleteLoading] = useState(false)
  const [deleteError, setDeleteError] = useState(null)
  const [expandedId, setExpandedId] = useState(null)
  const [viewingProductId, setViewingProductId] = useState(null)
  const [viewingPlanId, setViewingPlanId] = useState(null)
  const { documents } = useProductionDocuments()
  const { products } = useProducts()

  async function loadRecords() {
    setLoadError(null)
    try {
      const data = await qualityControlApi.getAll()
      setRecords(Array.isArray(data) ? data : [])
    } catch (err) {
      setLoadError(err.message || 'Failed to load quality control records.')
    }
  }

  useEffect(() => {
    loadRecords()
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
    if (!records) return []
    let list = records
    const q = search.trim().toLowerCase()
    if (q) {
      list = list.filter((qc) =>
        [String(qc.qualityControlID), qc.inspectionType, qc.batchNumber, formatProductionDoc(qc.productionDocumentID)]
          .filter(Boolean)
          .some((v) => v.toLowerCase().includes(q))
      )
    }
    if (filters.type !== '') {
      list = list.filter((qc) => String(qc.qualityControlType) === filters.type)
    }
    const date = (qc) => (qc.qualityControlDate || '').slice(0, 10)
    if (filters.dateFrom) list = list.filter((qc) => date(qc) >= filters.dateFrom)
    if (filters.dateTo) list = list.filter((qc) => date(qc) <= filters.dateTo)
    return list
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [records, search, filters, documents, products])

  const isFiltered =
    search.trim() !== '' || filters.type !== '' || filters.dateFrom !== '' || filters.dateTo !== ''
  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    id: (qc) => qc.qualityControlID,
    type: (qc) => qc.qualityControlType,
    date: (qc) => qc.qualityControlDate,
    inspection: (qc) => qc.inspectionType,
    batch: (qc) => qc.batchNumber,
    sampleRatio: (qc) => qc.sampleRatio,
    acceptance: (qc) => qc.qualityControlAcceptance,
    doc: (qc) => formatProductionDoc(qc.productionDocumentID)
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
    if (editingRecord?.qualityControlID) {
      await qualityControlApi.update(editingRecord.qualityControlID, payload)
    } else {
      await qualityControlApi.create(payload)
    }
    setEditingRecord(null)
    await loadRecords()
  }

  async function handleDelete() {
    setDeleteLoading(true)
    setDeleteError(null)
    try {
      await qualityControlApi.remove(deletingRecord.qualityControlID)
      setDeletingRecord(null)
      await loadRecords()
    } catch (err) {
      setDeleteError(err.message || 'Failed to delete quality control record.')
    } finally {
      setDeleteLoading(false)
    }
  }

  return (
    <div>
      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search by #, inspection type, batch or production doc…"
        filters={[
          {
            name: 'type',
            label: 'Type',
            type: 'select',
            value: filters.type,
            placeholder: 'All types',
            options: QC_TYPE_OPTIONS
          },
          { name: 'dateFrom', label: 'Date from', type: 'date', value: filters.dateFrom },
          { name: 'dateTo', label: 'Date to', type: 'date', value: filters.dateTo }
        ]}
        onFilterChange={(name, value) => setFilters((prev) => ({ ...prev, [name]: value }))}
        onReset={() => {
          setSearch('')
          setFilters({ type: '', dateFrom: '', dateTo: '' })
        }}
        actions={
          <Button onClick={() => setEditingRecord({})}>
            <Plus size={16} />
            New QC record
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
                <SortableTh label="Type" sortKey="type" sort={sort} onSort={toggleSort} />
                <SortableTh label="Date" sortKey="date" sort={sort} onSort={toggleSort} />
                <SortableTh label="Inspection Type" sortKey="inspection" sort={sort} onSort={toggleSort} />
                <SortableTh label="Batch Number" sortKey="batch" sort={sort} onSort={toggleSort} />
                <SortableTh label="Sample Ratio" sortKey="sampleRatio" sort={sort} onSort={toggleSort} />
                <SortableTh label="Acceptance" sortKey="acceptance" sort={sort} onSort={toggleSort} />
                <SortableTh label="Production Doc" sortKey="doc" sort={sort} onSort={toggleSort} />
                <th className="px-4 py-3 font-medium">Last change</th>
                <th className="px-4 py-3 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {records === null &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={11} className="px-4 py-3">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {records !== null && filtered.length === 0 && (
                <tr>
                  <td colSpan={11} className="px-4 py-10 text-center text-steel-500">
                    {isFiltered ? 'No quality control records match your filters.' : 'No quality control records yet — create the first one.'}
                  </td>
                </tr>
              )}

              {paged?.map((qc) => {
                const id = qc.qualityControlID
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
                      <td className="px-4 py-3">
                        <TypeBadge type={qc.qualityControlType} />
                      </td>
                      <td className="px-4 py-3 text-steel-700">{formatDate(qc.qualityControlDate)}</td>
                      <td className="px-4 py-3 text-steel-700">{qc.inspectionType || '—'}</td>
                      <td className="px-4 py-3 font-mono text-steel-700">{qc.batchNumber || '—'}</td>
                      <td className="px-4 py-3 text-steel-700">
                        {qc.sampleRatio != null ? `${(qc.sampleRatio * 100).toFixed(0)}%` : '—'}
                      </td>
                      <td className="px-4 py-3">
                        <AcceptanceBadge acceptance={qc.qualityControlAcceptance} />
                      </td>
                      <td className="px-4 py-3 font-mono text-steel-700">{formatProductionDoc(qc.productionDocumentID)}</td>
                                            <AuditStamp
                        createdAt={qc.createdAt}
                        createdBy={qc.createdBy}
                        modifiedAt={qc.modifiedAt}
                        modifiedBy={qc.modifiedBy}
                      />
<td className="px-4 py-3">
                        <div className="flex items-center justify-end gap-1" onClick={(e) => e.stopPropagation()}>
                          <button
                            onClick={() => setEditingRecord(qc)}
                            className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
                            aria-label={`Edit QC record ${id}`}
                            title="Edit"
                          >
                            <Pencil size={16} />
                          </button>
                          <button
                            onClick={() => setDeletingRecord(qc)}
                            className="p-1.5 rounded-md text-steel-500 hover:bg-stop-500/10 hover:text-stop-500"
                            aria-label={`Delete QC record ${id}`}
                            title="Delete"
                          >
                            <Trash2 size={16} />
                          </button>
                        </div>
                      </td>
                    </tr>

                    {isExpanded && (
                      <tr className="bg-panel-50">
                        <td colSpan={11} className="px-4 pb-4 pt-0">
                          <ProductionDocDetail
                            doc={findDoc(qc.productionDocumentID)}
                            products={products}
                            onViewProduct={setViewingProductId}
                            onViewPlan={setViewingPlanId}
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
        open={!!editingRecord}
        onClose={() => setEditingRecord(null)}
        title={editingRecord?.qualityControlID ? 'Edit QC record' : 'New QC record'}
      >
        {editingRecord && (
          <QualityControlForm
            qc={editingRecord.qualityControlID ? editingRecord : null}
            onSubmit={handleSave}
            onCancel={() => setEditingRecord(null)}
          />
        )}
      </Modal>

      <ConfirmDialog
        open={!!deletingRecord}
        title="Delete quality control record"
        message={`Delete QC record #${deletingRecord?.qualityControlID}? This can't be undone.`}
        onCancel={() => {
          setDeletingRecord(null)
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
      <ProductionPlanDetailsModal
        planId={viewingPlanId}
        open={viewingPlanId != null}
        onClose={() => setViewingPlanId(null)}
      />
    </div>
  )
}
