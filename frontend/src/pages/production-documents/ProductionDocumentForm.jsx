import { useState } from 'react'
import Field, { inputClass } from '../../components/ui/Field.jsx'
import Button from '../../components/ui/Button.jsx'
import ErrorBanner from '../../components/ui/ErrorBanner.jsx'
import OperatorAssignmentsEditor from './OperatorAssignmentsEditor.jsx'
import ProductPicker from '../../components/ProductPicker.jsx'
import ProductionPlanPicker from '../../components/ProductionPlanPicker.jsx'
import ProductionLinePicker from '../../components/ProductionLinePicker.jsx'
import { useProducts } from '../../lib/useProducts.js'
import { useProductionPlans, productionPlanLabel } from '../../lib/useProductionPlans.js'
import { useProductionLines } from '../../lib/useProductionLines.js'
import { useOperators } from '../../lib/useOperators.js'
import { toDateInputValue, fromDateInputValue } from '../../lib/date.js'
import { SHIFT_OPTIONS } from '../production-documents/shifts.js'

// Mirrors the backend's ProductionDocumentDTO: { productionDocument: {...}, operators: [...] }.
function toFormState(entry) {
  if (!entry) {
    return {
      productionDate: '',
      producedQuantity: '',
      rejectedQuantity: '',
      productionShift: '',
      productionPlanID: '',
      productID: '',
      productionLineID: '',
      operators: []
    }
  }
  const d = entry.productionDocument
  return {
    productionDate: toDateInputValue(d.productionDate),
    producedQuantity: d.producedQuantity ?? '',
    rejectedQuantity: d.rejectedQuantity ?? '',
    productionShift: d.productionShift ?? '',
    productionPlanID: d.productionPlanID ?? '',
    productID: d.productID ?? '',
    productionLineID: d.productionLineID ?? '',
    operators: entry.operators.map((o) => ({
      operatorID: o.operatorID,
      operatorRole: o.operatorRole ?? '',
      assignedDateFrom: toDateInputValue(o.assignedDateFrom),
      assignedDateUntil: toDateInputValue(o.assignedDateUntil)
    }))
  }
}

function toPayload(form, existingId) {
  return {
    productionDocument: {
      ...(existingId ? { productionDocumentID: existingId } : {}),
      productionDate: fromDateInputValue(form.productionDate),
      producedQuantity: form.producedQuantity === '' ? null : Number(form.producedQuantity),
      rejectedQuantity: form.rejectedQuantity === '' ? null : Number(form.rejectedQuantity),
      productionShift: form.productionShift === '' ? null : Number(form.productionShift),
       productionPlanID: Number(form.productionPlanID),
      productID: Number(form.productID),
      productionLineID: Number(form.productionLineID)
    },
    operators: form.operators.map((o) => ({
      operatorID: Number(o.operatorID),
      operatorRole: o.operatorRole || null,
      assignedDateFrom: fromDateInputValue(o.assignedDateFrom),
      assignedDateUntil: fromDateInputValue(o.assignedDateUntil)
    }))
  }
}

export default function ProductionDocumentForm({ entry, onSubmit, onCancel }) {
  const [form, setForm] = useState(() => toFormState(entry))
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState(null)
  const { products } = useProducts()
  const { plans } = useProductionPlans()
  const { lines } = useProductionLines()
  const { operators } = useOperators()

  const [planPickerOpen, setPlanPickerOpen] = useState(false)
  const [productPickerOpen, setProductPickerOpen] = useState(false)
  const [linePickerOpen, setLinePickerOpen] = useState(false)

  const isEdit = !!entry

  function set(field, value) {
    setForm((f) => ({ ...f, [field]: value }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError(null)

    if (!form.productionPlanID || !form.productID || !form.productionLineID) {
      setError('Please select a production plan, product, and production line.')
      return
    }
    if (form.operators.some((o) => !o.operatorID)) {
      setError('Every assignment row needs an operator selected.')
      return
    }

    setSaving(true)
    try {
      await onSubmit(toPayload(form, entry?.productionDocument?.productionDocumentID))
    } catch (err) {
      setError(err.message || 'Failed to save production document.')
    } finally {
       setSaving(false)
     }
   }

  function planLabel(id) {
    const plan = plans?.find((p) => p.productionPlanId === id)
    if (!plan) return `Plan ${id || '—'}`
    return productionPlanLabel(plan)
  }

  function productLabel(id) {
    const p = products?.find((p) => p.productID === id)
    if (!p) return `Product ${id || '—'}`
    return p.standardNumber ? `${p.standardNumber} — ${p.material || ''}`.trim() : p.material || `Product ${id}`
  }

  function lineLabel(id) {
    const l = lines?.find((l) => l.productionLineID === id)
    if (!l) return `Line ${id || '—'}`
    return `Line ${l.productionLineID}`
  }

  return (
    <form onSubmit={handleSubmit}>
      <ErrorBanner message={error} />

      <div className="grid grid-cols-3 gap-x-4">
        <Field label="Production Date">
          <input
            type="date"
            className={inputClass}
            value={form.productionDate}
            onChange={(e) => set('productionDate', e.target.value)}
          />
        </Field>

        <Field label="Shift">
          <select
            className={inputClass}
            value={form.productionShift}
            onChange={(e) => set('productionShift', e.target.value)}
          >
            <option value="">No shift</option>
            {SHIFT_OPTIONS.map((s) => (
              <option key={s.value} value={s.value}>
                {s.label}
              </option>
            ))}
          </select>
        </Field>

        <div />

        <Field label="Produced Qty">
          <input
            type="number"
            min="0"
            className={inputClass}
            value={form.producedQuantity}
            onChange={(e) => set('producedQuantity', e.target.value)}
          />
        </Field>

        <Field label="Rejected Qty">
          <input
            type="number"
            min="0"
            className={inputClass}
            value={form.rejectedQuantity}
            onChange={(e) => set('rejectedQuantity', e.target.value)}
          />
        </Field>

        <div />

        <Field label="Production Plan" required>
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={() => setPlanPickerOpen(true)}
              className="px-3 py-1.5 rounded-md border border-panel-300 bg-white text-sm text-steel-700 hover:bg-panel-100"
              aria-label="Choose production plan"
            >
              {form.productionPlanID ? 'Change' : 'Choose'}
            </button>
            {form.productionPlanID ? (
              <span className="font-mono text-steel-700">{planLabel(form.productionPlanID)}</span>
            ) : (
              <span className="text-steel-500">No plan selected</span>
            )}
          </div>
        </Field>

        <Field label="Product" required>
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={() => setProductPickerOpen(true)}
              className="px-3 py-1.5 rounded-md border border-panel-300 bg-white text-sm text-steel-700 hover:bg-panel-100"
              aria-label="Choose product"
            >
              {form.productID ? 'Change' : 'Choose'}
            </button>
            {form.productID ? (
              <span className="font-mono text-steel-700">{productLabel(form.productID)}</span>
            ) : (
              <span className="text-steel-500">No product selected</span>
            )}
          </div>
        </Field>

        <Field label="Production Line" required>
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={() => setLinePickerOpen(true)}
              className="px-3 py-1.5 rounded-md border border-panel-300 bg-white text-sm text-steel-700 hover:bg-panel-100"
              aria-label="Choose production line"
            >
              {form.productionLineID ? 'Change' : 'Choose'}
            </button>
            {form.productionLineID ? (
              <span className="font-mono text-steel-700">{lineLabel(form.productionLineID)}</span>
            ) : (
              <span className="text-steel-500">No line selected</span>
            )}
          </div>
        </Field>
      </div>

      <Field label="Operator assignments">
        <OperatorAssignmentsEditor
          rows={form.operators}
          operators={operators}
          onChange={(rows) => set('operators', rows)}
        />
      </Field>

      <div className="flex items-center justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onCancel} disabled={saving}>
          Cancel
        </Button>
        <Button type="submit" disabled={saving}>
          {saving ? 'Saving…' : isEdit ? 'Save changes' : 'Create document'}
        </Button>
      </div>
      <ProductionPlanPicker
        open={planPickerOpen}
        onClose={() => setPlanPickerOpen(false)}
        plans={plans}
        selectedId={form.productionPlanID || undefined}
        onSelect={(p) => set('productionPlanID', p.productionPlanId)}
      />

      <ProductPicker
        open={productPickerOpen}
        onClose={() => setProductPickerOpen(false)}
        products={products}
        selectedId={form.productID || undefined}
        onSelect={(p) => set('productID', p.productID)}
      />

      <ProductionLinePicker
        open={linePickerOpen}
        onClose={() => setLinePickerOpen(false)}
        lines={lines}
        selectedId={form.productionLineID || undefined}
        onSelect={(l) => set('productionLineID', l.productionLineID)}
      />
    </form>
  )
}
