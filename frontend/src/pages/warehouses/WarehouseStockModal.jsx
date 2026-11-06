import { useState, useEffect, useMemo } from 'react'
import { Search } from 'lucide-react'
import Modal from '../../components/ui/Modal.jsx'
import { inputClass } from '../../components/ui/Field.jsx'
import Button from '../../components/ui/Button.jsx'
import SortableTh from '../../components/ui/SortableTh.jsx'
import useSortable from '../../hooks/useSortable.js'
import { useProducts } from '../../lib/useProducts.js'
import { WAREHOUSE_TYPE_LABELS } from './warehouseTypes.js'

  function typeLabel(type) {
    return WAREHOUSE_TYPE_LABELS[type] ?? `Type ${type}`
  }

export default function WarehouseStockModal({ open, onClose, warehouse }) {
  const { products } = useProducts()
  const [search, setSearch] = useState('')
  const { sort, toggleSort, applySort } = useSortable()

  // Read-only stock view: the stock items are copied from the warehouse row
  // purely to drive search/sort locally. Nothing here mutates stock.
  const stockItems = useMemo(
    () => (warehouse?.products || []).map((p) => ({ productID: p.productID, quantity: p.quantity ?? 0 })),
    [warehouse]
  )

  useEffect(() => {
    if (warehouse) {
      setSearch('')
    }
  }, [warehouse])

  function findProduct(id) {
    return products?.find((p) => p.productID === id)
  }

  const displayedItems = useMemo(() => {
    let items = stockItems
    const q = search.trim().toLowerCase()
    if (q) {
      items = items.filter((item) => {
        const p = findProduct(item.productID)
        return [
          p?.standardNumber,
          p?.material,
          p?.productColor,
          p?.lampholderType,
          String(item.productID)
        ]
          .filter(Boolean)
          .some((v) => String(v).toLowerCase().includes(q))
      })
    }
    const sorted = applySort(items, {
      standardNumber: (item) => findProduct(item.productID)?.standardNumber || '',
      material: (item) => findProduct(item.productID)?.material || '',
      color: (item) => findProduct(item.productID)?.productColor || '',
      quantity: (item) => Number(item.quantity ?? 0)
    })
    return sorted
  }, [stockItems, search, products, sort])

  const wh = warehouse

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={`Stock for "${wh?.name || ''}"`}
      maxWidth="max-w-5xl"
      wide
    >
      <div>
        {wh && (
          <div className="mb-4 text-sm text-steel-600">
            Type: <span className="font-medium">{typeLabel(wh.warehouseType)}</span>
            {wh.address && (
              <>
                {' · '}
                Address: <span className="font-medium">{wh.address}</span>
              </>
            )}
          </div>
        )}

        <div className="mb-4 flex items-center gap-3">
          <div className="relative flex-1">
            <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-steel-500" />
            <input
              className={`${inputClass} pl-9`}
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search by standard number, material, color…"
            />
          </div>
        </div>

        <div className="border border-panel-300 rounded-md overflow-hidden mb-3">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                  <SortableTh label="Standard Number" sortKey="standardNumber" sort={sort} onSort={toggleSort} />
                  <SortableTh label="Material" sortKey="material" sort={sort} onSort={toggleSort} />
                  <SortableTh label="Color" sortKey="color" sort={sort} onSort={toggleSort} />
                  <SortableTh label="Weight" sortKey="standardNumber" sort={sort} onSort={toggleSort} />
                  <SortableTh label="Voltage / Power" sortKey="standardNumber" sort={sort} onSort={toggleSort} />
                  <SortableTh label="Unit" sortKey="standardNumber" sort={sort} onSort={toggleSort} />
                  <SortableTh label="Quantity" sortKey="quantity" sort={sort} onSort={toggleSort} />
                </tr>
              </thead>
              <tbody className="divide-y divide-panel-200">
                {displayedItems.length === 0 && (
                  <tr>
                    <td colSpan={7} className="px-4 py-8 text-center text-steel-500">
                      {search.trim() ? 'No products match your search.' : 'This warehouse has no stock yet.'}
                    </td>
                  </tr>
                )}

                {displayedItems.map((item, i) => {
                  const p = findProduct(item.productID)
                  return (
                    <tr key={i} className="hover:bg-panel-50">
                      <td className="px-4 py-2 font-mono text-steel-800">
                        {p?.standardNumber || `Product ${item.productID}`}
                      </td>
                      <td className="px-4 py-2 text-steel-700">{p?.material || '—'}</td>
                      <td className="px-4 py-2 text-steel-700">{p?.productColor || '—'}</td>
                      <td className="px-4 py-2 text-steel-700">{p?.weight ?? '—'}</td>
                      <td className="px-4 py-2 text-steel-700">
                        {p?.voltage ?? '—'} V / {p?.power ?? '—'} W
                      </td>
                      <td className="px-4 py-2 text-steel-700">{p?.consumptionUnit || '—'}</td>
                      <td className="px-4 py-2 text-steel-700 font-mono">{item.quantity ?? '—'}</td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        </div>

        <div className="flex items-center justify-end">
          <Button type="button" variant="secondary" onClick={onClose}>
            Close
          </Button>
        </div>
      </div>
    </Modal>
  )
}
