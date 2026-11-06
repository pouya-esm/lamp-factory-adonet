import { useCallback, useEffect, useState } from 'react'
import { productsApi } from '../api/productsApi.js'

export function useProducts() {
  const [products, setProducts] = useState(null)
  const [error, setError] = useState(null)

  const load = useCallback(async () => {
    setError(null)
    try {
      const data = await productsApi.getAll()
      setProducts(Array.isArray(data) ? data : [])
    } catch (err) {
      setError(err.message || 'Failed to load products.')
    }
  }, [])

  useEffect(() => {
    load()
  }, [load])

  return { products, error, refresh: load }
}
