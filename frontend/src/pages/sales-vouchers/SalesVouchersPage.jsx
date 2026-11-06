import { useEffect, useState, useMemo } from 'react'
import { Plus, Pencil, Trash2, Eye } from 'lucide-react'
import { salesVouchersApi } from '../../api/salesVouchersApi.js'
import { warehousesApi } from '../../api/warehousesApi.js'
import { useWarehouses } from '../../lib/useWarehouses.js'
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
import VoucherProductsModal from '../../components/VoucherProductsModal.jsx'
import SalesVoucherForm from './SalesVoucherForm.jsx'

export default function SalesVouchersPage() {
  const [entries, setEntries] = useState(null)
  const [loadError, setLoadError] = useState(null)
  const { warehouses } = useWarehouses()
  const { products } = useProducts()
  const [viewingEntry, setViewingEntry] = useState(null)
  const [search, setSearch] = useState('')
  const [filters, setFilters] = useState({})

  const [editingEntry, setEditingEntry] = useState(null)
  const [deletingEntry, setDeletingEntry] = useState(null)
  const [deleteLoading, setDeleteLoading] = useState(false)
  const [deleteError, setDeleteError] = useState(null)

  async function loadEntries() {
    setLoadError(null)
    try {
      const data = await salesVouchersApi.getAll()
      setEntries(Array.isArray(data) ? data : [])
    } catch (err) {
      setLoadError(err.message || 'Failed to load sales vouchers.')
    }
  }

  useEffect(() => {
    loadEntries()
  }, [])

  function warehouseName(id) {
    const w = warehouses?.find((w) => w.warehouseID === id)
    if (!w) return `Warehouse ${id}`
    return `${w.name || '—'} — ${w.address || '—'}`
  }

  const filtered = useMemo(() => {
    if (!entries) return []
    let list = entries
    const q = search.trim().toLowerCase()
    if (q) {
      list = list.filter((entry) =>
        [String(entry.voucher.salesVoucherID), warehouseName(entry.voucher.warehouseID)]
          .filter(Boolean)
          .some((v) => v.toLowerCase().includes(q))
      )
    }
    list = list.filter((entry) => {
      const wh = warehouses?.find((w) => w.warehouseID === entry.voucher.warehouseID)
      return wh?.warehouseType === 1
    })
    return list
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [entries, search, warehouses])

  const isFiltered = search.trim() !== ''
  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    id: (e) => e.voucher.salesVoucherID,
    warehouse: (e) => warehouseName(e.voucher.warehouseID),
    items: (e) => e.products?.length ?? 0
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    sort,
    warehouses
  ])

  async function handleSave(payload) {
    if (editingEntry?.voucher?.salesVoucherID) {
      await salesVouchersApi.update(editingEntry.voucher.salesVoucherID, payload)
    } else {
      await salesVouchersApi.create(payload)
      await updateWarehouseStock(payload)
    }
    setEditingEntry(null)
    await loadEntries()
  }

  async function updateWarehouseStock(payload) {
    const whId = Number(payload.voucher.warehouseID)
    const wh = warehouses?.find((w) => w.warehouseID === whId)
    if (!wh) return

    const currentStock = (wh.products || []).map((p) => ({
      productID: Number(p.productID),
      quantity: Number(p.quantity) || 0
    }))

    const updatedStock = [...currentStock]
    for (const item of payload.products) {
      const pid = Number(item.productID)
      const qty = Number(item.salesQuantity) || 0
      const existing = updatedStock.find((s) => s.productID === pid)
      if (existing) {
        existing.quantity -= qty
      } else {
        updatedStock.push({ productID: pid, quantity: -qty })
      }
    }

    await warehousesApi.update(whId, {
      warehouse: {
        warehouseID: wh.warehouseID,
        warehouseType: wh.warehouseType,
        name: wh.name,
        address: wh.address
      },
      products: updatedStock
    })
  }

  async function handleDelete() {
    setDeleteLoading(true)
    setDeleteError(null)
    try {
      await salesVouchersApi.remove(deletingEntry.voucher.salesVoucherID)
      setDeletingEntry(null)
      await loadEntries()
    } catch (err) {
      setDeleteError(err.message || 'Failed to delete sales voucher.')
    } finally {
      setDeleteLoading(false)
    }
  }

  return (
    <div>
      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search by # or warehouse…"
        filters={[]}
        onFilterChange={(name, value) => setFilters((prev) => ({ ...prev, [name]: value }))}
        onReset={() => {
          setSearch('')
          setFilters({})
        }}
        actions={
          <Button onClick={() => setEditingEntry({})}>
            <Plus size={16} />
            New sales voucher
          </Button>
        }
      />

      <ErrorBanner message={loadError} />

      <div className="rounded-lg border border-panel-300 bg-white overflow-hidden">
          <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                <SortableTh label="#" sortKey="id" sort={sort} onSort={toggleSort} />
                <SortableTh label="Warehouse" sortKey="warehouse" sort={sort} onSort={toggleSort} />
                <SortableTh label="Line Items" sortKey="items" sort={sort} onSort={toggleSort} />
                <th className="px-4 py-3 font-medium">Last change</th>
                <th className="px-4 py-3 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {entries === null &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={5} className="px-4 py-3">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {entries !== null && filtered.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-10 text-center text-steel-500">
                    {isFiltered ? 'No sales vouchers match your filters.' : 'No sales vouchers yet — create the first one.'}
                  </td>
                </tr>
              )}

              {paged?.map((entry) => {
                const id = entry.voucher.salesVoucherID

                return (
                  <tr key={id} className="hover:bg-panel-50">
                    <td className="px-4 py-3 font-mono text-steel-800">{id}</td>
                    <td className="px-4 py-3 text-steel-700">{warehouseName(entry.voucher.warehouseID)}</td>
                    <td className="px-4 py-3 text-steel-700">{entry.products?.length ?? 0} item(s)</td>
                                          <AuditStamp
                        createdAt={entry.voucher.createdAt}
                        createdBy={entry.voucher.createdBy}
                        modifiedAt={entry.voucher.modifiedAt}
                        modifiedBy={entry.voucher.modifiedBy}
                      />
<td className="px-4 py-3">
                      <div className="flex items-center justify-end gap-1">
                        <button
                          onClick={() => setViewingEntry(entry)}
                          className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
                          aria-label={`View products for voucher ${id}`}
                          title="View Products"
                        >
                          <Eye size={16} />
                        </button>
                        <button
                          onClick={() => setEditingEntry(entry)}
                          className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
                          aria-label={`Edit voucher ${id}`}
                          title="Edit"
                        >
                          <Pencil size={16} />
                        </button>
                        <button
                          onClick={() => setDeletingEntry(entry)}
                          className="p-1.5 rounded-md text-steel-500 hover:bg-stop-500/10 hover:text-stop-500"
                          aria-label={`Delete voucher ${id}`}
                          title="Delete"
                        >
                          <Trash2 size={16} />
                        </button>
                      </div>
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
        className="mt-4"
      />

      <Modal
        open={!!editingEntry}
        onClose={() => setEditingEntry(null)}
        title={editingEntry?.voucher?.salesVoucherID ? 'Edit sales voucher' : 'New sales voucher'}
        wide
      >
        {editingEntry && (
          <SalesVoucherForm
            entry={editingEntry.voucher ? editingEntry : null}
            onSubmit={handleSave}
            onCancel={() => setEditingEntry(null)}
          />
        )}
      </Modal>

      <ConfirmDialog
        open={!!deletingEntry}
        title="Delete sales voucher"
        message={`Delete voucher #${deletingEntry?.voucher?.salesVoucherID}? This can't be undone.`}
        onCancel={() => {
          setDeletingEntry(null)
          setDeleteError(null)
        }}
        onConfirm={handleDelete}
        loading={deleteLoading}
      />
      {deleteError && <p className="mt-2 text-sm text-stop-500">{deleteError}</p>}

      <VoucherProductsModal
        open={!!viewingEntry}
        onClose={() => setViewingEntry(null)}
        voucherId={viewingEntry?.voucher?.salesVoucherID}
        voucherLabel="Sales Voucher"
        items={viewingEntry?.products || []}
        products={products}
        quantityField="salesQuantity"
        quantityLabel="Sales Qty"
      />
    </div>
  )
}
