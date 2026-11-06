import { useEffect, useState } from 'react'
import { operatorsApi } from '../api/operatorsApi.js'

export function useOperators() {
  const [operators, setOperators] = useState(null)
  const [error, setError] = useState(null)

  useEffect(() => {
    let cancelled = false
    operatorsApi
      .getAll()
      .then((data) => {
        if (!cancelled) setOperators(Array.isArray(data) ? data : [])
      })
      .catch((err) => {
        if (!cancelled) setError(err.message || 'Failed to load operators.')
      })
    return () => {
      cancelled = true
    }
  }, [])

  return { operators, error }
}

export function operatorName(op) {
  if (!op) return ''
  return [op.firstName, op.lastName].filter(Boolean).join(' ') || `Operator ${op.operatorID}`
}
