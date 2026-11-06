import { useState, useEffect } from 'react'
import Field, { inputClass } from '../../components/ui/Field.jsx'
import Button from '../../components/ui/Button.jsx'
import ErrorBanner from '../../components/ui/ErrorBanner.jsx'
import { useRoles } from '../../lib/useRoles.js'

function normalizeUser(dto) {
  if (!dto) return { userID: null, username: '', userpass: '', fullname: '', phone: '', roleIds: [] }
  const u = dto.user ?? dto.User ?? {}
  return {
    userID: u.userID ?? u.UserID ?? dto.userID ?? dto.UserID ?? null,
    username: u.username ?? u.Username ?? '',
    userpass: u.userpass ?? u.Userpass ?? '',
    fullname: u.fullname ?? u.Fullname ?? '',
    phone: u.phone ?? u.Phone ?? '',
    roleIds: dto.roleIds ?? dto.RoleIds ?? []
  }
}

const emptyForm = { username: '', userpass: '', fullname: '', phone: '', roleIds: [] }

function toFormState(dto) {
  const u = normalizeUser(dto)
  return {
    username: u.username,
    userpass: u.userpass,
    fullname: u.fullname,
    phone: u.phone,
    roleIds: u.roleIds.map(String)
  }
}

function toPayload(form, existingId) {
  return {
    user: {
      ...(existingId ? { userID: existingId } : {}),
      username: form.username,
      userpass: form.userpass,
      fullname: form.fullname || null,
      phone: form.phone || null
    },
    roleIds: form.roleIds.map(Number).filter(Boolean)
  }
}

export default function UserForm({ user, onSubmit, onCancel }) {
  const [form, setForm] = useState(() => toFormState(user))
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState(null)
  const { roles } = useRoles()

  const isEdit = !!user

  useEffect(() => {
    setForm(toFormState(user))
  }, [user])

  function set(field, value) {
    setForm((f) => ({ ...f, [field]: value }))
  }

  function toggleRole(roleId) {
    const id = String(roleId)
    setForm((prev) => {
      const current = prev.roleIds.includes(id)
        ? prev.roleIds.filter((r) => r !== id)
        : [...prev.roleIds, id]
      return { ...prev, roleIds: current }
    })
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError(null)

    if (!form.username.trim()) {
      setError('Username is required.')
      return
    }
    if (!form.userpass.trim()) {
      setError('Password is required.')
      return
    }
    if (form.roleIds.length === 0) {
      setError('Please select at least one role.')
      return
    }

    setSaving(true)
    try {
      const u = normalizeUser(user)
      await onSubmit(toPayload(form, u.userID))
    } catch (err) {
      setError(err.message || 'Failed to save user.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <ErrorBanner message={error} />

      <div className="grid grid-cols-2 gap-x-4">
        <Field label="Username" required>
          <input className={inputClass} value={form.username} onChange={(e) => set('username', e.target.value)} />
        </Field>

        <Field label="Password" required>
          <input
            type="text"
            className={`${inputClass} font-mono`}
            value={form.userpass}
            onChange={(e) => set('userpass', e.target.value)}
          />
        </Field>

        <Field label="Full Name">
          <input className={inputClass} value={form.fullname} onChange={(e) => set('fullname', e.target.value)} />
        </Field>

        <Field label="Phone">
          <input className={inputClass} value={form.phone} onChange={(e) => set('phone', e.target.value)} />
        </Field>

        <Field label="Roles" required>
          <div className="space-y-2">
            {(roles ?? []).map((r) => (
              <label key={r.roleID} className="flex items-center gap-2">
                <input
                  type="checkbox"
                  className="h-4 w-4 rounded border-panel-300 text-primary"
                  checked={form.roleIds.includes(String(r.roleID))}
                  onChange={() => toggleRole(r.roleID)}
                />
                <span className="text-sm">{r.roleName}</span>
              </label>
            ))}
          </div>
          {form.roleIds.length === 0 && (
            <p className="text-xs text-stop-500 mt-1">Please select at least one role.</p>
          )}
        </Field>
      </div>

      <div className="flex items-center justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onCancel} disabled={saving}>
          Cancel
        </Button>
        <Button type="submit" disabled={saving}>
          {saving ? 'Saving…' : isEdit ? 'Save changes' : 'Create user'}
        </Button>
      </div>
    </form>
  )
}
