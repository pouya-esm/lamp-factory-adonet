import { useEffect, useState } from 'react'
import { productionLinesApi } from '../api/productionLinesApi.js'
import { lineStatusMeta } from '../pages/production-lines/lineStatus.js'

export default function LineStatusStrip() {
  const [lines, setLines] = useState(null)
  const [failed, setFailed] = useState(false)

  useEffect(() => {
    let cancelled = false
    productionLinesApi
      .getAll()
      .then((data) => {
        if (!cancelled) setLines(Array.isArray(data) ? data : [])
      })
      .catch(() => {
        if (!cancelled) setFailed(true)
      })
    return () => {
      cancelled = true
    }
  }, [])

  if (failed || (lines && lines.length === 0)) return null

  return (
    <div className="px-4 py-3 border-t border-steel-700/60">
      <p className="text-[11px] uppercase tracking-wider text-steel-500 font-medium mb-2">Line Status</p>
      <div className="flex flex-wrap gap-2">
        {lines === null &&
          [1, 2, 3].map((i) => <span key={i} className="h-6 w-14 rounded bg-steel-800 animate-pulse" />)}
        {lines?.map((line) => {
          const meta = lineStatusMeta(line.status)
          return (
            <span
              key={line.productionLineID}
              title={meta.label}
              className="flex items-center gap-1.5 rounded bg-steel-800 px-2 py-1 text-xs font-mono text-steel-300"
            >
              <span className={`h-2 w-2 rounded-full ${meta.dot}`} />
              L{line.productionLineID}
            </span>
          )
        })}
      </div>
    </div>
  )
}
