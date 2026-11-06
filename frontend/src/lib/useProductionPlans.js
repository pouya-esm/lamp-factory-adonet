import { useEffect, useState } from 'react'
import { productionPlansApi } from '../api/productionPlansApi.js'

// GET /api/productionplan returns [{ productionPlan, products }, ...]
// (see ProductionPlanService.GetAllProductionPlansAsync). This flattens that
// down to just the plan objects for use in a <select>.
export function useProductionPlans() {
  const [plans, setPlans] = useState(null)
  const [error, setError] = useState(null)

  useEffect(() => {
    let cancelled = false
    productionPlansApi
      .getAll()
      .then((data) => {
        if (!cancelled) {
          const list = Array.isArray(data) ? data : []
          setPlans(list.map((entry) => entry.productionPlan).filter(Boolean))
        }
      })
      .catch((err) => {
        if (!cancelled) setError(err.message || 'Failed to load production plans.')
      })
    return () => {
      cancelled = true
    }
  }, [])

  return { plans, error }
}

export function productionPlanLabel(plan) {
  if (!plan) return ''
  const target = plan.targetDate ? plan.targetDate.slice(0, 10) : 'no target date'
  return `Plan #${plan.productionPlanId} — ${target}`
}
