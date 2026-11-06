import { useEffect, useState } from 'react'
import Modal from './ui/Modal.jsx'
import { productionPlansApi } from '../api/productionPlansApi.js'
import { priorityMeta } from '../pages/production-plans/priority.js'
import { useProducts } from '../lib/useProducts.js'

function formatDate(isoString) {
  if (!isoString) return '—'
  return isoString.slice(0, 10)
}

// Read-only view of a single Production Plan, opened from a linked plan
// reference in the Quality Control detail panel. GET /api/productionplan/{id}
// returns { productionPlan, products }, where products is the list of
// ProductionPlanProduct junction rows.
export default function ProductionPlanDetailsModal({ planId, open, onClose }) {
  const [plan, setPlan] = useState(null)
  const [items, setItems] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)
  const { products } = useProducts()

  useEffect(() => {
    if (!open || !planId) return
    setLoading(true)
    setError(null)
    productionPlansApi
      .getById(planId)
      .then((data) => {
        setPlan(data.productionPlan)
        setItems(data.products ?? [])
      })
      .catch((err) => setError(err.message || 'Failed to load production plan.'))
      .finally(() => setLoading(false))
  }, [open, planId])

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={plan ? `Production Plan ${plan.productionPlanId}` : 'Production Plan'}
      wide
    >
      {loading && <p className="py-4 text-sm text-steel-500">Loading…</p>}
      {error && <p className="py-4 text-sm text-stop-500">{error}</p>}
      {!loading && !error && plan && (
        <div className="space-y-4">
          <div className="border border-panel-300 rounded-md overflow-hidden">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                  <th className="px-4 py-2 font-medium">Field</th>
                  <th className="px-4 py-2 font-medium">Value</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-panel-200">
                <tr>
                  <td className="px-4 py-2 font-medium text-steel-500">Plan ID</td>
                  <td className="px-4 py-2 font-mono text-steel-800">{plan.productionPlanId}</td>
                </tr>
                <tr>
                  <td className="px-4 py-2 font-medium text-steel-500">Issue Date</td>
                  <td className="px-4 py-2 text-steel-700">{formatDate(plan.issueDate)}</td>
                </tr>
                <tr>
                  <td className="px-4 py-2 font-medium text-steel-500">Target Date</td>
                  <td className="px-4 py-2 text-steel-700">{formatDate(plan.targetDate)}</td>
                </tr>
                <tr>
                  <td className="px-4 py-2 font-medium text-steel-500">Priority</td>
                  <td className="px-4 py-2">
                    <span
                      className={`inline-flex items-center rounded px-2 py-0.5 text-xs font-medium ${priorityMeta(plan.priority).badge}`}
                    >
                      {priorityMeta(plan.priority).label}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <div>
            <p className="text-xs uppercase tracking-wider text-steel-500 font-medium mb-2">
              Products in this plan
            </p>
            {items.length === 0 ? (
              <p className="text-sm text-steel-500">No products in this plan.</p>
            ) : (
              <div className="rounded-md border border-panel-300 overflow-hidden">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="bg-panel-100 text-left text-xs text-steel-500">
                      <th className="px-3 py-2 font-medium">Standard Number</th>
                      <th className="px-3 py-2 font-medium">Material</th>
                      <th className="px-3 py-2 font-medium text-right">Min Qty</th>
                      <th className="px-3 py-2 font-medium text-right">Max Qty</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-panel-200">
                    {items.map((item, i) => {
                      const p = products?.find((prod) => prod.productID === item.productID)
                      return (
                        <tr key={i}>
                          <td className="px-3 py-2 font-mono text-steel-800">
                            {p?.standardNumber || `Product ${item.productID}`}
                          </td>
                          <td className="px-3 py-2 text-steel-700">{p?.material || '—'}</td>
                          <td className="px-3 py-2 text-right font-mono text-steel-700">
                            {item.minQuantity ?? '—'}
                          </td>
                          <td className="px-3 py-2 text-right font-mono text-steel-700">
                            {item.maxQuantity ?? '—'}
                          </td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      )}
    </Modal>
  )
}
