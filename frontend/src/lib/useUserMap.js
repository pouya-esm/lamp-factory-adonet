import { useEffect, useState } from 'react'
import { usersApi } from '../api/usersApi.js'

// Resolves user ids to usernames for the audit stamps. The audit columns store
// only actor ids, so the "Edited by …" line needs this lookup. A user deleted
// after writing a row simply won't be in the map, and the stamp falls back to
// showing the raw id ("user #7") rather than a blank.
//
// The list is fetched once and cached in module scope: every module table
// renders an AuditStamp, and refetching per page would mean 18 identical
// requests on every navigation.
let cachedUsers = null

function normalizeUsername(dto) {
  const u = dto?.user ?? dto?.User ?? dto
  return u?.username ?? u?.Username ?? null
}

function normalizeId(dto) {
  const u = dto?.user ?? dto?.User ?? dto
  return u?.userID ?? u?.UserID ?? null
}

export function useUserMap() {
  const [users, setUsers] = useState(cachedUsers)

  useEffect(() => {
    if (cachedUsers) return
    let cancelled = false
    usersApi
      .getAll()
      .then((data) => {
        const map = new Map()
        for (const dto of Array.isArray(data) ? data : []) {
          const id = normalizeId(dto)
          const username = normalizeUsername(dto)
          if (id != null && username) map.set(Number(id), username)
        }
        if (!cancelled) {
          cachedUsers = map
          setUsers(map)
        }
      })
      .catch(() => {
        if (!cancelled) setUsers(new Map())
      })
    return () => {
      cancelled = true
    }
  }, [])

  // id → "@username", or "#id" when the user no longer exists (deleted actor).
  const label = (id) => {
    if (id == null) return null
    const n = Number(id)
    const username = users?.get(n)
    return username ? `@${username}` : `user #${n}`
  }

  return { users, label }
}
