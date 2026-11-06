import { useUserMap } from '../lib/useUserMap.js'

// Resolves the "Last change" line for one record. The schema keeps a single
// audit stamp per record (no full change history), so this describes only the
// latest state: the record was either edited — show who touched it last — or
// it is still in its original state — show who created it.
//
// Props are the four audit columns as they arrive from the API (camelCase),
// all nullable. `as` selects the wrapper: 'td' for module tables, 'div' for
// card-style modules.
export function AuditStampContent({ createdAt, createdBy, modifiedAt, modifiedBy }) {
  const { label } = useUserMap()

  const edited = modifiedAt != null && createdAt != null && new Date(modifiedAt) > new Date(createdAt)
  const when = edited ? modifiedAt : createdAt
  const whoId = edited ? modifiedBy : createdBy
  const who = edited ? label(modifiedBy) : label(createdBy)

  if (when == null) {
    return <span className="text-xs text-steel-500">No audit info</span>
  }

  const verb = edited ? 'Edited by' : 'Created by'
  const date = when.slice(0, 10)

  return (
    <span className="flex flex-col leading-tight whitespace-nowrap">
      <span className="text-xs text-steel-600">
        {verb} <span className="font-medium text-steel-800">{who ?? `user #${whoId ?? '?'}`}</span>
      </span>
      <span className="text-[11px] text-steel-500">{date}</span>
    </span>
  )
}

export default function AuditStamp({ createdAt, createdBy, modifiedAt, modifiedBy, className = '' }) {
  return (
    <td className={`px-4 py-3 ${className}`}>
      <AuditStampContent
        createdAt={createdAt}
        createdBy={createdBy}
        modifiedAt={modifiedAt}
        modifiedBy={modifiedBy}
      />
    </td>
  )
}
