import { useEffect, useState } from 'react'
import { auditApi } from '../../api/auditApi.js'
import ErrorBanner from '../../components/ui/ErrorBanner.jsx'
import FilterBar from '../../components/FilterBar.jsx'
import Pagination from '../../components/ui/Pagination.jsx'
import SortableTh from '../../components/ui/SortableTh.jsx'
import usePagedList from '../../hooks/usePagedList.js'
import useSortable from '../../hooks/useSortable.js'

// Admin-only audit log. Every business record contributes exactly one row —
// its latest state change — because the schema tracks a single audit stamp per
// entity rather than a full change history. Read-only by design.
export default function AuditLogPage() {
  const [rows, setRows] = useState(null)
  const [entityNames, setEntityNames] = useState([])
  const [loadError, setLoadError] = useState(null)
  const [search, setSearch] = useState('')
  const [entity, setEntity] = useState('')

  async function loadAll() {
    setLoadError(null)
    try {
      const [data, names] = await Promise.all([auditApi.getAll(), auditApi.getEntityNames()])
      setRows(Array.isArray(data?.items) ? data.items : Array.isArray(data) ? data : [])
      setEntityNames(Array.isArray(names) ? names : [])
    } catch (err) {
      setLoadError(err.message || 'Failed to load the audit log.')
    }
  }

  useEffect(() => {
    loadAll()
  }, [])

  const filtered = rows
    ? rows.filter((r) => {
        if (entity && r.entityName !== entity) return false
        const q = search.trim().toLowerCase()
        if (!q) return true
        return [r.entityName, r.entityLabel, r.lastByUsername, r.lastAction]
          .filter(Boolean)
          .some((v) => v.toLowerCase().includes(q))
      })
    : []

  const isFiltered = search.trim() !== '' || entity !== ''
  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    entity: (r) => r.entityName,
    record: (r) => r.entityLabel,
    action: (r) => r.lastAction,
    user: (r) => r.lastByUsername,
    at: (r) => (r.lastAt ? new Date(r.lastAt).getTime() : null)
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    entity,
    sort
  ])

  return (
    <div>
      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search by entity, record, user…"
        filters={[
          {
            name: 'entity',
            label: 'Entity',
            type: 'select',
            value: entity,
            placeholder: 'All entities',
            options: entityNames.map((n) => ({ value: n, label: n }))
          }
        ]}
        onFilterChange={(name, value) => setEntity(value)}
        onReset={() => {
          setSearch('')
          setEntity('')
        }}
      />

      <ErrorBanner message={loadError} />

      <div className="rounded-lg border border-panel-300 bg-white overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                <SortableTh label="Entity" sortKey="entity" sort={sort} onSort={toggleSort} />
                <SortableTh label="Record" sortKey="record" sort={sort} onSort={toggleSort} />
                <SortableTh label="Action" sortKey="action" sort={sort} onSort={toggleSort} />
                <SortableTh label="User" sortKey="user" sort={sort} onSort={toggleSort} />
                <SortableTh label="Timestamp" sortKey="at" sort={sort} onSort={toggleSort} />
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {rows === null &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={5} className="px-4 py-3">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {rows !== null && filtered.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-10 text-center text-steel-500">
                    {isFiltered ? 'No audit entries match your filters.' : 'No audited records yet.'}
                  </td>
                </tr>
              )}

              {paged.map((r) => (
                <tr
                  key={`${r.entityName}-${r.entityID}`}
                  className="hover:bg-panel-50"
                >
                  <td className="px-4 py-3 font-medium text-steel-800">{r.entityName}</td>
                  <td className="px-4 py-3 text-steel-700">{r.entityLabel || `#${r.entityID}`}</td>
                  <td className="px-4 py-3">
                    <span
                      className={`inline-flex items-center rounded px-2 py-0.5 text-xs font-medium ${
                        r.lastAction === 'Modified'
                          ? 'bg-amber-500/10 text-amber-600'
                          : 'bg-line-500/10 text-line-600'
                      }`}
                    >
                      {r.lastAction}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-steel-700">
                    {r.lastByUsername ? `@${r.lastByUsername}` : `user #${r.lastBy ?? '?'}`}
                  </td>
                  <td className="px-4 py-3 text-steel-700 whitespace-nowrap">
                    {r.lastAt ? r.lastAt.slice(0, 16).replace('T', ' ') : '—'}
                  </td>
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
