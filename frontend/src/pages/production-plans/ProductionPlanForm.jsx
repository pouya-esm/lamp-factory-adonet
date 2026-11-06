import { useState } from 'react'
import Field, { inputClass } from '../../components/ui/Field.jsx'
import Button from '../../components/ui/Button.jsx'
import ErrorBanner from '../../components/ui/ErrorBanner.jsx'
import ProductionPlanProductsEditor from './ProductionPlanProductsEditor.jsx'
import { PRIORITY_OPTIONS } from './priority.js'
import { useProducts } from '../../lib/useProducts.js'
import { toDateInputValue, fromDateInputValue } from '../../lib/date.js'

// Mirrors the backend's ProductionPlanDTO: { productionPlan: {...}, products: [...] }.
function toFormState(entry) {
  if (!entry) {
    return { issueDate: '', targetDate: '', priority: 2, products: [] }
  }
  return {
    issueDate: toDateInputValue(entry.productionPlan.issueDate),
    targetDate: toDateInputValue(entry.productionPlan.targetDate),
    priority: entry.productionPlan.priority ?? '',
    products: entry.products.map((p) => ({
      productID: p.productID,
      minQuantity: p.minQuantity ?? '',
      maxQuantity: p.maxQuantity ?? ''
    }))
  }
}

function toPayload(form, existingId) {
  return {
    productionPlan: {
      ...(existingId ? { productionPlanId: existingId } : {}),
      issueDate: fromDateInputValue(form.issueDate),
      targetDate: fromDateInputValue(form.targetDate),
       priority: form.priority === '' ? null : Number(form.priority)
    },
    products: form.products.map((p) => ({
      productID: Number(p.productID),
      minQuantity: p.minQuantity === '' ? null : Number(p.minQuantity),
      maxQuantity: p.maxQuantity === '' ? null : Number(p.maxQuantity)
    }))
  }
}

export default function ProductionPlanForm({ entry, onSubmit, onCancel }) {
  const [form, setForm] = useState(() => toFormState(entry))
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState(null)
  const { products } = useProducts()

  const isEdit = !!entry

  async function handleSubmit(e) {
    e.preventDefault()
    setError(null)

    if (form.products.length === 0) {
      setError('Add at least one product to the plan.')
      return
    }
    if (form.products.some((p) => !p.productID)) {
      setError('Every row needs a product selected.')
      return
    }

    setSaving(true)
    try {
      await onSubmit(toPayload(form, entry?.productionPlan?.productionPlanId))
    } catch (err) {
      setError(err.message || 'Failed to save production plan.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <ErrorBanner message={error} />

      <div className="grid grid-cols-3 gap-x-4">
        <Field label="Issue Date">
          <input
            type="date"
            className={inputClass}
            value={form.issueDate}
            onChange={(e) => setForm((f) => ({ ...f, issueDate: e.target.value }))}
          />
        </Field>

        <Field label="Target Date">
          <input
            type="date"
            className={inputClass}
            value={form.targetDate}
            onChange={(e) => setForm((f) => ({ ...f, targetDate: e.target.value }))}
          />
        </Field>

        <Field label="Priority">
          <select
            className={inputClass}
            value={form.priority}
            onChange={(e) => setForm((f) => ({ ...f, priority: Number(e.target.value) }))}
          >
            {PRIORITY_OPTIONS.map((p) => (
              <option key={p.value} value={p.value}>
                {p.label}
              </option>
            ))}
          </select>
        </Field>
      </div>

      <Field label="Products" required>
        <ProductionPlanProductsEditor
          rows={form.products}
          products={products}
          onChange={(rows) => setForm((f) => ({ ...f, products: rows }))}
        />
      </Field>

      <div className="flex items-center justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onCancel} disabled={saving}>
          Cancel
        </Button>
        <Button type="submit" disabled={saving}>
          {saving ? 'Saving…' : isEdit ? 'Save changes' : 'Create plan'}
        </Button>
      </div>
    </form>
  )
}
