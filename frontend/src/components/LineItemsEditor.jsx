import { useState } from 'react'
import { Plus, Trash2 } from 'lucide-react'
import { inputClass } from './ui/Field.jsx'
import ProductPicker from './ProductPicker.jsx'

export default function LineItemsEditor({ rows, products, quantityField, quantityLabel = 'Quantity', onChange, warehouseStock, outbound, productTypeFilter }) {
  const [productPickerOpen, setProductPickerOpen] = useState(false)
  const [productPickerIndex, setProductPickerIndex] = useState(null)
  const [quantityErrors, setQuantityErrors] = useState({})

  function updateRow(index, patch) {
    const next = rows.map((row, i) => (i === index ? { ...row, ...patch } : row))
    onChange(next)
  }

  function addRow() {
    onChange([...rows, { productID: '', [quantityField]: '' }])
  }

  function removeRow(index) {
    onChange(rows.filter((_, i) => i !== index))
  }

  function openPicker(index) {
    setProductPickerIndex(index)
    setProductPickerOpen(true)
  }

  function closePicker() {
    setProductPickerOpen(false)
    setProductPickerIndex(null)
  }

  function handleSelectProduct(p) {
    const idx = productPickerIndex
    if (idx != null) {
      updateRow(idx, { productID: Number(p.productID) })
    }
    closePicker()
  }

  function handleQuantityChange(index, value) {
    updateRow(index, { [quantityField]: value })

    if (outbound) {
      const qty = parseInt(value, 10)
      if (isNaN(qty) || qty <= 0) {
        setQuantityErrors((prev) => ({ ...prev, [index]: null }))
        return
      }
      const productID = rows[index]?.productID
      const available = getStock(productID)
      if (available != null && qty > available) {
        setQuantityErrors((prev) => ({ ...prev, [index]: `Cannot exceed available stock (${available})` }))
      } else {
        setQuantityErrors((prev) => ({ ...prev, [index]: null }))
      }
    }
  }

  function getStock(productID) {
    if (!warehouseStock) return null
    const entry = warehouseStock.find((w) => w.productID === productID)
    return entry?.quantity ?? 0
  }

  function getProductLabel(p) {
    if (!p) return '—'
    return p.standardNumber ? `${p.standardNumber} — ${p.material || 'Unnamed'}` : (p.material || `Product ${p.productID}`)
  }

  return (
    <div>
      <div className="rounded-md border border-panel-300 overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
              <th className="px-3 py-2 font-medium">Product</th>
              {warehouseStock != null && (
                <th className="px-3 py-2 font-medium w-32">Current Stock</th>
              )}
              <th className="px-3 py-2 font-medium w-32">{quantityLabel}</th>
              <th className="px-3 py-2 w-10" />
            </tr>
          </thead>
          <tbody className="divide-y divide-panel-200">
            {rows.length === 0 && (
              <tr>
                <td colSpan={warehouseStock != null ? 4 : 3} className="px-3 py-6 text-center text-steel-500">
                  No line items yet.
                </td>
              </tr>
            )}
            {rows.map((row, index) => {
              const p = products?.find((prod) => prod.productID === row.productID)
              const stock = getStock(row.productID)
              const qtyError = quantityErrors[index]
              const hasError = outbound && qtyError != null
              return (
                <tr key={index}>
                  <td className="px-3 py-2">
                    <div className="flex items-center gap-2">
                      <button
                        type="button"
                        onClick={() => openPicker(index)}
                        className="px-3 py-1.5 rounded-md border border-panel-300 bg-white text-sm text-steel-700 hover:bg-panel-100"
                        aria-label="Choose product"
                      >
                        {row.productID ? 'Change' : 'Choose'}
                      </button>
                      {row.productID ? (
                        <span className="font-mono text-steel-700">{getProductLabel(p)}</span>
                      ) : (
                        <span className="text-steel-500">No product selected</span>
                      )}
                    </div>
                  </td>
                  {warehouseStock != null && (
                    <td className="px-3 py-2 text-steel-700 font-mono">
                      {row.productID ? (stock != null ? stock : '—') : '—'}
                      {row.productID && stock != null && p?.consumptionUnit ? ` ${p.consumptionUnit}` : ''}
                    </td>
                  )}
                  <td className="px-3 py-2">
                    <input
                      type="number"
                      min="0"
                      step="any"
                      className={`${inputClass} ${hasError ? 'border-stop-500 focus:ring-stop-500' : ''}`}
                      value={row[quantityField]}
                      onChange={(e) => handleQuantityChange(index, e.target.value)}
                      readOnly={outbound && !row.productID}
                    />
                    {hasError && <p className="mt-1 text-xs text-stop-500">{qtyError}</p>}
                  </td>
                  <td className="px-3 py-2">
                    <button
                      type="button"
                      onClick={() => removeRow(index)}
                      className="p-1.5 rounded-md text-steel-500 hover:bg-stop-500/10 hover:text-stop-500"
                      aria-label="Remove row"
                      title="Remove"
                    >
                      <Trash2 size={16} />
                    </button>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>

      <button
        type="button"
        onClick={addRow}
        className="mt-2 inline-flex items-center gap-1.5 text-sm font-medium text-amber-600 hover:text-amber-700"
      >
        <Plus size={16} />
        Add line item
      </button>

      <ProductPicker
        open={productPickerOpen}
        onClose={closePicker}
        products={products}
        selectedId={rows[productPickerIndex ?? -1]?.productID}
        onSelect={handleSelectProduct}
        productTypeFilter={productTypeFilter}
        warehouseStock={warehouseStock}
      />
    </div>
  )
}
