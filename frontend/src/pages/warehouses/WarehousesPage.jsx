import { useEffect, useMemo, useState, Fragment } from 'react'
import { Plus, Pencil, Trash2, Eye } from 'lucide-react'
import { warehousesApi } from '../../api/warehousesApi.js'
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
import { WAREHOUSE_TYPE_LABELS, WAREHOUSE_TYPE_OPTIONS } from './warehouseTypes.js'
import WarehouseForm from './WarehouseForm.jsx'
import WarehouseStockModal from './WarehouseStockModal.jsx'

function TypeBadge({ type }) {
  const label = WAREHOUSE_TYPE_LABELS[type] ?? 'Unknown'
  const isFinal = type === 1
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

function normalizeWarehouse(entry) {
  if (!entry) return null
  if (entry.warehouse) {
    return {
      ...entry.warehouse,
      products: Array.isArray(entry.products) ? entry.products : []
    }
  }
  return entry
}

export default function WarehousesPage() {
  const [warehouses, setWarehouses] = useState(null)
  const [loadError, setLoadError] = useState(null)
  const [search, setSearch] = useState('')
  const [filters, setFilters] = useState({ type: '' })

  const [editingWarehouse, setEditingWarehouse] = useState(null)
  const [stockWarehouse, setStockWarehouse] = useState(null)
  const [deletingWarehouse, setDeletingWarehouse] = useState(null)
  const [deleteLoading, setDeleteLoading] = useState(false)
  const [deleteError, setDeleteError] = useState(null)

  async function loadWarehouses() {
    setLoadError(null)
    try {
      const data = await warehousesApi.getAll()
      const list = Array.isArray(data) ? data : []
      setWarehouses(list.map(normalizeWarehouse))
    } catch (err) {
      setLoadError(err.message || 'Failed to load warehouses.')
    }
  }

  useEffect(() => {
    loadWarehouses()
  }, [])

  const filtered = useMemo(() => {
    if (!warehouses) return []
    let list = warehouses
    const q = search.trim().toLowerCase()
    if (q) {
      list = list.filter((w) => [w.name, w.address].filter(Boolean).some((v) => v.toLowerCase().includes(q)))
    }
    if (filters.type !== '') {
      list = list.filter((w) => String(w.warehouseType) === filters.type)
    }
    return list
  }, [warehouses, search, filters])

  const isFiltered = search.trim() !== '' || filters.type !== ''
  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    name: (w) => w.name,
    type: (w) => w.warehouseType,
    address: (w) => w.address,
    stock: (w) => w.products?.length ?? 0
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    filters,
    sort
  ])

  async function handleSave(payload) {
    if (editingWarehouse?.warehouseID) {
      await warehousesApi.update(editingWarehouse.warehouseID, payload)
    } else {
      await warehousesApi.create(payload)
    }
    setEditingWarehouse(null)
    await loadWarehouses()
  }

  async function handleDelete() {
    setDeleteLoading(true)
    setDeleteError(null)
    try {
      await warehousesApi.remove(deletingWarehouse.warehouseID)
      setDeletingWarehouse(null)
      await loadWarehouses()
    } catch (err) {
      setDeleteError(err.message || 'Failed to delete warehouse.')
    } finally {
      setDeleteLoading(false)
    }
  }

  return (
    <div>
      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search by name or address…"
        filters={[
          {
            name: 'type',
            label: 'Type',
            type: 'select',
            value: filters.type,
            placeholder: 'All types',
            options: WAREHOUSE_TYPE_OPTIONS
          }
        ]}
        onFilterChange={(name, value) => setFilters((prev) => ({ ...prev, [name]: value }))}
        onReset={() => {
          setSearch('')
          setFilters({ type: '' })
        }}
        actions={
          <Button onClick={() => setEditingWarehouse({})}>
            <Plus size={16} />
            New warehouse
          </Button>
        }
      />

      <ErrorBanner message={loadError} />

      <div className="rounded-lg border border-panel-300 bg-white overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                <SortableTh label="Name" sortKey="name" sort={sort} onSort={toggleSort} />
                <SortableTh label="Type" sortKey="type" sort={sort} onSort={toggleSort} />
                <SortableTh label="Address" sortKey="address" sort={sort} onSort={toggleSort} />
                <SortableTh label="Stock" sortKey="stock" sort={sort} onSort={toggleSort} />
                <th className="px-4 py-3 font-medium">Last change</th>
                <th className="px-4 py-3 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {warehouses === null &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={6} className="px-4 py-3">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {warehouses !== null && filtered.length === 0 && (
                <tr>
                  <td colSpan={6} className="px-4 py-10 text-center text-steel-500">
                    {isFiltered ? 'No warehouses match your filters.' : 'No warehouses yet — create the first one.'}
                  </td>
                </tr>
              )}

              {paged?.map((w) => (
                <tr key={w.warehouseID} className="hover:bg-panel-50">
                  <td className="px-4 py-3 text-steel-800 font-medium">{w.name || '—'}</td>
                  <td className="px-4 py-3">
                    <TypeBadge type={w.warehouseType} />
                  </td>
                  <td className="px-4 py-3 text-steel-700">{w.address || '—'}</td>
                  <td className="px-4 py-3 text-steel-700">{w.products?.length ?? 0} product(s)</td>
                                    <AuditStamp
                    createdAt={w.createdAt}
                    createdBy={w.createdBy}
                    modifiedAt={w.modifiedAt}
                    modifiedBy={w.modifiedBy}
                  />
<td className="px-4 py-3">
                    <div className="flex items-center justify-end gap-1">
                      <button
                        onClick={() => setStockWarehouse(w)}
                        className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
                        aria-label={`View stock for ${w.name}`}
                        title="View stock"
                      >
                        <Eye size={16} />
                      </button>
                      <button
                        onClick={() => setEditingWarehouse(w)}
                        className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
                        aria-label={`Edit ${w.name}`}
                        title="Edit"
                      >
                        <Pencil size={16} />
                      </button>
                      <button
                        onClick={() => setDeletingWarehouse(w)}
                        className="p-1.5 rounded-md text-steel-500 hover:bg-stop-500/10 hover:text-stop-500"
                        aria-label={`Delete ${w.name}`}
                        title="Delete"
                      >
                        <Trash2 size={16} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
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
        open={!!editingWarehouse}
        onClose={() => setEditingWarehouse(null)}
        title={editingWarehouse?.warehouseID ? 'Edit warehouse' : 'New warehouse'}
        wide
      >
        {editingWarehouse && (
          <WarehouseForm
            warehouse={editingWarehouse.warehouseID ? editingWarehouse : null}
            onSubmit={handleSave}
            onCancel={() => setEditingWarehouse(null)}
          />
        )}
      </Modal>

      <WarehouseStockModal
        open={!!stockWarehouse}
        onClose={() => setStockWarehouse(null)}
        warehouse={stockWarehouse}
      />

      <ConfirmDialog
        open={!!deletingWarehouse}
        title="Delete warehouse"
        message={`Delete "${deletingWarehouse?.name || 'this warehouse'}"? This can't be undone.`}
        onCancel={() => {
          setDeletingWarehouse(null)
          setDeleteError(null)
        }}
        onConfirm={handleDelete}
        loading={deleteLoading}
      />
      {deleteError && <p className="mt-2 text-sm text-stop-500">{deleteError}</p>}
    </div>
  )
}
