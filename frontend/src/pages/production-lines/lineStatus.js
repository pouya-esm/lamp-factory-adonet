// ProductionLine.Status is a NOT NULL BIT column (false = Stopped, true =
// Running) with a DEFAULT of 0, so a newly created line is always Stopped.
// The API therefore always sends a boolean, never null.
export const LINE_STATUS_OPTIONS = [
  { value: true, label: 'Running' },
  { value: false, label: 'Stopped' }
]

export function lineStatusMeta(status) {
  if (status === true) return { label: 'Running', dot: 'bg-line-500', badge: 'bg-line-500/10 text-line-600' }
  return { label: 'Stopped', dot: 'bg-stop-500', badge: 'bg-stop-500/10 text-stop-500' }
}
