import { useState, useEffect, useMemo } from 'react'
import { Search } from 'lucide-react'
import Modal from './ui/Modal.jsx'
import SortableTh from './ui/SortableTh.jsx'
import useSortable from '../hooks/useSortable.js'
import { inputClass } from './ui/Field.jsx'
import ProductDetailsModal from './ProductDetailsModal.jsx'

export default function VoucherProductsModal({ open, onClose, voucherId, voucherLabel, items, products, quantityField, quantityLabel = 'Quantity', productLink = false }) {
  const [search, setSearch] = useState('')
  const [viewingProductId, setViewingProductId] = useState(null)
  const { sort, toggleSort, applySort } = useSortable()

  useEffect(() => {
    if (!open) {
      setSearch('')
      setViewingProductId(null)
    }
  }, [open])

  function findProduct(id) {
    return products?.find((p) => p.productID === id)
  }

  const filtered = useMemo(() => {
    if (!open) return []
    const q = search.trim().toLowerCase()
    if (!q) return Array.isArray(items) ? [...items] : []
    return Array.isArray(items)
      ? items.filter((item) => {
          const p = findProduct(item.productID)
          return [
            p?.standardNumber,
            p?.material,
            p?.productColor,
            String(item.productID),
            String(item[quantityField])
          ]
            .filter(Boolean)
            .some((v) => String(v).toLowerCase().includes(q))
        })
      : []
  }, [items, search, products, quantityField, open])

  const sortedItems = applySort(filtered, {
    id: (item) => String(item.productID),
    standardNumber: (item) => findProduct(item.productID)?.standardNumber || '',
    material: (item) => findProduct(item.productID)?.material || '',
    color: (item) => findProduct(item.productID)?.productColor || '',
    quantity: (item) => Number(item[quantityField] ?? 0)
  })

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={`Products for ${voucherLabel} #${voucherId}`}
      maxWidth="max-w-3xl"
    >
      <div>
        <div className="mb-4 relative">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-steel-500" />
          <input
            className={`${inputClass} pl-9`}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search by standard number, material, color…"
          />
        </div>
        {sortedItems.length === 0 ? (
          <p className="py-4 text-center text-steel-500">
            {search ? 'No products match your search.' : 'No line items.'}
          </p>
        ) : (
          <div className="border border-panel-300 rounded-md overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                    <SortableTh label="#" sortKey="id" sort={sort} onSort={toggleSort} className="w-10" />
                    <SortableTh label="Standard Number" sortKey="standardNumber" sort={sort} onSort={toggleSort} />
                    <SortableTh label="Material" sortKey="material" sort={sort} onSort={toggleSort} />
                    <SortableTh label="Color" sortKey="color" sort={sort} onSort={toggleSort} />
                    <SortableTh label="Voltage / Power" sortKey="standardNumber" sort={sort} onSort={toggleSort} />
                    <SortableTh label="Unit" sortKey="standardNumber" sort={sort} onSort={toggleSort} />
                    <SortableTh label={`${quantityLabel}`} sortKey="quantity" sort={sort} onSort={toggleSort} className="text-right" />
                  </tr>
                </thead>
                <tbody className="divide-y divide-panel-200">
                  {sortedItems.map((item, i) => {
                    const p = findProduct(item.productID)
                    return (
                      <tr key={`${item.productID}-${i}`} className="hover:bg-panel-50">
                        <td className="px-4 py-2 font-mono text-steel-800">{p?.productID ?? item.productID}</td>
                        <td className="px-4 py-2 font-mono text-steel-800">
                          {productLink ? (
                            <button
                              type="button"
                              onClick={() => setViewingProductId(item.productID)}
                              className="font-mono text-amber-600 hover:text-amber-700 hover:underline"
                              title="View product details"
                            >
                              {p?.standardNumber || `Product ${item.productID}`}
                            </button>
                          ) : (
                            p?.standardNumber || `Product ${item.productID}`
                          )}
                        </td>
                        <td className="px-4 py-2 text-steel-700">{p?.material || '—'}</td>
                        <td className="px-4 py-2 text-steel-700">{p?.productColor || '—'}</td>
                        <td className="px-4 py-2 text-steel-700">
                          {p?.voltage ?? '—'} V / {p?.power ?? '—'} W
                        </td>
                        <td className="px-4 py-2 text-steel-700">{p?.consumptionUnit || '—'}</td>
                        <td className="px-4 py-2 text-steel-700 text-right font-mono">
                          {item[quantityField] ?? '—'}
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>

      {productLink && (
        <ProductDetailsModal
          productId={viewingProductId}
          open={viewingProductId != null}
          onClose={() => setViewingProductId(null)}
        />
      )}
    </Modal>
  )
}
