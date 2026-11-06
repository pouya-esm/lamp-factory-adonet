import { useState } from 'react'
import { Plus, Trash2 } from 'lucide-react'
import { inputClass } from '../../components/ui/Field.jsx'
import ProductPicker from '../../components/ProductPicker.jsx'

// Like the shared LineItemsEditor, but Production Plan line items have two
// quantity fields (min/max) instead of one, so it gets its own small editor
// rather than forcing that shape into the generic component. Product selection
// opens a modal picker (Final goods only) instead of a flat dropdown.
export default function ProductionPlanProductsEditor({ rows, products, onChange }) {
  const [pickerOpen, setPickerOpen] = useState(false)
  const [pickerRow, setPickerRow] = useState(null)

  function updateRow(index, patch) {
    onChange(rows.map((row, i) => (i === index ? { ...row, ...patch } : row)))
  }

  function addRow() {
    onChange([...rows, { productID: '', minQuantity: '', maxQuantity: '' }])
  }

  function removeRow(index) {
    onChange(rows.filter((_, i) => i !== index))
  }

  function openPicker(index) {
    setPickerRow(index)
    setPickerOpen(true)
  }

  function closePicker() {
    setPickerOpen(false)
    setPickerRow(null)
  }

  function handleSelect(product) {
    if (pickerRow !== null) {
      updateRow(pickerRow, { productID: Number(product.productID) })
    }
    closePicker()
  }

  function productLabel(id) {
    const p = products?.find((p) => p.productID === id)
    if (!p) return `Product ${id}`
    return p.standardNumber ? `${p.standardNumber} — ${p.material || ''}`.trim() : p.material || `Product ${id}`
  }

  return (
    <div>
      <div className="rounded-md border border-panel-300 overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
              <th className="px-3 py-2 font-medium">Product</th>
              <th className="px-3 py-2 font-medium w-28">Min Qty</th>
              <th className="px-3 py-2 font-medium w-28">Max Qty</th>
              <th className="px-3 py-2 w-10" />
            </tr>
          </thead>
          <tbody className="divide-y divide-panel-200">
            {rows.length === 0 && (
              <tr>
                <td colSpan={4} className="px-3 py-6 text-center text-steel-500">
                  No products in this plan yet.
                </td>
              </tr>
            )}
            {rows.map((row, index) => (
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
                      <span className="font-mono text-steel-700">{productLabel(row.productID)}</span>
                    ) : (
                      <span className="text-steel-500">No product selected</span>
                    )}
                  </div>
                </td>
                <td className="px-3 py-2">
                  <input
                    type="number"
                    min="0"
                    className={inputClass}
                    value={row.minQuantity}
                    onChange={(e) => updateRow(index, { minQuantity: e.target.value })}
                  />
                </td>
                <td className="px-3 py-2">
                  <input
                    type="number"
                    min="0"
                    className={inputClass}
                    value={row.maxQuantity}
                    onChange={(e) => updateRow(index, { maxQuantity: e.target.value })}
                  />
                </td>
                <td className="px-3 py-2">
                  <button
                    type="button"
                    onClick={() => removeRow(index)}
                    className="p-1.5 rounded-md text-steel-500 hover:bg-stop-500/10 hover:text-stop-500"
                    aria-label="Remove row"
                  >
                    <Trash2 size={16} />
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <button
        type="button"
        onClick={addRow}
        className="mt-2 inline-flex items-center gap-1.5 text-sm font-medium text-amber-600 hover:text-amber-700"
      >
        <Plus size={16} />
        Add product
      </button>

      <ProductPicker
        open={pickerOpen}
        onClose={closePicker}
        products={products}
        productTypeFilter={0}
        selectedId={pickerRow !== null ? rows[pickerRow]?.productID || undefined : undefined}
        onSelect={handleSelect}
      />
    </div>
  )
}
