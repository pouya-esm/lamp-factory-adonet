import { useState } from 'react'
import Field, { inputClass } from '../../components/ui/Field.jsx'
import Button from '../../components/ui/Button.jsx'
import ErrorBanner from '../../components/ui/ErrorBanner.jsx'
import { QC_TYPE_OPTIONS } from './qualityControlTypes.js'
import { useProductionDocuments, productionDocumentLabelById } from '../../lib/useProductionDocuments.js'
import ProductionDocumentPicker from '../../components/ProductionDocumentPicker.jsx'
import { toDateInputValue, fromDateInputValue } from '../../lib/date.js'

const emptyForm = {
  qualityControlType: QC_TYPE_OPTIONS[0].value,
  qualityControlDate: '',
  inspectionType: '',
  sampleRatio: '',
  batchNumber: '',
  qualityControlAcceptance: '',
  productionDocumentID: ''
}

function toFormState(qc) {
  if (!qc) return emptyForm
  return {
    qualityControlType: qc.qualityControlType ?? emptyForm.qualityControlType,
    qualityControlDate: toDateInputValue(qc.qualityControlDate),
    inspectionType: qc.inspectionType ?? '',
    sampleRatio: qc.sampleRatio ?? '',
    batchNumber: qc.batchNumber ?? '',
    // Backend wire format for the BIT column is a JSON boolean (true/false);
    // '' keeps the control unset so NULL is written back to the database.
    qualityControlAcceptance: qc.qualityControlAcceptance ?? '',
    productionDocumentID: qc.productionDocumentID ?? ''
  }
}

function toPayload(form, existingId) {
  return {
    ...(existingId ? { qualityControlID: existingId } : {}),
    qualityControlType: Number(form.qualityControlType),
    qualityControlDate: fromDateInputValue(form.qualityControlDate),
    inspectionType: form.inspectionType || null,
    sampleRatio: form.sampleRatio === '' ? null : Number(form.sampleRatio),
    batchNumber: form.batchNumber || null,
    qualityControlAcceptance: form.qualityControlAcceptance === '' ? null : form.qualityControlAcceptance === 'true',
       productionDocumentID: Number(form.productionDocumentID)
  }
}

export default function QualityControlForm({ qc, onSubmit, onCancel }) {
  const [form, setForm] = useState(() => toFormState(qc))
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState(null)
  const [pickerOpen, setPickerOpen] = useState(false)
  const { documents } = useProductionDocuments()

  const isEdit = !!qc

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
      await onSubmit(toPayload(form, qc?.qualityControlID))
    } catch (err) {
      setError(err.message || 'Failed to save quality control record.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <ErrorBanner message={error} />

      <div className="grid grid-cols-2 gap-x-4">
        <Field label="Type" required>
          <select
            className={inputClass}
            value={form.qualityControlType}
            onChange={(e) => set('qualityControlType', Number(e.target.value))}
          >
            {QC_TYPE_OPTIONS.map((t) => (
              <option key={t.value} value={t.value}>
                {t.label}
              </option>
            ))}
          </select>
        </Field>

        <Field label="Inspection Date">
          <input
            type="date"
            className={inputClass}
            value={form.qualityControlDate}
            onChange={(e) => set('qualityControlDate', e.target.value)}
          />
        </Field>

        <Field label="Inspection Type">
          <input
            className={inputClass}
            value={form.inspectionType}
            onChange={(e) => set('inspectionType', e.target.value)}
            placeholder="e.g. Visual, Electrical"
          />
        </Field>

        <Field label="Batch Number">
          <input
            className={`${inputClass} font-mono`}
            value={form.batchNumber}
            onChange={(e) => set('batchNumber', e.target.value)}
          />
        </Field>

        <Field label="Acceptance">
          <select
            className={inputClass}
            value={form.qualityControlAcceptance}
            onChange={(e) => set('qualityControlAcceptance', e.target.value)}
          >
            <option value="">Not inspected yet</option>
            <option value="true">True</option>
            <option value="false">False</option>
          </select>
        </Field>

        <Field label="Sample Ratio">
          <input
            type="number"
            step="any"
            min="0"
            max="1"
            className={inputClass}
            value={form.sampleRatio}
            onChange={(e) => set('sampleRatio', e.target.value)}
            placeholder="e.g. 0.1 for 10%"
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
      </div>

      <div className="flex items-center justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onCancel} disabled={saving}>
          Cancel
        </Button>
        <Button type="submit" disabled={saving}>
          {saving ? 'Saving…' : isEdit ? 'Save changes' : 'Create record'}
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
