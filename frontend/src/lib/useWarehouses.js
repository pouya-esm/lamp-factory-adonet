import { useEffect, useState } from 'react'
import { warehousesApi } from '../api/warehousesApi.js'
import { WAREHOUSE_TYPE_LABELS } from '../pages/warehouses/warehouseTypes.js'

function normalizeWarehouse(entry) {
  if (!entry) return null
  if (entry.warehouse) {
    return {
      ...entry.warehouse,
      products: Array.isArray(entry.products) ? entry.products : []
    }
  }
  return entry
}

export function useWarehouses() {
  const [warehouses, setWarehouses] = useState(null)
  const [error, setError] = useState(null)

  useEffect(() => {
    let cancelled = false
    warehousesApi
      .getAll()
      .then((data) => {
        if (!cancelled) {
          const list = Array.isArray(data) ? data : []
          setWarehouses(list.map(normalizeWarehouse))
        }
      })
      .catch((err) => {
        if (!cancelled) setError(err.message || 'Failed to load warehouses.')
      })
    return () => {
      cancelled = true
    }
  }, [])

  return { warehouses, error }
}

export function warehouseLabel(w) {
  if (!w) return ''
  const typeLabel = WAREHOUSE_TYPE_LABELS[w.warehouseType] ?? `Type ${w.warehouseType}`
  return `${w.name || 'Warehouse'} — ${typeLabel}`
}

export function warehouseLabelById(id, warehouses) {
  if (!id) return ''
  const w = warehouses?.find((w) => w.warehouseID === id)
  return warehouseLabel(w) || `Warehouse ${id}`
}
