import { useState } from 'react'
import Field, { inputClass } from '../../components/ui/Field.jsx'
import Button from '../../components/ui/Button.jsx'
import ErrorBanner from '../../components/ui/ErrorBanner.jsx'
import { LINE_STATUS_OPTIONS } from './lineStatus.js'

// API JSON is camelCase (productionLineID, status). Status is a NOT NULL BIT
// on the server, so the wire format is a JSON boolean; the <select> below
// shuttles it as a string ('true'/'false') and we convert at the boundaries.
const emptyForm = { status: 'false' } // default Stopped

function toFormState(line) {
  if (!line) return emptyForm
  return { status: line.status === true ? 'true' : 'false' }
}

function toPayload(form, existingId) {
  return {
    ...(existingId ? { productionLineID: existingId } : {}),
    status: form.status === 'true'
  }
}

export default function ProductionLineForm({ line, onSubmit, onCancel }) {
  const [form, setForm] = useState(() => toFormState(line))
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState(null)

  const isEdit = !!line

  async function handleSubmit(e) {
    e.preventDefault()
    setError(null)
    setSaving(true)
    try {
      await onSubmit(toPayload(form, line?.productionLineID))
    } catch (err) {
      setError(err.message || 'Failed to save production line.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <ErrorBanner message={error} />

      <Field label="Status">
        <select
          className={inputClass}
          value={form.status}
          onChange={(e) => setForm({ status: e.target.value })}
        >
          {LINE_STATUS_OPTIONS.map((s) => (
            <option key={String(s.value)} value={String(s.value)}>
              {s.label}
            </option>
          ))}
        </select>
      </Field>

      <div className="flex items-center justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onCancel} disabled={saving}>
          Cancel
        </Button>
        <Button type="submit" disabled={saving}>
          {saving ? 'Saving…' : isEdit ? 'Save changes' : 'Create production line'}
        </Button>
      </div>
    </form>
  )
}
