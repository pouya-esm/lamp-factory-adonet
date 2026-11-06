import { useState } from 'react'
import Field, { inputClass } from '../../components/ui/Field.jsx'
import Button from '../../components/ui/Button.jsx'
import ErrorBanner from '../../components/ui/ErrorBanner.jsx'
import { WAREHOUSE_TYPE_OPTIONS } from './warehouseTypes.js'

function toFormState(entry) {
  if (!entry) {
    return {
      warehouseType: WAREHOUSE_TYPE_OPTIONS[0].value,
      name: '',
      address: ''
    }
  }
  return {
    warehouseType: entry.warehouseType ?? WAREHOUSE_TYPE_OPTIONS[0].value,
    name: entry.name ?? '',
    address: entry.address ?? ''
  }
}

function toPayload(form, existingId) {
  return {
    warehouse: {
      ...(existingId ? { warehouseID: existingId } : {}),
      warehouseType: Number(form.warehouseType),
      name: form.name || null,
      address: form.address || null
    },
    products: []
  }
}

export default function WarehouseForm({ warehouse, onSubmit, onCancel }) {
  const [form, setForm] = useState(() => toFormState(warehouse))
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState(null)

  const isEdit = !!warehouse

  function set(field, value) {
    setForm((f) => ({ ...f, [field]: value }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError(null)

    if (!form.name) {
      setError('Please enter a warehouse name.')
      return
    }

    setSaving(true)
    try {
      await onSubmit(toPayload(form, warehouse?.warehouseID))
    } catch (err) {
      setError(err.message || 'Failed to save warehouse.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <ErrorBanner message={error} />

      <Field label="Warehouse Type" required>
        <select
          className={inputClass}
          value={form.warehouseType}
          onChange={(e) => set('warehouseType', Number(e.target.value))}
        >
          {WAREHOUSE_TYPE_OPTIONS.map((t) => (
            <option key={t.value} value={t.value}>
              {t.label}
            </option>
          ))}
        </select>
      </Field>

      <Field label="Name" required>
        <input className={inputClass} value={form.name} onChange={(e) => set('name', e.target.value)} />
      </Field>

      <Field label="Address">
        <textarea
          className={`${inputClass} min-h-[80px] resize-y`}
          value={form.address}
          onChange={(e) => set('address', e.target.value)}
        />
      </Field>

      <div className="flex items-center justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onCancel} disabled={saving}>
          Cancel
        </Button>
        <Button type="submit" disabled={saving}>
          {saving ? 'Saving…' : isEdit ? 'Save changes' : 'Create warehouse'}
        </Button>
      </div>
    </form>
  )
}
