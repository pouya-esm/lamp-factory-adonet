import { useEffect, useState } from 'react'
import { apiClient } from '../api/client.js'

// GET /api/productiondocument returns [{ productionDocument, operators }, ...]
// (see ProductionDocumentService.GetAllDocumentsAsync). This flattens that
// down to just the document objects, since most forms only need to pick a
// ProductionDocumentID and show a short label for it.
export function useProductionDocuments() {
  const [documents, setDocuments] = useState(null)
  const [error, setError] = useState(null)

  useEffect(() => {
    let cancelled = false
    apiClient
      .get('/productiondocument')
      .then((data) => {
        if (!cancelled) {
          const list = Array.isArray(data) ? data : []
          setDocuments(list.map((entry) => entry.productionDocument).filter(Boolean))
        }
      })
      .catch((err) => {
        if (!cancelled) setError(err.message || 'Failed to load production documents.')
      })
    return () => {
      cancelled = true
    }
  }, [])

  return { documents, error }
}

export function productionDocumentLabel(doc) {
  if (!doc) return ''
  const date = doc.productionDate ? doc.productionDate.slice(0, 10) : 'no date'
  return `PD #${doc.productionDocumentID} — ${date}`
}

export function productionDocumentLabelById(id, documents) {
  if (!id) return ''
  const doc = documents?.find((d) => d.productionDocumentID === id)
  return productionDocumentLabel(doc) || `PD #${id}`
}
