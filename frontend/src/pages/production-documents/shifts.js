export const SHIFT_LABELS = {
  1: 'Morning',
  2: 'Evening',
  3: 'Night'
}

export const SHIFT_OPTIONS = [
  { value: 1, label: 'Morning' },
  { value: 2, label: 'Evening' },
  { value: 3, label: 'Night' }
]

export function shiftLabel(shift) {
  if (shift == null) return '—'
  return SHIFT_LABELS[shift] ?? `Shift ${shift}`
}
