import { useEffect, useState } from 'react'
import { reportsApi } from '../../api/reportsApi.js'
import ReportFilterBar from '../../components/ReportFilterBar.jsx'
import ErrorBanner from '../../components/ui/ErrorBanner.jsx'
import Pagination from '../../components/ui/Pagination.jsx'
import SortableTh from '../../components/ui/SortableTh.jsx'
import usePagedList from '../../hooks/usePagedList.js'
import useSortable from '../../hooks/useSortable.js'
import { priorityMeta } from '../production-plans/priority.js'
import { shiftLabel } from '../production-documents/shifts.js'

function formatDate(isoString) {
  if (!isoString) return '—'
  return isoString.slice(0, 10)
}

function PriorityBadge({ priority }) {
  if (priority == null) return <span className="text-steel-500">—</span>
  const meta = priorityMeta(priority)
  return (
    <span className={`inline-flex items-center rounded px-2 py-0.5 text-xs font-medium ${meta.badge}`}>
      {meta.label}
    </span>
  )
}

export default function ProductionTraceabilityPage() {
  const [startDate, setStartDate] = useState('')
  const [standardNumber, setStandardNumber] = useState('')
  const [rows, setRows] = useState(null)
  const [error, setError] = useState(null)

  async function runReport() {
    setError(null)
    try {
      const data = await reportsApi.getProductionTraceability({
        startDate: startDate || undefined,
        standardNumber: standardNumber || undefined
      })
      setRows(Array.isArray(data) ? data : [])
    } catch (err) {
      setError(err.message || 'Failed to load the report.')
    }
  }

  useEffect(() => {
    runReport()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    doc: (r) => r.productionDocumentID,
    product: (r) => r.productName,
    date: (r) => r.productionDate,
    shift: (r) => r.productionShift,
    line: (r) => r.lineNumber,
    produced: (r) => r.producedQuantity,
    rejected: (r) => r.rejectedQuantity,
    operator: (r) => r.operatorName,
    operatorRole: (r) => r.operatorRole,
    planPriority: (r) => r.planPriority,
    targetDate: (r) => r.targetDate
  }
  const sorted = applySort(rows ?? [], sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [sort])

  return (
    <div>
      <ReportFilterBar
        startDate={startDate}
        onStartDateChange={setStartDate}
        standardNumber={standardNumber}
        onStandardNumberChange={setStandardNumber}
        onApply={runReport}
        standardNumberLabel="Product standard number"
      />

      <ErrorBanner message={error} />

      <div className="rounded-lg border border-panel-300 bg-white overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                <SortableTh label="Doc #" sortKey="doc" sort={sort} onSort={toggleSort} />
                <SortableTh label="Product" sortKey="product" sort={sort} onSort={toggleSort} />
                <SortableTh label="Date" sortKey="date" sort={sort} onSort={toggleSort} />
                <SortableTh label="Shift" sortKey="shift" sort={sort} onSort={toggleSort} />
                <SortableTh label="Line" sortKey="line" sort={sort} onSort={toggleSort} />
                <SortableTh label="Produced" sortKey="produced" sort={sort} onSort={toggleSort} />
                <SortableTh label="Rejected" sortKey="rejected" sort={sort} onSort={toggleSort} />
                <SortableTh label="Operator" sortKey="operator" sort={sort} onSort={toggleSort} />
                <SortableTh label="Operator Role" sortKey="operatorRole" sort={sort} onSort={toggleSort} />
                <SortableTh label="Plan Priority" sortKey="planPriority" sort={sort} onSort={toggleSort} />
                <SortableTh label="Target Date" sortKey="targetDate" sort={sort} onSort={toggleSort} />
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {rows === null &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={11} className="px-4 py-3">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {rows !== null && paged.length === 0 && (
                <tr>
                  <td colSpan={11} className="px-4 py-10 text-center text-steel-500">
                    No results for these filters.
                  </td>
                </tr>
              )}

              {paged.map((row, i) => (
                <tr key={i} className="hover:bg-panel-50">
                  <td className="px-4 py-3 font-mono text-steel-800">{row.productionDocumentID}</td>
                  <td className="px-4 py-3 text-steel-700">{row.productName}</td>
                  <td className="px-4 py-3 text-steel-700">{formatDate(row.productionDate)}</td>
                  <td className="px-4 py-3 text-steel-700">{shiftLabel(row.productionShift)}</td>
                  <td className="px-4 py-3 font-mono text-steel-700">
                    L{row.lineNumber ?? '—'}
                  </td>
                  <td className="px-4 py-3 text-steel-700">{row.producedQuantity ?? '—'}</td>
                  <td className="px-4 py-3 text-steel-700">{row.rejectedQuantity ?? '—'}</td>
                  <td className="px-4 py-3 text-steel-700">{row.operatorName || '—'}</td>
                  <td className="px-4 py-3 text-steel-700">{row.operatorRole || '—'}</td>
                  <td className="px-4 py-3">
                    <PriorityBadge priority={row.planPriority} />
                  </td>
                  <td className="px-4 py-3 text-steel-700">{formatDate(row.targetDate)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      <Pagination
        page={page}
        totalPages={totalPages}
        totalItems={totalItems}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        className="mt-4"
      />
    </div>
  )
}
