import { useState, useMemo } from 'react'
import Field, { inputClass } from '../../components/ui/Field.jsx'
import Button from '../../components/ui/Button.jsx'
import ErrorBanner from '../../components/ui/ErrorBanner.jsx'
import LineItemsEditor from '../../components/LineItemsEditor.jsx'
import WarehousePicker from '../../components/WarehousePicker.jsx'
import ProductionDocumentPicker from '../../components/ProductionDocumentPicker.jsx'
import { useWarehouses, warehouseLabelById } from '../../lib/useWarehouses.js'
import { useProducts } from '../../lib/useProducts.js'
import { useProductionDocuments, productionDocumentLabelById } from '../../lib/useProductionDocuments.js'
import { toDateInputValue, fromDateInputValue } from '../../lib/date.js'

// Mirrors the backend's ChargeVoucherProductDTO: { voucher: {...}, products: [...] }.
function toFormState(entry) {
  if (!entry) {
    return { chargeDate: '', warehouseID: '', productionDocumentID: '', products: [] }
  }
  return {
    chargeDate: toDateInputValue(entry.voucher.chargeDate),
    warehouseID: entry.voucher.warehouseID ?? '',
    productionDocumentID: entry.voucher.productionDocumentID ?? '',
    products: entry.products.map((p) => ({ productID: p.productID, chargeQuantity: p.chargeQuantity ?? '' }))
  }
}

function toPayload(form, existingId) {
  return {
    voucher: {
      ...(existingId ? { chargeVoucherID: existingId } : {}),
      chargeDate: fromDateInputValue(form.chargeDate),
       warehouseID: Number(form.warehouseID),
      productionDocumentID: Number(form.productionDocumentID)
    },
    products: form.products.map((p) => ({
      productID: Number(p.productID),
      chargeQuantity: p.chargeQuantity === '' ? null : Number(p.chargeQuantity)
    }))
  }
}

export default function ChargeVoucherForm({ entry, onSubmit, onCancel }) {
  const [form, setForm] = useState(() => toFormState(entry))
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState(null)
  const [warehousePickerOpen, setWarehousePickerOpen] = useState(false)
  const [docPickerOpen, setDocPickerOpen] = useState(false)
  const { warehouses } = useWarehouses()
  const { products } = useProducts()
  const { documents } = useProductionDocuments()

  const isEdit = !!entry

  const warehouseStock = useMemo(
    () => warehouses?.find((w) => w.warehouseID === form.warehouseID)?.products || [],
    [warehouses, form.warehouseID]
  )

  function set(field, value) {
    setForm((f) => ({ ...f, [field]: value }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError(null)

    if (!form.warehouseID) {
      setError('Please select a warehouse.')
      return
    }
    if (!form.productionDocumentID) {
      setError('Please select a production document.')
      return
    }
    if (form.products.length === 0) {
      setError('Add at least one product line item.')
      return
    }
    if (form.products.some((p) => !p.productID || p.chargeQuantity === '')) {
      setError('Every line item needs a product and a quantity.')
      return
    }

    setSaving(true)
    try {
      await onSubmit(toPayload(form, entry?.voucher?.chargeVoucherID))
    } catch (err) {
      setError(err.message || 'Failed to save charge voucher.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <ErrorBanner message={error} />

      <div className="grid grid-cols-2 gap-x-4">
        <Field label="Charge Date">
          <input
            type="date"
            className={inputClass}
            value={form.chargeDate}
            onChange={(e) => set('chargeDate', e.target.value)}
          />
        </Field>

        <Field label="Warehouse" required>
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={() => setWarehousePickerOpen(true)}
              className="px-3 py-1.5 rounded-md border border-panel-300 bg-white text-sm text-steel-700 hover:bg-panel-100"
              aria-label="Choose warehouse"
            >
              {form.warehouseID ? 'Change' : 'Choose'}
            </button>
            {form.warehouseID ? (
              <span className="font-mono text-steel-700">
                {warehouseLabelById(form.warehouseID, warehouses)}
              </span>
            ) : (
              <span className="text-steel-500">No warehouse selected</span>
            )}
          </div>
        </Field>

        <Field label="Production Document" required>
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={() => setDocPickerOpen(true)}
              className="px-3 py-1.5 rounded-md border border-panel-300 bg-white text-sm text-steel-700 hover:bg-panel-100"
              aria-label="Choose production document"
            >
              {form.productionDocumentID ? 'Change' : 'Choose'}
            </button>
            {form.productionDocumentID ? (
              <span className="font-mono text-steel-700">
                {productionDocumentLabelById(form.productionDocumentID, documents)}
              </span>
            ) : (
              <span className="text-steel-500">No document selected</span>
            )}
          </div>
        </Field>
      </div>

      <Field label="Line items" required>
          <LineItemsEditor
          rows={form.products}
          products={products}
          quantityField="chargeQuantity"
          quantityLabel="Charge Qty"
          onChange={(rows) => set('products', rows)}
          warehouseStock={warehouseStock}
          outbound={false}
          productTypeFilter={0}
        />
      </Field>

      <div className="flex items-center justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onCancel} disabled={saving}>
          Cancel
        </Button>
        <Button type="submit" disabled={saving}>
          {saving ? 'Saving…' : isEdit ? 'Save changes' : 'Create voucher'}
        </Button>
      </div>

      <WarehousePicker
        open={warehousePickerOpen}
        onClose={() => setWarehousePickerOpen(false)}
        warehouses={warehouses?.filter((w) => w.warehouseType === 1) || []}
        selectedId={form.warehouseID || undefined}
        onSelect={(w) => set('warehouseID', Number(w.warehouseID))}
        hideType
      />

      <ProductionDocumentPicker
        open={docPickerOpen}
        onClose={() => setDocPickerOpen(false)}
        documents={documents}
        selectedId={form.productionDocumentID || undefined}
        onSelect={(doc) => set('productionDocumentID', Number(doc.productionDocumentID))}
      />
    </form>
  )
}
