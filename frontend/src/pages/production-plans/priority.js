// ProductionPlan.Priority is stored as a plain int (1-3) in both the DB and
// backend, with no C# enum for it. The mapping is: 1=Low, 2=Normal, 3=High.
export const PRIORITY_OPTIONS = [
  { value: 1, label: 'Low' },
  { value: 2, label: 'Normal' },
  { value: 3, label: 'High' }
]

const PRIORITY_META = {
  1: { label: 'Low', badge: 'bg-steel-500/10 text-steel-600' },
  2: { label: 'Normal', badge: 'bg-line-500/10 text-line-600' },
  3: { label: 'High', badge: 'bg-stop-500/10 text-stop-500' }
}

export function priorityMeta(priority) {
  return PRIORITY_META[priority] ?? { label: priority != null ? `Priority ${priority}` : '—', badge: 'bg-panel-200 text-steel-600' }
}
