import { Layers } from 'lucide-react'

// Nested sub-table shown when a voucher row is expanded. `items` are the
// line items ({ productID, [quantityField]: number }), `products` is the
// full product list (from useProducts) used to look up standard number,
// material, and unit for display.
export default function LineItemsDetail({ items, products, quantityField, quantityLabel = 'Quantity' }) {
  function findProduct(id) {
    return products?.find((p) => p.productID === id)
  }

  if (!items || items.length === 0) {
    return <p className="ml-8 py-3 text-sm text-steel-500">No line items.</p>
  }

  return (
    <div className="ml-8 rounded-md border border-panel-300 bg-white overflow-hidden">
      <div className="flex items-center gap-1.5 px-3 py-2 border-b border-panel-200 text-xs uppercase tracking-wider text-steel-500 font-medium">
        <Layers size={13} />
        Line items
      </div>
      <table className="w-full text-sm">
        <thead>
          <tr className="text-left text-xs text-steel-500">
            <th className="px-3 py-2 font-medium">Standard Number</th>
            <th className="px-3 py-2 font-medium">Material</th>
            <th className="px-3 py-2 font-medium text-right">{quantityLabel}</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-panel-200">
          {items.map((item, i) => {
            const p = findProduct(item.productID)
            return (
              <tr key={i}>
                <td className="px-3 py-2 font-mono text-steel-800">
                  {p?.standardNumber || `Product ${item.productID}`}
                </td>
                <td className="px-3 py-2 text-steel-700">{p?.material || '—'}</td>
                <td className="px-3 py-2 text-steel-700 text-right font-mono">
                  {item[quantityField] ?? '—'}
                  {p?.consumptionUnit ? ` ${p.consumptionUnit}` : ''}
                </td>
              </tr>
            )
          })}
        </tbody>
      </table>
    </div>
  )
}
