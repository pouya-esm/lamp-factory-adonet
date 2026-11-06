import { Search } from 'lucide-react'
import { inputClass } from './ui/Field.jsx'
import Button from './ui/Button.jsx'

export default function ReportFilterBar({ startDate, onStartDateChange, standardNumber, onStandardNumberChange, onApply, standardNumberLabel = 'Standard number' }) {
  return (
    <form
      onSubmit={(e) => {
        e.preventDefault()
        onApply()
      }}
      className="flex flex-col sm:flex-row sm:items-end gap-3 mb-4"
    >
      <div className="w-full sm:w-48">
        <label className="block text-xs font-medium text-steel-600 mb-1.5">From date</label>
        <input
          type="date"
          className={inputClass}
          value={startDate}
          onChange={(e) => onStartDateChange(e.target.value)}
        />
      </div>

      <div className="w-full sm:w-64">
        <label className="block text-xs font-medium text-steel-600 mb-1.5">{standardNumberLabel}</label>
        <div className="relative">
          <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-steel-500" />
          <input
            className={`${inputClass} pl-8`}
            value={standardNumber}
            onChange={(e) => onStandardNumberChange(e.target.value)}
            placeholder="e.g. STD-L100"
          />
        </div>
      </div>

      <Button type="submit">Apply filters</Button>
    </form>
  )
}
