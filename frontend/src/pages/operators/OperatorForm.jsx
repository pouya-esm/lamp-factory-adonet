import { useState } from 'react'
import Field, { inputClass } from '../../components/ui/Field.jsx'
import Button from '../../components/ui/Button.jsx'
import ErrorBanner from '../../components/ui/ErrorBanner.jsx'
import { toDateInputValue, fromDateInputValue } from '../../lib/date.js'

// API JSON is camelCase (operatorID, nationalID, firstName, lastName, hiringDate).
const emptyForm = {
  nationalID: '',
  firstName: '',
  lastName: '',
  hiringDate: ''
}

function toFormState(operator) {
  if (!operator) return emptyForm
  return {
    nationalID: operator.nationalID ?? '',
    firstName: operator.firstName ?? '',
    lastName: operator.lastName ?? '',
    hiringDate: toDateInputValue(operator.hiringDate)
  }
}

function toPayload(form, existingId) {
  return {
    ...(existingId ? { operatorID: existingId } : {}),
    nationalID: form.nationalID,
    firstName: form.firstName || null,
    lastName: form.lastName || null,
    hiringDate: fromDateInputValue(form.hiringDate)
  }
}

export default function OperatorForm({ operator, onSubmit, onCancel }) {
  const [form, setForm] = useState(() => toFormState(operator))
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState(null)

  const isEdit = !!operator

  function set(field, value) {
    setForm((f) => ({ ...f, [field]: value }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError(null)

    if (!form.nationalID.trim()) {
      setError('National ID is required.')
      return
    }

    setSaving(true)
    try {
      await onSubmit(toPayload(form, operator?.operatorID))
    } catch (err) {
      setError(err.message || 'Failed to save operator.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <ErrorBanner message={error} />

      <Field label="National ID" required>
        <input
          className={`${inputClass} font-mono`}
          value={form.nationalID}
          onChange={(e) => set('nationalID', e.target.value)}
        />
      </Field>

      <div className="grid grid-cols-2 gap-x-4">
        <Field label="First Name">
          <input className={inputClass} value={form.firstName} onChange={(e) => set('firstName', e.target.value)} />
        </Field>

        <Field label="Last Name">
          <input className={inputClass} value={form.lastName} onChange={(e) => set('lastName', e.target.value)} />
        </Field>
      </div>

      <Field label="Hiring Date">
        <input
          type="date"
          className={inputClass}
          value={form.hiringDate}
          onChange={(e) => set('hiringDate', e.target.value)}
        />
      </Field>

      <div className="flex items-center justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onCancel} disabled={saving}>
          Cancel
        </Button>
        <Button type="submit" disabled={saving}>
          {saving ? 'Saving…' : isEdit ? 'Save changes' : 'Create operator'}
        </Button>
      </div>
    </form>
  )
}
