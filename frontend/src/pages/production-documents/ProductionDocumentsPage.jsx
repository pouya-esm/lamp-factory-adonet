import { useEffect, useState, Fragment, useMemo } from 'react'
import { Plus, Pencil, Trash2, ChevronRight, ChevronDown } from 'lucide-react'
import { productionDocumentsApi } from '../../api/productionDocumentsApi.js'
import { useProducts } from '../../lib/useProducts.js'
import { useOperators, operatorName } from '../../lib/useOperators.js'
import { useProductionLines } from '../../lib/useProductionLines.js'
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
import ProductionDocumentForm from './ProductionDocumentForm.jsx'

function formatDate(isoString) {
  if (!isoString) return '—'
  return isoString.slice(0, 10)
}

function OperatorsDetail({ items, operators }) {
  if (!items || items.length === 0) {
    return <p className="px-3 py-3 text-sm text-steel-500">No operators assigned.</p>
  }
  return (
    <table className="w-full text-sm">
      <thead>
        <tr className="text-left text-xs text-steel-500">
          <th className="px-3 py-2 font-medium">Operator</th>
          <th className="px-3 py-2 font-medium">Role</th>
          <th className="px-3 py-2 font-medium">From</th>
          <th className="px-3 py-2 font-medium">Until</th>
        </tr>
      </thead>
      <tbody className="divide-y divide-panel-200">
        {items.map((item, i) => {
          const op = operators?.find((o) => o.operatorID === item.operatorID)
          return (
            <tr key={i}>
              <td className="px-3 py-2 text-steel-800">{op ? operatorName(op) : `Operator ${item.operatorID}`}</td>
              <td className="px-3 py-2 text-steel-700">{item.operatorRole || '—'}</td>
              <td className="px-3 py-2 text-steel-700">{formatDate(item.assignedDateFrom)}</td>
              <td className="px-3 py-2 text-steel-700">{formatDate(item.assignedDateUntil)}</td>
            </tr>
          )
        })}
      </tbody>
    </table>
  )
}

export default function ProductionDocumentsPage() {
  const [entries, setEntries] = useState(null)
  const [loadError, setLoadError] = useState(null)
  const { products } = useProducts()
  const { operators } = useOperators()
  const { lines } = useProductionLines()
  const [expandedId, setExpandedId] = useState(null)
  const [search, setSearch] = useState('')
  const [filters, setFilters] = useState({ line: '', dateFrom: '', dateTo: '' })

  const [editingEntry, setEditingEntry] = useState(null)
  const [deletingEntry, setDeletingEntry] = useState(null)
  const [viewingProductId, setViewingProductId] = useState(null)
  const [deleteLoading, setDeleteLoading] = useState(false)
  const [deleteError, setDeleteError] = useState(null)

  async function loadEntries() {
    setLoadError(null)
    try {
      const data = await productionDocumentsApi.getAll()
      setEntries(Array.isArray(data) ? data : [])
    } catch (err) {
      setLoadError(err.message || 'Failed to load production documents.')
    }
  }

  useEffect(() => {
    loadEntries()
  }, [])

  function productLabel(id) {
    const p = products?.find((p) => p.productID === id)
    return p?.standardNumber || `Product ${id}`
  }

  const filtered = useMemo(() => {
    if (!entries) return []
    let list = entries
    const q = search.trim().toLowerCase()
    if (q) {
      list = list.filter((entry) => {
        const d = entry.productionDocument
        return [String(d.productionDocumentID), productLabel(d.productID)]
          .some((v) => v.toLowerCase().includes(q))
      })
    }
    if (filters.line !== '') {
      list = list.filter((entry) => String(entry.productionDocument.productionLineID) === filters.line)
    }
    const date = (entry) => (entry.productionDocument.productionDate || '').slice(0, 10)
    if (filters.dateFrom) list = list.filter((entry) => date(entry) >= filters.dateFrom)
    if (filters.dateTo) list = list.filter((entry) => date(entry) <= filters.dateTo)
    return list
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [entries, search, filters, products])

  const isFiltered =
    search.trim() !== '' || filters.line !== '' || filters.dateFrom !== '' || filters.dateTo !== ''
  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    id: (entry) => entry.productionDocument.productionDocumentID,
    date: (entry) => entry.productionDocument.productionDate,
    product: (entry) => productLabel(entry.productionDocument.productID),
    line: (entry) => entry.productionDocument.productionLineID,
    produced: (entry) => entry.productionDocument.producedQuantity,
    rejected: (entry) => entry.productionDocument.rejectedQuantity,
    operators: (entry) => entry.operators?.length ?? 0
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    filters,
    sort
  ])

  function toggleExpand(id) {
    setExpandedId((prev) => (prev === id ? null : id))
  }

  async function handleSave(payload) {
    if (editingEntry?.productionDocument?.productionDocumentID) {
      await productionDocumentsApi.update(editingEntry.productionDocument.productionDocumentID, payload)
    } else {
      await productionDocumentsApi.create(payload)
    }
    setEditingEntry(null)
    await loadEntries()
  }

  async function handleDelete() {
    setDeleteLoading(true)
    setDeleteError(null)
    try {
      await productionDocumentsApi.remove(deletingEntry.productionDocument.productionDocumentID)
      setDeletingEntry(null)
      await loadEntries()
    } catch (err) {
      setDeleteError(err.message || 'Failed to delete production document.')
    } finally {
      setDeleteLoading(false)
    }
  }

  return (
    <div>
      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search by # or product…"
        filters={[
          {
            name: 'line',
            label: 'Line',
            type: 'select',
            value: filters.line,
            placeholder: 'All lines',
            options: (lines ?? []).map((l) => ({ value: l.productionLineID, label: `Line ${l.productionLineID}` }))
          },
          { name: 'dateFrom', label: 'Date from', type: 'date', value: filters.dateFrom },
          { name: 'dateTo', label: 'Date to', type: 'date', value: filters.dateTo }
        ]}
        onFilterChange={(name, value) => setFilters((prev) => ({ ...prev, [name]: value }))}
        onReset={() => {
          setSearch('')
          setFilters({ line: '', dateFrom: '', dateTo: '' })
        }}
        actions={
          <Button onClick={() => setEditingEntry({})}>
            <Plus size={16} />
            New production document
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
                <SortableTh label="Product" sortKey="product" sort={sort} onSort={toggleSort} />
                <SortableTh label="Line" sortKey="line" sort={sort} onSort={toggleSort} />
                <SortableTh label="Produced" sortKey="produced" sort={sort} onSort={toggleSort} />
                <SortableTh label="Rejected" sortKey="rejected" sort={sort} onSort={toggleSort} />
                <SortableTh label="Operators" sortKey="operators" sort={sort} onSort={toggleSort} />
                <th className="px-4 py-3 font-medium">Last change</th>
                <th className="px-4 py-3 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {entries === null &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={10} className="px-4 py-3">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {entries !== null && filtered.length === 0 && (
                <tr>
                  <td colSpan={10} className="px-4 py-10 text-center text-steel-500">
                    {isFiltered ? 'No production documents match your filters.' : 'No production documents yet — create the first one.'}
                  </td>
                </tr>
              )}

              {paged?.map((entry) => {
                const d = entry.productionDocument
                const id = d.productionDocumentID
                const isExpanded = expandedId === id
                const hasItems = (entry.operators?.length ?? 0) > 0

                return (
                  <Fragment key={id}>
                    <tr
                      onClick={() => hasItems && toggleExpand(id)}
                      className={`hover:bg-panel-50 ${hasItems ? 'cursor-pointer' : ''}`}
                    >
                      <td className="px-4 py-3">
                        {hasItems && (
                          <span className="text-steel-400">
                            {isExpanded ? <ChevronDown size={16} /> : <ChevronRight size={16} />}
                          </span>
                        )}
                      </td>
                      <td className="px-4 py-3 font-mono text-steel-800">{id}</td>
                      <td className="px-4 py-3 text-steel-700">{formatDate(d.productionDate)}</td>
                      <td className="px-4 py-3 font-mono text-steel-700">
                        <button
                          type="button"
                          onClick={(e) => {
                            e.stopPropagation()
                            setViewingProductId(d.productID)
                          }}
                          className="font-mono text-amber-600 hover:text-amber-700 hover:underline"
                          title="View product details"
                        >
                          {productLabel(d.productID)}
                        </button>
                      </td>
                      <td className="px-4 py-3 text-steel-700">Line {d.productionLineID}</td>
                      <td className="px-4 py-3 text-steel-700">{d.producedQuantity ?? '—'}</td>
                      <td className="px-4 py-3 text-steel-700">{d.rejectedQuantity ?? '—'}</td>
                      <td className="px-4 py-3 text-steel-700">{entry.operators?.length ?? 0} assigned</td>
                                            <AuditStamp
                        createdAt={entry.productionDocument.createdAt}
                        createdBy={entry.productionDocument.createdBy}
                        modifiedAt={entry.productionDocument.modifiedAt}
                        modifiedBy={entry.productionDocument.modifiedBy}
                      />
<td className="px-4 py-3">
                        <div className="flex items-center justify-end gap-1" onClick={(e) => e.stopPropagation()}>
                          <button
                            onClick={() => setEditingEntry(entry)}
                            className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
                            aria-label={`Edit document ${id}`}
                            title="Edit"
                          >
                            <Pencil size={16} />
                          </button>
                          <button
                            onClick={() => setDeletingEntry(entry)}
                            className="p-1.5 rounded-md text-steel-500 hover:bg-stop-500/10 hover:text-stop-500"
                            aria-label={`Delete document ${id}`}
                            title="Delete"
                          >
                            <Trash2 size={16} />
                          </button>
                        </div>
                      </td>
                    </tr>

                    {isExpanded && (
                      <tr className="bg-panel-50">
                        <td colSpan={10} className="px-4 pb-4 pt-0">
                          <OperatorsDetail items={entry.operators} operators={operators} />
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
        open={!!editingEntry}
        onClose={() => setEditingEntry(null)}
        title={editingEntry?.productionDocument?.productionDocumentID ? 'Edit production document' : 'New production document'}
        wide
      >
        {editingEntry && (
          <ProductionDocumentForm
            entry={editingEntry.productionDocument ? editingEntry : null}
            onSubmit={handleSave}
            onCancel={() => setEditingEntry(null)}
          />
        )}
      </Modal>

      <ConfirmDialog
        open={!!deletingEntry}
        title="Delete production document"
        message={`Delete document #${deletingEntry?.productionDocument?.productionDocumentID}? This can't be undone.`}
        onCancel={() => {
          setDeletingEntry(null)
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
