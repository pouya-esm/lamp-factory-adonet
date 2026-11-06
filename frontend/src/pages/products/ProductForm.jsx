import { useState } from 'react'
import Field, { inputClass } from '../../components/ui/Field.jsx'
import Button from '../../components/ui/Button.jsx'
import ErrorBanner from '../../components/ui/ErrorBanner.jsx'
import { PRODUCT_TYPE_OPTIONS, LAMPHOLDER_TYPE_OPTIONS } from './productTypes.js'

// NOTE: the API's JSON is camelCase (productID, standardNumber, ...) — see
// the comment in ProductsPage.jsx. `product` passed in here comes straight
// from that camelCase response.
const emptyForm = {
  productType: PRODUCT_TYPE_OPTIONS[1].value, // default to Component
  productColor: '',
  material: '',
  standardNumber: '',
  weight: '',
  voltage: '',
  power: '',
  lampholderType: '',
  consumptionUnit: ''
}

function toFormState(product) {
  if (!product) return emptyForm
  return {
    productType: product.productType ?? emptyForm.productType,
    productColor: product.productColor ?? '',
    material: product.material ?? '',
    standardNumber: product.standardNumber ?? '',
    weight: product.weight ?? '',
    voltage: product.voltage ?? '',
    power: product.power ?? '',
    lampholderType: product.lampholderType ?? '',
    consumptionUnit: product.consumptionUnit ?? ''
  }
}

// Turns '' back into null for optional numeric/text fields, and casts
// productType to a number, so we don't send strings where the backend
// expects a nullable number or a raw enum int.
function toPayload(form, existingId) {
  const numeric = (v) => (v === '' || v === null ? null : Number(v))
  return {
    ...(existingId ? { productID: existingId } : {}),
    productType: Number(form.productType),
    productColor: form.productColor || null,
    material: form.material || null,
    standardNumber: form.standardNumber || null,
    weight: numeric(form.weight),
    voltage: numeric(form.voltage),
    power: numeric(form.power),
    lampholderType: form.lampholderType || null,
    consumptionUnit: form.consumptionUnit || null
  }
}

export default function ProductForm({ product, onSubmit, onCancel }) {
  const [form, setForm] = useState(() => toFormState(product))
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState(null)

  const isEdit = !!product

  function set(field, value) {
    setForm((f) => ({ ...f, [field]: value }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError(null)
    setSaving(true)
    try {
      await onSubmit(toPayload(form, product?.productID))
    } catch (err) {
      setError(err.message || 'Failed to save product.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <ErrorBanner message={error} />

      <div className="grid grid-cols-2 gap-x-4">
        <Field label="Product Type" required>
          <select
            className={inputClass}
            value={form.productType}
            onChange={(e) => set('productType', Number(e.target.value))}
          >
            {PRODUCT_TYPE_OPTIONS.map((t) => (
              <option key={t.value} value={t.value}>
                {t.label}
              </option>
            ))}
          </select>
        </Field>

        <Field label="Standard Number">
          <input
            className={`${inputClass} font-mono`}
            value={form.standardNumber}
            onChange={(e) => set('standardNumber', e.target.value)}
            placeholder="e.g. LED-A60-9W"
          />
        </Field>

        <Field label="Material">
          <input className={inputClass} value={form.material} onChange={(e) => set('material', e.target.value)} />
        </Field>

        <Field label="Color">
          <input
            className={inputClass}
            value={form.productColor}
            onChange={(e) => set('productColor', e.target.value)}
          />
        </Field>

        <Field label="Weight">
          <input
            type="number"
            step="any"
            className={inputClass}
            value={form.weight}
            onChange={(e) => set('weight', e.target.value)}
          />
        </Field>

        <Field label="Voltage">
          <input
            type="number"
            step="any"
            className={inputClass}
            value={form.voltage}
            onChange={(e) => set('voltage', e.target.value)}
          />
        </Field>

        <Field label="Power">
          <input
            type="number"
            step="any"
            className={inputClass}
            value={form.power}
            onChange={(e) => set('power', e.target.value)}
          />
        </Field>

        <Field label="Lampholder Type">
          <select
            className={inputClass}
            value={form.lampholderType}
            onChange={(e) => set('lampholderType', e.target.value)}
          >
            <option value="">Not specified</option>
            {LAMPHOLDER_TYPE_OPTIONS.map((t) => (
              <option key={t.value} value={t.value}>
                {t.label}
              </option>
            ))}
          </select>
        </Field>

        <Field label="Consumption Unit">
          <input
            className={inputClass}
            value={form.consumptionUnit}
            onChange={(e) => set('consumptionUnit', e.target.value)}
            placeholder="e.g. pcs, g, m"
          />
        </Field>
      </div>

      <div className="flex items-center justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onCancel} disabled={saving}>
          Cancel
        </Button>
        <Button type="submit" disabled={saving}>
          {saving ? 'Saving…' : isEdit ? 'Save changes' : 'Create product'}
        </Button>
      </div>
    </form>
  )
}
