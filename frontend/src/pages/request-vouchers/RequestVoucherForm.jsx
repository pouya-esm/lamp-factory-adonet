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

// Mirrors the backend's RequestVoucherProductDTO: { voucher: {...}, products: [...] }.
function toFormState(entry) {
  if (!entry) {
    return { requestDate: '', warehouseID: '', productionDocumentID: '', products: [] }
  }
  return {
    requestDate: toDateInputValue(entry.voucher.requestDate),
    warehouseID: entry.voucher.warehouseID ?? '',
    productionDocumentID: entry.voucher.productionDocumentID ?? '',
    products: entry.products.map((p) => ({ productID: p.productID, requestedQuantity: p.requestedQuantity ?? '' }))
  }
}

function toPayload(form, existingId) {
  return {
    voucher: {
      ...(existingId ? { requestVoucherID: existingId } : {}),
      requestDate: fromDateInputValue(form.requestDate),
       warehouseID: Number(form.warehouseID),
      productionDocumentID: Number(form.productionDocumentID)
    },
    products: form.products.map((p) => ({
      productID: Number(p.productID),
      requestedQuantity: p.requestedQuantity === '' ? null : Number(p.requestedQuantity)
    }))
  }
}

export default function RequestVoucherForm({ entry, onSubmit, onCancel }) {
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

  function getStock(productID) {
    const entry = warehouseStock.find((w) => w.productID === productID)
    return entry?.quantity ?? 0
  }

  function hasStockError() {
    return form.products.some(
      (p) => p.productID && p.requestedQuantity !== '' && Number(p.requestedQuantity) > getStock(p.productID)
    )
  }

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
    if (form.products.some((p) => !p.productID || p.requestedQuantity === '')) {
      setError('Every line item needs a product and a quantity.')
      return
    }
    if (hasStockError()) {
      setError('Requested quantity exceeds available stock for one or more products.')
      return
    }

    setSaving(true)
    try {
      await onSubmit(toPayload(form, entry?.voucher?.requestVoucherID))
    } catch (err) {
      setError(err.message || 'Failed to save request voucher.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <ErrorBanner message={error} />

      <div className="grid grid-cols-2 gap-x-4">
        <Field label="Request Date">
          <input
            type="date"
            className={inputClass}
            value={form.requestDate}
            onChange={(e) => set('requestDate', e.target.value)}
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
          quantityField="requestedQuantity"
          quantityLabel="Requested Qty"
          onChange={(rows) => set('products', rows)}
          warehouseStock={warehouseStock}
          outbound={true}
          productTypeFilter={1}
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
        warehouses={warehouses?.filter((w) => w.warehouseType === 0) || []}
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
