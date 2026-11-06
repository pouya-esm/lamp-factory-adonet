import { useState, useMemo } from 'react'
import { Search } from 'lucide-react'
import Field, { inputClass } from '../../components/ui/Field.jsx'
import Button from '../../components/ui/Button.jsx'
import ErrorBanner from '../../components/ui/ErrorBanner.jsx'

const emptyForm = { roleName: '', description: '', permissionIds: [] }

function toFormState(role) {
  if (!role) return emptyForm
  return {
    roleName: role.roleName ?? '',
    description: role.description ?? '',
    permissionIds: role.permissionIds ?? []
  }
}

function toPayload(form, existingId) {
  return {
    ...(existingId ? { roleID: existingId } : {}),
    roleName: form.roleName,
    description: form.description || null,
    permissionIds: form.permissionIds
  }
}

export default function RoleForm({ role, permissions, onSubmit, onCancel }) {
  const [form, setForm] = useState(() => toFormState(role))
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState(null)
  const [permSearch, setPermSearch] = useState('')

  const isEdit = !!role

  function set(field, value) {
    setForm((f) => ({ ...f, [field]: value }))
  }

  function togglePermission(permId) {
    setForm((prev) => {
      const ids = prev.permissionIds.includes(permId)
        ? prev.permissionIds.filter((id) => id !== permId)
        : [...prev.permissionIds, permId]
      return { ...prev, permissionIds: ids }
    })
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError(null)

    if (!form.roleName.trim()) {
      setError('Role name is required.')
      return
    }

    setSaving(true)
    try {
      await onSubmit(toPayload(form, role?.roleID))
    } catch (err) {
      setError(err.message || 'Failed to save role.')
    } finally {
      setSaving(false)
    }
  }

  const filteredPermissions = useMemo(() => {
    if (!permissions) return []
    const q = permSearch.trim().toLowerCase()
    if (!q) return permissions
      return permissions.filter((p) => {
      const permName = `${p.resourceName}.${p.actionName}`.toLowerCase()
      return (
        permName.includes(q) ||
        p.resourceName?.toLowerCase().includes(q) ||
        p.actionName?.toLowerCase().includes(q)
      )
    })
  }, [permissions, permSearch])

  return (
    <form onSubmit={handleSubmit}>
      <ErrorBanner message={error} />

      <Field label="Role Name" required>
        <input className={inputClass} value={form.roleName} onChange={(e) => set('roleName', e.target.value)} />
      </Field>

      <Field label="Description">
        <textarea
          className={`${inputClass} min-h-[80px] resize-y`}
          value={form.description}
          onChange={(e) => set('description', e.target.value)}
        />
      </Field>

      {permissions && permissions.length > 0 && (
        <Field label="Permissions">
          <div className="mb-2 relative">
            <Search size={16} className="absolute left-2 top-1/2 -translate-y-1/2 text-steel-400" />
            <input
              className={`${inputClass} pl-8`}
              placeholder="Search permissions by resource or action…"
              value={permSearch}
              onChange={(e) => setPermSearch(e.target.value)}
            />
          </div>

          <div className="border border-panel-300 rounded-lg max-h-72 overflow-y-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500 sticky top-0">
                  <th className="px-3 py-2 w-4">#</th>
                  <th className="px-3 py-2">Resource</th>
                  <th className="px-3 py-2">Action</th>
                  <th className="px-3 py-2 w-16 text-center">Selected</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-panel-200">
                {filteredPermissions.map((p) => {
                  const permId = p.permissionID ?? p.PermissionID
                  const checked = form.permissionIds.includes(permId)
                  return (
                    <tr
                      key={permId}
                      className={`cursor-pointer hover:bg-panel-50 transition-colors ${checked ? 'bg-primary/5' : ''}`}
                      onClick={() => togglePermission(permId)}
                    >
                      <td className="px-3 py-2 text-steel-400">{permId}</td>
                      <td className="px-3 py-2 text-steel-800 font-medium">{p.resourceName}</td>
                      <td className="px-3 py-2 text-steel-700">{p.actionName}</td>
                      <td className="px-3 py-2 text-center">
                        <input
                          type="checkbox"
                          className="h-4 w-4 rounded border-panel-300 text-primary"
                          checked={checked}
                          onChange={(e) => {
                            e.stopPropagation()
                            togglePermission(permId)
                          }}
                          onClick={(e) => e.stopPropagation()}
                        />
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
          <p className="text-xs text-steel-500 mt-1">
            {form.permissionIds.length} permission{form.permissionIds.length !== 1 ? 's' : ''} selected (out of {filteredPermissions.length} shown)
          </p>
        </Field>
      )}

      <div className="flex items-center justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onCancel} disabled={saving}>
          Cancel
        </Button>
        <Button type="submit" disabled={saving}>
          {saving ? 'Saving…' : isEdit ? 'Save changes' : 'Create role'}
        </Button>
      </div>
    </form>
  )
}
