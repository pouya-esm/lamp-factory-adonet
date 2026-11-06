import { useEffect, useState } from 'react'
import Modal from './ui/Modal.jsx'
import { productsApi } from '../api/productsApi.js'
import { PRODUCT_TYPE_LABELS, LAMPHOLDER_TYPE_LABELS } from '../pages/products/productTypes.js'

// Read-only view of a single Product, opened from a linked product reference
// in other modules (Production Documents, Quality Control). Pass a productId
// and the modal fetches the full record itself so callers don't have to thread
// the whole product list through just to show a label.
export default function ProductDetailsModal({ productId, open, onClose }) {
  const [product, setProduct] = useState(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)

  useEffect(() => {
    if (!open || !productId) return
    setLoading(true)
    setError(null)
    productsApi
      .getById(productId)
      .then((data) => setProduct(data))
      .catch((err) => setError(err.message || 'Failed to load product.'))
      .finally(() => setLoading(false))
  }, [open, productId])

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={product ? `Product ${product.productID}` : 'Product'}
      wide
    >
      {loading && <p className="py-4 text-sm text-steel-500">Loading…</p>}
      {error && <p className="py-4 text-sm text-stop-500">{error}</p>}
      {!loading && !error && product && (
        <div className="border border-panel-300 rounded-md overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                  <th className="px-4 py-2 font-medium">Field</th>
                  <th className="px-4 py-2 font-medium">Value</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-panel-200">
                <tr>
                  <td className="px-4 py-2 font-medium text-steel-500">Product ID</td>
                  <td className="px-4 py-2 font-mono text-steel-800">{product.productID}</td>
                </tr>
                <tr>
                  <td className="px-4 py-2 font-medium text-steel-500">Standard Number</td>
                  <td className="px-4 py-2 font-mono text-steel-800">{product.standardNumber || '—'}</td>
                </tr>
                <tr>
                  <td className="px-4 py-2 font-medium text-steel-500">Product Type</td>
                  <td className="px-4 py-2 text-steel-700">
                    {PRODUCT_TYPE_LABELS[product.productType] ?? product.productType ?? '—'}
                  </td>
                </tr>
                <tr>
                  <td className="px-4 py-2 font-medium text-steel-500">Material</td>
                  <td className="px-4 py-2 text-steel-700">{product.material || '—'}</td>
                </tr>
                <tr>
                  <td className="px-4 py-2 font-medium text-steel-500">Color</td>
                  <td className="px-4 py-2 text-steel-700">{product.productColor || '—'}</td>
                </tr>
                <tr>
                  <td className="px-4 py-2 font-medium text-steel-500">Weight</td>
                  <td className="px-4 py-2 font-mono text-steel-700">{product.weight ?? '—'}</td>
                </tr>
                <tr>
                  <td className="px-4 py-2 font-medium text-steel-500">Voltage</td>
                  <td className="px-4 py-2 font-mono text-steel-700">{product.voltage ?? '—'}</td>
                </tr>
                <tr>
                  <td className="px-4 py-2 font-medium text-steel-500">Power</td>
                  <td className="px-4 py-2 font-mono text-steel-700">{product.power ?? '—'}</td>
                </tr>
                <tr>
                  <td className="px-4 py-2 font-medium text-steel-500">Lampholder Type</td>
                  <td className="px-4 py-2 text-steel-700">
                    {product.lampholderType ? LAMPHOLDER_TYPE_LABELS[product.lampholderType] ?? product.lampholderType : '—'}
                  </td>
                </tr>
                <tr>
                  <td className="px-4 py-2 font-medium text-steel-500">Consumption Unit</td>
                  <td className="px-4 py-2 text-steel-700">{product.consumptionUnit || '—'}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      )}
    </Modal>
  )
}
