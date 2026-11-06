import { useState } from 'react'
import { Plus, Trash2 } from 'lucide-react'
import { inputClass } from '../../components/ui/Field.jsx'
import ProductPicker from '../../components/ProductPicker.jsx'

function productLabel(id, products) {
  const p = products?.find((p) => p.productID === id)
  if (!p) return `Product ${id}`
  return p.standardNumber ? `${p.standardNumber} — ${p.material || ''}`.trim() : p.material || `Product ${id}`
}

// BOM-specific editor for "component + consumption" rows. Mirrors the generic
// LineItemsEditor but swaps the flat product <select> for the modal ProductPicker
// (Component-type products only). Uses its own component so the shared editor
// used by the voucher modules stays untouched.
export default function BOMComponentsEditor({ rows, products, onChange }) {
  const [pickerOpen, setPickerOpen] = useState(false)
  const [pickerRow, setPickerRow] = useState(null)

  function updateRow(index, patch) {
    onChange(rows.map((row, i) => (i === index ? { ...row, ...patch } : row)))
  }

  function addRow() {
    onChange([...rows, { productID: '', consumptionAmount: '' }])
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

  return (
    <div>
      <div className="rounded-md border border-panel-300 overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
              <th className="px-3 py-2 font-medium">Product</th>
              <th className="px-3 py-2 font-medium w-32">Consumption / unit</th>
              <th className="px-3 py-2 font-medium w-24">Unit</th>
              <th className="px-3 py-2 w-10" />
            </tr>
          </thead>
          <tbody className="divide-y divide-panel-200">
            {rows.length === 0 && (
              <tr>
                <td colSpan={4} className="px-3 py-6 text-center text-steel-500">
                  No components in this BOM yet.
                </td>
              </tr>
            )}
            {rows.map((row, index) => {
              const component = products?.find((p) => p.productID === row.productID)
              return (
                <tr key={index}>
                  <td className="px-3 py-2">
                    <div className="flex items-center gap-2">
                      <button
                        type="button"
                        onClick={() => openPicker(index)}
                        className="px-3 py-1.5 rounded-md border border-panel-300 bg-white text-sm text-steel-700 hover:bg-panel-100"
                        aria-label="Choose component"
                      >
                        {row.productID ? 'Change' : 'Choose'}
                      </button>
                      {row.productID ? (
                        <span className="font-mono text-steel-700">{productLabel(row.productID, products)}</span>
                      ) : (
                        <span className="text-steel-500">No product selected</span>
                      )}
                    </div>
                  </td>
                  <td className="px-3 py-2">
                    <input
                      type="number"
                      min="0"
                      step="any"
                      className={inputClass}
                      value={row.consumptionAmount}
                      onChange={(e) => updateRow(index, { consumptionAmount: e.target.value })}
                    />
                  </td>
                  <td className="px-3 py-2 text-steel-700 font-mono">
                    {component?.consumptionUnit || '—'}
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
        Add component
      </button>

      <ProductPicker
        open={pickerOpen}
        onClose={closePicker}
        products={products}
        productTypeFilter={1}
        selectedId={pickerRow !== null ? rows[pickerRow]?.productID || undefined : undefined}
        onSelect={handleSelect}
      />
    </div>
  )
}
