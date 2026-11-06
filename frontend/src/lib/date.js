// The backend serializes DateTime as an ISO string (e.g. "2026-03-01T00:00:00").
// <input type="date"> needs just the "yyyy-MM-dd" part, and expects that same
// format back on save. These helpers convert both directions so every
// module's form can reuse them instead of re-deriving this each time.

export function toDateInputValue(isoString) {
  if (!isoString) return ''
  return isoString.slice(0, 10)
}

export function fromDateInputValue(value) {
  return value || null
}
