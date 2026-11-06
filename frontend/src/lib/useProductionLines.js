import { useEffect, useState } from 'react'
import { productionLinesApi } from '../api/productionLinesApi.js'

export function useProductionLines() {
  const [lines, setLines] = useState(null)
  const [error, setError] = useState(null)

  useEffect(() => {
    let cancelled = false
    productionLinesApi
      .getAll()
      .then((data) => {
        if (!cancelled) setLines(Array.isArray(data) ? data : [])
      })
      .catch((err) => {
        if (!cancelled) setError(err.message || 'Failed to load production lines.')
      })
    return () => {
      cancelled = true
    }
  }, [])

  return { lines, error }
}
