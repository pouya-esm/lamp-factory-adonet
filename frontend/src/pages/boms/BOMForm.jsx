import { useState } from 'react'
import Field from '../../components/ui/Field.jsx'
import Button from '../../components/ui/Button.jsx'
import ErrorBanner from '../../components/ui/ErrorBanner.jsx'
import ProductPicker from '../../components/ProductPicker.jsx'
import BOMComponentsEditor from './BOMComponentsEditor.jsx'
import { useProducts } from '../../lib/useProducts.js'

// NOTE ON CASING (confirmed against a real Scalar response, don't re-guess
// this): System.Text.Json's camelCase policy lowercases a whole leading run
// of uppercase letters EXCEPT it keeps the last one capitalized if that run
// isn't immediately followed by a lowercase letter. So:
//   "BOM"     -> "bom"     (the whole word lowercases, nothing follows it)
//   "BOM_ID"  -> "boM_ID"  (the "M" survives because "_" comes right after it)
// i.e. the wrapper key is "bom", but the id field inside it is "boM_ID".
function toFormState(entry) {
  if (!entry) {
    return { productID: '', components: [] }
  }
  return {
    productID: entry.bom.productID ?? '',
    components: entry.components.map((c) => ({ productID: c.productID, consumptionAmount: c.consumptionAmount ?? '' }))
  }
}

function toPayload(form, existingId) {
  return {
    bom: {
       ...(existingId ? { boM_ID: existingId } : {}),
      productID: Number(form.productID)
    },
    components: form.components.map((c) => ({
      productID: Number(c.productID),
      consumptionAmount: c.consumptionAmount === '' ? null : Number(c.consumptionAmount)
    }))
  }
}

function productLabel(id, products) {
  const p = products?.find((p) => p.productID === id)
  if (!p) return `Product ${id}`
  return p.standardNumber ? `${p.standardNumber} — ${p.material || ''}`.trim() : p.material || `Product ${id}`
}

export default function BOMForm({ entry, onSubmit, onCancel }) {
  const [form, setForm] = useState(() => toFormState(entry))
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState(null)
  const [pickerOpen, setPickerOpen] = useState(false)
  const { products } = useProducts()

  const isEdit = !!entry

  async function handleSubmit(e) {
    e.preventDefault()
    setError(null)

    if (!form.productID) {
      setError('Please select the product this BOM builds.')
      return
    }
    if (form.components.length === 0) {
      setError('Add at least one component.')
      return
    }
    if (form.components.some((c) => !c.productID || c.consumptionAmount === '')) {
      setError('Every component needs a product and a consumption amount.')
      return
    }

    setSaving(true)
    try {
      await onSubmit(toPayload(form, entry?.bom?.boM_ID))
    } catch (err) {
      setError(err.message || 'Failed to save BOM.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <ErrorBanner message={error} />

      <Field label="Product (what this BOM builds)" required>
        {isEdit ? (
          // The product a BOM builds is fixed once the BOM exists — re-pointing it
          // would silently change what every component row means, so on edit we
          // show the value read-only instead of offering the "Change" picker.
          <div className="py-1.5">
            {form.productID ? (
              <span className="font-mono text-steel-700">{productLabel(form.productID, products)}</span>
            ) : (
              <span className="text-steel-500">No product selected</span>
            )}
          </div>
        ) : (
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={() => setPickerOpen(true)}
              className="px-3 py-1.5 rounded-md border border-panel-300 bg-white text-sm text-steel-700 hover:bg-panel-100"
              aria-label="Choose product"
            >
              {form.productID ? 'Change' : 'Choose'}
            </button>
            {form.productID ? (
              <span className="font-mono text-steel-700">{productLabel(form.productID, products)}</span>
            ) : (
              <span className="text-steel-500">No product selected</span>
            )}
          </div>
        )}
      </Field>

      <Field label="Components" required>
        <BOMComponentsEditor
          rows={form.components}
          products={products}
          onChange={(rows) => setForm((f) => ({ ...f, components: rows }))}
        />
      </Field>

      <div className="flex items-center justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onCancel} disabled={saving}>
          Cancel
        </Button>
        <Button type="submit" disabled={saving}>
          {saving ? 'Saving…' : isEdit ? 'Save changes' : 'Create BOM'}
        </Button>
      </div>

      {!isEdit && (
        <ProductPicker
          open={pickerOpen}
          onClose={() => setPickerOpen(false)}
          products={products}
          productTypeFilter={0}
          selectedId={form.productID || undefined}
          onSelect={(p) => setForm((f) => ({ ...f, productID: Number(p.productID) }))}
        />
      )}
    </form>
  )
}
