import { useEffect, useState } from 'react'
import { rolesApi } from '../api/rolesApi.js'

export function useRoles() {
  const [roles, setRoles] = useState(null)
  const [error, setError] = useState(null)

  useEffect(() => {
    let cancelled = false
    rolesApi
      .getAll()
      .then((data) => {
        if (!cancelled) setRoles(Array.isArray(data) ? data : [])
      })
      .catch((err) => {
        if (!cancelled) setError(err.message || 'Failed to load roles.')
      })
    return () => {
      cancelled = true
    }
  }, [])

  return { roles, error }
}
