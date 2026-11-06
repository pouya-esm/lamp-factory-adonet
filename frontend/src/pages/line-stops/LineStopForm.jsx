import { useState } from 'react'
import Field, { inputClass } from '../../components/ui/Field.jsx'
import Button from '../../components/ui/Button.jsx'
import ErrorBanner from '../../components/ui/ErrorBanner.jsx'
import { useProductionDocuments, productionDocumentLabelById } from '../../lib/useProductionDocuments.js'
import ProductionDocumentPicker from '../../components/ProductionDocumentPicker.jsx'
import { toDateInputValue, fromDateInputValue } from '../../lib/date.js'

const emptyForm = { lineStopDate: '', reason: '', productionDocumentID: '' }

function toFormState(lineStop) {
  if (!lineStop) return emptyForm
  return {
    lineStopDate: toDateInputValue(lineStop.lineStopDate),
    reason: lineStop.reason ?? '',
    productionDocumentID: lineStop.productionDocumentID ?? ''
  }
}

function toPayload(form, existingId) {
  return {
    ...(existingId ? { lineStopID: existingId } : {}),
    lineStopDate: fromDateInputValue(form.lineStopDate),
    reason: form.reason || null,
       productionDocumentID: Number(form.productionDocumentID)
  }
}

export default function LineStopForm({ lineStop, onSubmit, onCancel }) {
  const [form, setForm] = useState(() => toFormState(lineStop))
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState(null)
  const [pickerOpen, setPickerOpen] = useState(false)
  const { documents } = useProductionDocuments()

  const isEdit = !!lineStop

  function set(field, value) {
    setForm((f) => ({ ...f, [field]: value }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError(null)

    if (!form.productionDocumentID) {
      setError('Please select a production document.')
      return
    }

    setSaving(true)
    try {
      await onSubmit(toPayload(form, lineStop?.lineStopID))
    } catch (err) {
      setError(err.message || 'Failed to save line stop.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <ErrorBanner message={error} />

      <Field label="Stop Date">
        <input
          type="date"
          className={inputClass}
          value={form.lineStopDate}
          onChange={(e) => set('lineStopDate', e.target.value)}
        />
      </Field>

      <Field label="Production Document" required>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={() => setPickerOpen(true)}
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

      <Field label="Reason">
        <textarea
          className={`${inputClass} min-h-[90px] resize-y`}
          value={form.reason}
          onChange={(e) => set('reason', e.target.value)}
          placeholder="What caused the line to stop?"
        />
      </Field>

      <div className="flex items-center justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onCancel} disabled={saving}>
          Cancel
        </Button>
        <Button type="submit" disabled={saving}>
          {saving ? 'Saving…' : isEdit ? 'Save changes' : 'Create line stop'}
        </Button>
      </div>

      <ProductionDocumentPicker
        open={pickerOpen}
        onClose={() => setPickerOpen(false)}
        documents={documents}
        selectedId={form.productionDocumentID || undefined}
        onSelect={(doc) => set('productionDocumentID', Number(doc.productionDocumentID))}
      />
    </form>
  )
}
