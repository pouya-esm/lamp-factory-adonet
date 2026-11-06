import { useState } from 'react'
import { Plus, Trash2 } from 'lucide-react'
import { inputClass } from '../../components/ui/Field.jsx'
import { operatorName } from '../../lib/useOperators.js'
import OperatorPicker from '../../components/OperatorPicker.jsx'

// Production document line items are operator assignments, not
// product+quantity rows, so this gets its own editor rather than reusing
// LineItemsEditor.
export default function OperatorAssignmentsEditor({ rows, operators, onChange }) {
  const [pickerOpen, setPickerOpen] = useState(false)
  const [pickerRow, setPickerRow] = useState(null)

  function updateRow(index, patch) {
    onChange(rows.map((row, i) => (i === index ? { ...row, ...patch } : row)))
  }

  function addRow() {
    onChange([...rows, { operatorID: '', operatorRole: '', assignedDateFrom: '', assignedDateUntil: '' }])
  }

  function removeRow(index) {
    onChange(rows.filter((_, i) => i !== index))
  }

  function openPicker(index) {
    setPickerRow(index)
    setPickerOpen(true)
  }

  function closePicker() {
    setPickerOpen(false)
    setPickerRow(null)
  }

  function handleSelect(op) {
    if (pickerRow !== null) {
      updateRow(pickerRow, { operatorID: Number(op.operatorID) })
    }
    closePicker()
  }

  return (
    <div>
      <div className="rounded-md border border-panel-300 overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
              <th className="px-3 py-2 font-medium">Operator</th>
              <th className="px-3 py-2 font-medium">Role</th>
              <th className="px-3 py-2 font-medium w-36">From</th>
              <th className="px-3 py-2 font-medium w-36">Until</th>
              <th className="px-3 py-2 w-10" />
            </tr>
          </thead>
          <tbody className="divide-y divide-panel-200">
            {rows.length === 0 && (
              <tr>
                <td colSpan={5} className="px-3 py-6 text-center text-steel-500">
                  No operators assigned yet.
                </td>
              </tr>
            )}
            {rows.map((row, index) => (
              <tr key={index}>
                 <td className="px-3 py-2">
                   <div className="flex items-center gap-2">
                     <button
                       type="button"
                       onClick={() => openPicker(index)}
                       className="px-3 py-1.5 rounded-md border border-panel-300 bg-white text-sm text-steel-700 hover:bg-panel-100"
                       aria-label="Choose operator"
                     >
                       {row.operatorID ? 'Change' : 'Choose'}
                     </button>
                     {row.operatorID ? (
                       <span className="font-mono text-steel-700">
                         {operators?.find((o) => o.operatorID === row.operatorID)
                           ? operatorName(operators.find((o) => o.operatorID === row.operatorID))
                           : `Operator ${row.operatorID}`}
                       </span>
                     ) : (
                       <span className="text-steel-500">No operator selected</span>
                     )}
      <OperatorPicker
        open={pickerOpen}
        onClose={closePicker}
        operators={operators}
        selectedId={pickerRow !== null ? rows[pickerRow]?.operatorID || undefined : undefined}
        onSelect={handleSelect}
      />
    </div>
                 </td>
                <td className="px-3 py-2">
                  <input
                    className={inputClass}
                    value={row.operatorRole}
                    onChange={(e) => updateRow(index, { operatorRole: e.target.value })}
                    placeholder="e.g. Line lead"
                  />
                </td>
                <td className="px-3 py-2">
                  <input
                    type="date"
                    className={inputClass}
                    value={row.assignedDateFrom}
                    onChange={(e) => updateRow(index, { assignedDateFrom: e.target.value })}
                  />
                </td>
                <td className="px-3 py-2">
                  <input
                    type="date"
                    className={inputClass}
                    value={row.assignedDateUntil}
                    onChange={(e) => updateRow(index, { assignedDateUntil: e.target.value })}
                  />
                </td>
                <td className="px-3 py-2">
                  <button
                    type="button"
                    onClick={() => removeRow(index)}
                    className="p-1.5 rounded-md text-steel-500 hover:bg-stop-500/10 hover:text-stop-500"
                    aria-label="Remove row"
                  >
                    <Trash2 size={16} />
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <button
        type="button"
        onClick={addRow}
        className="mt-2 inline-flex items-center gap-1.5 text-sm font-medium text-amber-600 hover:text-amber-700"
      >
        <Plus size={16} />
        Assign operator
      </button>
    </div>
  )
}
