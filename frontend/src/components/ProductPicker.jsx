import { useState, useMemo } from 'react'
import { Search } from 'lucide-react'
import Modal from './ui/Modal.jsx'
import Pagination from './ui/Pagination.jsx'
import SortableTh from './ui/SortableTh.jsx'
import { inputClass } from './ui/Field.jsx'
import usePagedList from '../hooks/usePagedList.js'
import useSortable from '../hooks/useSortable.js'
import { PRODUCT_TYPE_LABELS } from '../pages/products/productTypes.js'

function findProductStock(productId, warehouseStock) {
  if (!warehouseStock) return null
  const entry = warehouseStock.find((w) => w.productID === productId)
  return entry?.quantity ?? 0
}

export default function ProductPicker({
  open,
  onClose,
  products,
  selectedId,
  onSelect,
  productTypeFilter,
  warehouseStock,
  stockLabel = 'Current Stock'
}) {
  const [search, setSearch] = useState('')

  const loading = products == null

  const filtered = useMemo(() => {
    if (!products) return []
    let list = products
    if (productTypeFilter != null) {
      list = list.filter((p) => p.productType === productTypeFilter)
    }
    const q = search.trim().toLowerCase()
    if (!q) return list
    return list.filter((p) =>
      [
        String(p.productID),
        p.standardNumber,
        p.material,
        p.productColor,
        PRODUCT_TYPE_LABELS[p.productType]
      ]
        .filter(Boolean)
        .some((v) => String(v).toLowerCase().includes(q))
    )
  }, [products, search, productTypeFilter])

  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    id: (p) => p.productID,
    standardNumber: (p) => p.standardNumber ?? '',
    material: (p) => p.material ?? '',
    ...(productTypeFilter == null && { type: (p) => p.productType }),
    color: (p) => p.productColor ?? '',
    weight: (p) => p.weight ?? 0,
    ...(warehouseStock != null && { stock: (p) => findProductStock(p.productID, warehouseStock) })
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    sort,
    productTypeFilter,
    warehouseStock
  ])

  function handleSelect(p) {
    onSelect(p)
    onClose()
  }

  const hasStock = warehouseStock != null
  const colCount = (productTypeFilter == null ? 5 : 6) + (hasStock ? 1 : 0)

  return (
    <Modal open={open} onClose={onClose} title="Select a product" maxWidth="max-w-4xl">
      <div className="mb-3">
        <div className="relative">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-steel-500" />
          <input
            className={`${inputClass} pl-9`}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search by ID, standard number, material or color…"
          />
        </div>
      </div>

      <div className="border border-panel-300 rounded-md overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                <SortableTh label="#" sortKey="id" sort={sort} onSort={toggleSort} />
                <SortableTh label="Standard Number" sortKey="standardNumber" sort={sort} onSort={toggleSort} />
                <SortableTh label="Material" sortKey="material" sort={sort} onSort={toggleSort} />
                {productTypeFilter == null && (
                  <SortableTh label="Type" sortKey="type" sort={sort} onSort={toggleSort} />
                )}
                <SortableTh label="Color" sortKey="color" sort={sort} onSort={toggleSort} />
                <SortableTh label="Weight" sortKey="weight" sort={sort} onSort={toggleSort} />
                {hasStock && (
                  <th className="px-4 py-3 font-medium">{stockLabel}</th>
                )}
                <th className="px-4 py-3 font-medium text-right w-28">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {loading &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={colCount} className="px-4 py-2">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {!loading && filtered.length === 0 && (
                <tr>
                  <td colSpan={colCount} className="px-4 py-8 text-center text-steel-500">
                    No products match your search.
                  </td>
                </tr>
              )}

              {!loading &&
                paged.map((p) => {
                  const isSelected = selectedId != null && Number(p.productID) === Number(selectedId)
                  return (
                    <tr key={p.productID} className="hover:bg-panel-50">
                      <td className="px-4 py-2 font-mono text-steel-800">{p.productID}</td>
                      <td className="px-4 py-2 text-steel-700">{p.standardNumber || '—'}</td>
                      <td className="px-4 py-2 text-steel-700">{p.material || '—'}</td>
                      {productTypeFilter == null && (
                        <td className="px-4 py-2 text-steel-700">{PRODUCT_TYPE_LABELS[p.productType] ?? `Type ${p.productType}`}</td>
                      )}
                      <td className="px-4 py-2 text-steel-700">{p.productColor || '—'}</td>
                      <td className="px-4 py-2 font-mono text-steel-700">{p.weight ?? '—'}</td>
                      {hasStock && (
                        <td className="px-4 py-2 text-steel-700 font-mono">{findProductStock(p.productID, warehouseStock)}</td>
                      )}
                      <td className="px-4 py-2 text-right">
                        <button
                          type="button"
                          onClick={() => handleSelect(p)}
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
