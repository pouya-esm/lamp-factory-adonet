import { useEffect, useState, Fragment, useMemo } from 'react'
import { Plus, Pencil, Trash2, ChevronRight, ChevronDown } from 'lucide-react'
import { productionPlansApi } from '../../api/productionPlansApi.js'
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
import ProductionPlanForm from './ProductionPlanForm.jsx'
import { priorityMeta, PRIORITY_OPTIONS } from './priority.js'

function formatDate(isoString) {
  if (!isoString) return '—'
  return isoString.slice(0, 10)
}

function PriorityBadge({ priority }) {
  const meta = priorityMeta(priority)
  return (
    <span className={`inline-flex items-center rounded px-2 py-0.5 text-xs font-medium ${meta.badge}`}>
      {meta.label}
    </span>
  )
}

function ProductsDetail({ items, products }) {
  if (!items || items.length === 0) {
    return <p className="px-3 py-3 text-sm text-steel-500">No products in this plan.</p>
  }
  return (
    <table className="w-full text-sm">
      <thead>
        <tr className="text-left text-xs text-steel-500">
          <th className="px-3 py-2 font-medium">Standard Number</th>
          <th className="px-3 py-2 font-medium">Material</th>
          <th className="px-3 py-2 font-medium text-right">Min Qty</th>
          <th className="px-3 py-2 font-medium text-right">Max Qty</th>
        </tr>
      </thead>
      <tbody className="divide-y divide-panel-200">
        {items.map((item, i) => {
          const p = products?.find((prod) => prod.productID === item.productID)
          return (
            <tr key={i}>
              <td className="px-3 py-2 font-mono text-steel-800">{p?.standardNumber || `Product ${item.productID}`}</td>
              <td className="px-3 py-2 text-steel-700">{p?.material || '—'}</td>
              <td className="px-3 py-2 text-steel-700 text-right font-mono">{item.minQuantity ?? '—'}</td>
              <td className="px-3 py-2 text-steel-700 text-right font-mono">{item.maxQuantity ?? '—'}</td>
            </tr>
          )
        })}
      </tbody>
    </table>
  )
}

export default function ProductionPlansPage() {
  const [entries, setEntries] = useState(null)
  const [loadError, setLoadError] = useState(null)
  const { products } = useProducts()
  const [expandedId, setExpandedId] = useState(null)
  const [search, setSearch] = useState('')
  const [filters, setFilters] = useState({ priority: '', issuedFrom: '', issuedTo: '' })

  const [editingEntry, setEditingEntry] = useState(null)
  const [deletingEntry, setDeletingEntry] = useState(null)
  const [deleteLoading, setDeleteLoading] = useState(false)
  const [deleteError, setDeleteError] = useState(null)

  async function loadEntries() {
    setLoadError(null)
    try {
      const data = await productionPlansApi.getAll()
      setEntries(Array.isArray(data) ? data : [])
    } catch (err) {
      setLoadError(err.message || 'Failed to load production plans.')
    }
  }

  useEffect(() => {
    loadEntries()
  }, [])

  const filtered = useMemo(() => {
    if (!entries) return []
    let list = entries
    const q = search.trim().toLowerCase()
    if (q) {
      list = list.filter((entry) => String(entry.productionPlan.productionPlanId).includes(q))
    }
    if (filters.priority !== '') {
      list = list.filter((entry) => String(entry.productionPlan.priority) === filters.priority)
    }
    const issued = (entry) => (entry.productionPlan.issueDate || '').slice(0, 10)
    if (filters.issuedFrom) list = list.filter((entry) => issued(entry) >= filters.issuedFrom)
    if (filters.issuedTo) list = list.filter((entry) => issued(entry) <= filters.issuedTo)
    return list
  }, [entries, search, filters])

  const isFiltered =
    search.trim() !== '' ||
    filters.priority !== '' ||
    filters.issuedFrom !== '' ||
    filters.issuedTo !== ''
  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    id: (entry) => entry.productionPlan.productionPlanId,
    issueDate: (entry) => entry.productionPlan.issueDate,
    targetDate: (entry) => entry.productionPlan.targetDate,
    priority: (entry) => entry.productionPlan.priority,
    products: (entry) => entry.products?.length ?? 0
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
    if (editingEntry?.productionPlan?.productionPlanId) {
      await productionPlansApi.update(editingEntry.productionPlan.productionPlanId, payload)
    } else {
      await productionPlansApi.create(payload)
    }
    setEditingEntry(null)
    await loadEntries()
  }

  async function handleDelete() {
    setDeleteLoading(true)
    setDeleteError(null)
    try {
      await productionPlansApi.remove(deletingEntry.productionPlan.productionPlanId)
      setDeletingEntry(null)
      await loadEntries()
    } catch (err) {
      setDeleteError(err.message || 'Failed to delete production plan.')
    } finally {
      setDeleteLoading(false)
    }
  }

  return (
    <div>
      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search by plan #…"
        filters={[
          {
            name: 'priority',
            label: 'Priority',
            type: 'select',
            value: filters.priority,
            placeholder: 'All priorities',
            options: PRIORITY_OPTIONS
          },
          { name: 'issuedFrom', label: 'Issued from', type: 'date', value: filters.issuedFrom },
          { name: 'issuedTo', label: 'Issued to', type: 'date', value: filters.issuedTo }
        ]}
        onFilterChange={(name, value) => setFilters((prev) => ({ ...prev, [name]: value }))}
        onReset={() => {
          setSearch('')
          setFilters({ priority: '', issuedFrom: '', issuedTo: '' })
        }}
        actions={
          <Button onClick={() => setEditingEntry({})}>
            <Plus size={16} />
            New production plan
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
                <SortableTh label="Issue Date" sortKey="issueDate" sort={sort} onSort={toggleSort} />
                <SortableTh label="Target Date" sortKey="targetDate" sort={sort} onSort={toggleSort} />
                <SortableTh label="Priority" sortKey="priority" sort={sort} onSort={toggleSort} />
                <SortableTh label="Products" sortKey="products" sort={sort} onSort={toggleSort} />
                <th className="px-4 py-3 font-medium">Last change</th>
                <th className="px-4 py-3 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {entries === null &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={8} className="px-4 py-3">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {entries !== null && filtered.length === 0 && (
                <tr>
                  <td colSpan={8} className="px-4 py-10 text-center text-steel-500">
                    {isFiltered ? 'No production plans match your filters.' : 'No production plans yet — create the first one.'}
                  </td>
                </tr>
              )}

              {paged?.map((entry) => {
                const id = entry.productionPlan.productionPlanId
                const isExpanded = expandedId === id
                const hasItems = (entry.products?.length ?? 0) > 0

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
                      <td className="px-4 py-3 text-steel-700">{formatDate(entry.productionPlan.issueDate)}</td>
                      <td className="px-4 py-3 text-steel-700">{formatDate(entry.productionPlan.targetDate)}</td>
                      <td className="px-4 py-3">
                        <PriorityBadge priority={entry.productionPlan.priority} />
                      </td>
                      <td className="px-4 py-3 text-steel-700">{entry.products?.length ?? 0} item(s)</td>
                                            <AuditStamp
                        createdAt={entry.productionPlan.createdAt}
                        createdBy={entry.productionPlan.createdBy}
                        modifiedAt={entry.productionPlan.modifiedAt}
                        modifiedBy={entry.productionPlan.modifiedBy}
                      />
<td className="px-4 py-3">
                        <div className="flex items-center justify-end gap-1" onClick={(e) => e.stopPropagation()}>
                          <button
                            onClick={() => setEditingEntry(entry)}
                            className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
                            aria-label={`Edit plan ${id}`}
                            title="Edit"
                          >
                            <Pencil size={16} />
                          </button>
                          <button
                            onClick={() => setDeletingEntry(entry)}
                            className="p-1.5 rounded-md text-steel-500 hover:bg-stop-500/10 hover:text-stop-500"
                            aria-label={`Delete plan ${id}`}
                            title="Delete"
                          >
                            <Trash2 size={16} />
                          </button>
                        </div>
                      </td>
                    </tr>

                    {isExpanded && (
                      <tr className="bg-panel-50">
                        <td colSpan={8} className="px-4 pb-4 pt-0">
                          <ProductsDetail items={entry.products} products={products} />
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
        title={editingEntry?.productionPlan?.productionPlanId ? 'Edit production plan' : 'New production plan'}
        wide
      >
        {editingEntry && (
          <ProductionPlanForm
            entry={editingEntry.productionPlan ? editingEntry : null}
            onSubmit={handleSave}
            onCancel={() => setEditingEntry(null)}
          />
        )}
      </Modal>

      <ConfirmDialog
        open={!!deletingEntry}
        title="Delete production plan"
        message={`Delete plan #${deletingEntry?.productionPlan?.productionPlanId}? This can't be undone.`}
        onCancel={() => {
          setDeletingEntry(null)
          setDeleteError(null)
        }}
        onConfirm={handleDelete}
        loading={deleteLoading}
      />
      {deleteError && <p className="mt-2 text-sm text-stop-500">{deleteError}</p>}
    </div>
  )
}
