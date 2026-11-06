import { useEffect, useState, Fragment, useMemo } from 'react'
import { Plus, Pencil, Trash2, Eye, ChevronRight, ChevronDown } from 'lucide-react'
import { requestVouchersApi } from '../../api/requestVouchersApi.js'
import { warehousesApi } from '../../api/warehousesApi.js'
import { useWarehouses } from '../../lib/useWarehouses.js'
import { useProducts } from '../../lib/useProducts.js'
import { useProductionDocuments } from '../../lib/useProductionDocuments.js'
import { WAREHOUSE_TYPE_LABELS } from '../warehouses/warehouseTypes.js'
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
import VoucherProductsModal from '../../components/VoucherProductsModal.jsx'
import RequestVoucherForm from './RequestVoucherForm.jsx'

function formatDate(isoString) {
  if (!isoString) return '—'
  return isoString.slice(0, 10)
}

function ProductionDocDetail({ doc, products, onViewProduct }) {
  if (!doc) {
    return <p className="py-4 text-sm text-steel-500">Production document not found.</p>
  }
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

export default function RequestVouchersPage() {
  const [entries, setEntries] = useState(null)
  const [loadError, setLoadError] = useState(null)
   const { warehouses } = useWarehouses()
  const { products } = useProducts()
  const { documents } = useProductionDocuments()
  const [viewingEntry, setViewingEntry] = useState(null)
  const [search, setSearch] = useState('')
  const [filters, setFilters] = useState({ dateFrom: '', dateTo: '' })

  const [expandedId, setExpandedId] = useState(null)
  const [viewingProductId, setViewingProductId] = useState(null)

  const [editingEntry, setEditingEntry] = useState(null)
  const [deletingEntry, setDeletingEntry] = useState(null)
  const [deleteLoading, setDeleteLoading] = useState(false)
  const [deleteError, setDeleteError] = useState(null)

  async function loadEntries() {
    setLoadError(null)
    try {
      const data = await requestVouchersApi.getAll()
      setEntries(Array.isArray(data) ? data : [])
    } catch (err) {
      setLoadError(err.message || 'Failed to load request vouchers.')
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

   function findDoc(id) {
     return documents?.find((d) => d.productionDocumentID === id)
   }

   function formatProductionDoc(id) {
     if (!id) return '—'
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
    if (!entries) return []
    let list = entries
    const q = search.trim().toLowerCase()
    if (q) {
      list = list.filter((entry) =>
        [String(entry.voucher.requestVoucherID), String(entry.voucher.productionDocumentID), warehouseName(entry.voucher.warehouseID)]
          .filter(Boolean)
          .some((v) => v.toLowerCase().includes(q))
      )
    }
    list = list.filter((entry) => {
      const wh = warehouses?.find((w) => w.warehouseID === entry.voucher.warehouseID)
      return wh?.warehouseType === 0
    })
    if (filters.dateFrom) list = list.filter((entry) => (entry.voucher.requestDate || '').slice(0, 10) >= filters.dateFrom)
    if (filters.dateTo) list = list.filter((entry) => (entry.voucher.requestDate || '').slice(0, 10) <= filters.dateTo)
    return list
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [entries, search, filters, warehouses])

  const isFiltered = search.trim() !== '' || filters.dateFrom !== '' || filters.dateTo !== ''
  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    id: (e) => e.voucher.requestVoucherID,
    date: (e) => e.voucher.requestDate,
    warehouse: (e) => warehouseName(e.voucher.warehouseID),
    doc: (e) => e.voucher.productionDocumentID,
    items: (e) => e.products?.length ?? 0
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    filters,
    sort,
    warehouses
  ])

  async function handleSave(payload) {
    if (editingEntry?.voucher?.requestVoucherID) {
      await requestVouchersApi.update(editingEntry.voucher.requestVoucherID, payload)
    } else {
      await requestVouchersApi.create(payload)
      await deductWarehouseStock(payload)
    }
    setEditingEntry(null)
    await loadEntries()
  }

  async function deductWarehouseStock(payload) {
    const whId = Number(payload.voucher.warehouseID)
    const wh = warehouses?.find((w) => w.warehouseID === whId)
    if (!wh) return

    const currentStock = (wh.products || []).map((p) => ({
      productID: Number(p.productID),
      quantity: Number(p.quantity) || 0
    }))

    const updatedStock = currentStock.map((s) => {
      const item = payload.products.find((p) => Number(p.productID) === s.productID)
      if (item) {
        const ded = Number(item.requestedQuantity) || 0
        return { ...s, quantity: Math.max(0, s.quantity - ded) }
      }
      return s
    })

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
      await requestVouchersApi.remove(deletingEntry.voucher.requestVoucherID)
      setDeletingEntry(null)
      await loadEntries()
    } catch (err) {
      setDeleteError(err.message || 'Failed to delete request voucher.')
    } finally {
      setDeleteLoading(false)
    }
  }

  return (
    <div>
      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search by #, document or warehouse…"
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
          <Button onClick={() => setEditingEntry({})}>
            <Plus size={16} />
            New request voucher
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
                <SortableTh label="Request Date" sortKey="date" sort={sort} onSort={toggleSort} />
                <SortableTh label="Warehouse" sortKey="warehouse" sort={sort} onSort={toggleSort} />
                <SortableTh label="Production Doc" sortKey="doc" sort={sort} onSort={toggleSort} />
                <SortableTh label="Line Items" sortKey="items" sort={sort} onSort={toggleSort} />
               <th className="px-4 py-3 font-medium">Last change</th>
                <th className="px-4 py-3 font-medium text-right">Actions</th>
               </tr>
             </thead>
             <tbody className="divide-y divide-panel-200">
               {entries === null &&
                 [1, 2, 3].map((i) => (
                   <tr key={i}>
                     <td colSpan={7} className="px-4 py-3">
                       <div className="h-4 rounded bg-panel-100 animate-pulse" />
                     </td>
                   </tr>
                 ))}

               {entries !== null && filtered.length === 0 && (
                 <tr>
                   <td colSpan={7} className="px-4 py-10 text-center text-steel-500">
                     {isFiltered ? 'No request vouchers match your filters.' : 'No request vouchers yet — create the first one.'}
                   </td>
                 </tr>
               )}

               {paged?.map((entry) => {
                 const id = entry.voucher.requestVoucherID
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
                       <td className="px-4 py-3 text-steel-700">{formatDate(entry.voucher.requestDate)}</td>
                       <td className="px-4 py-3 text-steel-700">{warehouseName(entry.voucher.warehouseID)}</td>
                       <td className="px-4 py-3 font-mono text-steel-700">
                         {formatProductionDoc(entry.voucher.productionDocumentID)}
                       </td>
                       <td className="px-4 py-3 text-steel-700">{entry.products?.length ?? 0} item(s)</td>
                       <AuditStamp
                         createdAt={entry.voucher.createdAt}
                         createdBy={entry.voucher.createdBy}
                         modifiedAt={entry.voucher.modifiedAt}
                         modifiedBy={entry.voucher.modifiedBy}
                       />
                       <td className="px-4 py-3">
                         <div className="flex items-center justify-end gap-1" onClick={(e) => e.stopPropagation()}>
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

                     {isExpanded && (
                       <tr className="bg-panel-50">
                         <td colSpan={7} className="px-4 pb-4 pt-0">
                           <ProductionDocDetail
                             doc={findDoc(entry.voucher.productionDocumentID)}
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
        open={!!editingEntry}
        onClose={() => setEditingEntry(null)}
        title={editingEntry?.voucher?.requestVoucherID ? 'Edit request voucher' : 'New request voucher'}
        wide
      >
        {editingEntry && (
          <RequestVoucherForm
            entry={editingEntry.voucher ? editingEntry : null}
            onSubmit={handleSave}
            onCancel={() => setEditingEntry(null)}
          />
        )}
      </Modal>

      <ConfirmDialog
        open={!!deletingEntry}
        title="Delete request voucher"
        message={`Delete voucher #${deletingEntry?.voucher?.requestVoucherID}? This can't be undone.`}
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
        voucherId={viewingEntry?.voucher?.requestVoucherID}
        voucherLabel="Request Voucher"
        items={viewingEntry?.products || []}
        products={products}
        quantityField="requestedQuantity"
        quantityLabel="Requested Qty"
        productLink
      />

      <ProductDetailsModal
        productId={viewingProductId}
        open={viewingProductId != null}
        onClose={() => setViewingProductId(null)}
      />
    </div>
  )
}
