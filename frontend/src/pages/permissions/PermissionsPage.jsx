import { useEffect, useMemo, useState } from 'react'
import { Plus, Pencil, Trash2 } from 'lucide-react'
import { rbacApi } from '../../api/rbacApi.js'
import Button from '../../components/ui/Button.jsx'
import Modal from '../../components/ui/Modal.jsx'
import ConfirmDialog from '../../components/ui/ConfirmDialog.jsx'
import ErrorBanner from '../../components/ui/ErrorBanner.jsx'
import FilterBar from '../../components/FilterBar.jsx'
import Pagination from '../../components/ui/Pagination.jsx'
import SortableTh from '../../components/ui/SortableTh.jsx'
import usePagedList from '../../hooks/usePagedList.js'
import useSortable from '../../hooks/useSortable.js'
import AuditStamp from '../../components/AuditStamp.jsx'
import Field, { inputClass } from '../../components/ui/Field.jsx'

export default function PermissionsPage() {
  const [permissions, setPermissions] = useState(null)
  const [resources, setResources] = useState([])
  const [actions, setActions] = useState([])
  const [loadError, setLoadError] = useState(null)
  const [search, setSearch] = useState('')

  const [editingPermission, setEditingPermission] = useState(null)
  const [deletingPermission, setDeletingPermission] = useState(null)
  const [deleteLoading, setDeleteLoading] = useState(false)
  const [deleteError, setDeleteError] = useState(null)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState(null)
  const [formState, setFormState] = useState({ resourceID: '', actionID: '' })

  async function loadAll() {
    setLoadError(null)
    try {
      const [permData, resData, actData] = await Promise.all([
        rbacApi.permissions.getAll(),
        rbacApi.resources.getAll(),
        rbacApi.actions.getAll()
      ])
      setPermissions(Array.isArray(permData) ? permData : [])
      setResources(Array.isArray(resData) ? resData : [])
      setActions(Array.isArray(actData) ? actData : [])
    } catch (err) {
      setLoadError(err.message || 'Failed to load data.')
    }
  }

  useEffect(() => {
    loadAll()
  }, [])

  const filtered = useMemo(() => {
    if (!permissions) return []
    const q = search.trim().toLowerCase()
    if (!q) return permissions
    return permissions.filter((p) => {
      const resName = p.resourceName || p.ResourceName || ''
      const actName = p.actionName || p.ActionName || ''
      return `${resName}.${actName}`.toLowerCase().includes(q)
    })
  }, [permissions, search])

  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    id: (p) => p.PermissionID ?? p.permissionID,
    resource: (p) => p.permissionID ? (p.resourceName || p.ResourceName || '') : '',
    action: (p) => (p.actionName || p.ActionName || '')
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    sort
  ])

  async function handleSave() {
    setFormError(null)
    if (!formState.resourceID || !formState.actionID) {
      setFormError('Please select both a resource and an action.')
      return
    }
    setSaving(true)
    try {
      if (editingPermission) {
        await rbacApi.permissions.update(editingPermission.PermissionID ?? editingPermission.permissionID, {
          ResourceID: Number(formState.resourceID),
          ActionID: Number(formState.actionID)
        })
      } else {
        await rbacApi.permissions.create({
          ResourceID: Number(formState.resourceID),
          ActionID: Number(formState.actionID)
        })
      }
      setEditingPermission(null)
      setFormState({ resourceID: '', actionID: '' })
      await loadAll()
    } catch (err) {
      setFormError(err.message || 'Failed to save permission.')
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete() {
    setDeleteLoading(true)
    setDeleteError(null)
    try {
      await rbacApi.permissions.remove(deletingPermission.PermissionID ?? deletingPermission.permissionID)
      setDeletingPermission(null)
      await loadAll()
    } catch (err) {
      setDeleteError(err.message || 'Failed to delete permission.')
    } finally {
      setDeleteLoading(false)
    }
  }

  function openEdit(permission) {
    setEditingPermission(permission)
    setFormState({
      resourceID: permission.ResourceID ?? permission.resourceID ?? '',
      actionID: permission.ActionID ?? permission.actionID ?? ''
    })
    setFormError(null)
  }

  function openNew() {
    setEditingPermission({})
    setFormState({ resourceID: '', actionID: '' })
    setFormError(null)
  }

  return (
    <div>
      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search by resource.action…"
        actions={
          <Button onClick={openNew}>
            <Plus size={16} />
            New Permission
          </Button>
        }
      />

      <ErrorBanner message={loadError} />

      <div className="rounded-lg border border-panel-300 bg-white overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                <SortableTh label="#" sortKey="id" sort={sort} onSort={toggleSort} />
                <SortableTh label="Resource" sortKey="resource" sort={sort} onSort={toggleSort} />
                <SortableTh label="Action" sortKey="action" sort={sort} onSort={toggleSort} />
                <th className="px-4 py-3 font-medium">Last change</th>
                <th className="px-4 py-3 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {permissions === null &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={5} className="px-4 py-3">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {permissions !== null && filtered.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-10 text-center text-steel-500">
                    {search.trim() ? 'No permissions match your search.' : 'No permissions yet — create the first one.'}
                  </td>
                </tr>
              )}

              {paged?.map((p) => (
                <tr key={p.PermissionID ?? p.permissionID} className="hover:bg-panel-50">
                  <td className="px-4 py-3 font-mono text-steel-800">{p.PermissionID ?? p.permissionID}</td>
                  <td className="px-4 py-3 text-steel-800">{p.resourceName || p.ResourceName}</td>
                  <td className="px-4 py-3 text-steel-700">{p.actionName || p.ActionName}</td>
                                    <AuditStamp
                    createdAt={p.createdAt}
                    createdBy={p.createdBy}
                    modifiedAt={p.modifiedAt}
                    modifiedBy={p.modifiedBy}
                  />
<td className="px-4 py-3">
                    <div className="flex items-center justify-end gap-1">
                      <button
                        onClick={() => openEdit(p)}
                        className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
                        title="Edit"
                      >
                        <Pencil size={16} />
                      </button>
                      <button
                        onClick={() => setDeletingPermission(p)}
                        className="p-1.5 rounded-md text-steel-500 hover:bg-stop-500/10 hover:text-stop-500"
                        title="Delete"
                      >
                        <Trash2 size={16} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      <Pagination
        page={page}
        totalPages={totalPages}
        totalItems={totalItems}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        className="mt-4"
      />

      <Modal open={!!editingPermission} onClose={() => setEditingPermission(null)} title={editingPermission ? 'Edit permission' : 'New permission'}>
        {editingPermission && (
          <div className="space-y-4">
            <ErrorBanner message={formError} />

            <Field label="Resource" required>
              <select
                className={inputClass}
                value={formState.resourceID}
                onChange={(e) => setFormState((f) => ({ ...f, resourceID: e.target.value }))}
              >
                <option value="">Select a resource…</option>
                {resources.map((r) => (
                  <option key={r.resourceID ?? r.ResourceID} value={r.resourceID ?? r.ResourceID}>
                    {r.name ?? r.Name}
                  </option>
                ))}
              </select>
            </Field>

            <Field label="Action" required>
              <select
                className={inputClass}
                value={formState.actionID}
                onChange={(e) => setFormState((f) => ({ ...f, actionID: e.target.value }))}
              >
                <option value="">Select an action…</option>
                {actions.map((a) => (
                  <option key={a.actionID ?? a.ActionID} value={a.actionID ?? a.ActionID}>
                    {a.name ?? a.Name}
                  </option>
                ))}
              </select>
            </Field>

            <div className="flex items-center justify-end gap-2 pt-2">
              <Button type="button" variant="secondary" onClick={() => setEditingPermission(null)} disabled={saving}>
                Cancel
              </Button>
              <Button onClick={handleSave} disabled={saving}>
                {saving ? 'Saving…' : editingPermission ? 'Save changes' : 'Create permission'}
              </Button>
            </div>
          </div>
        )}
      </Modal>

      <ConfirmDialog
        open={!!deletingPermission}
        title="Delete permission"
        message={`Delete permission "${deletingPermission?.resourceName || deletingPermission?.ResourceName || ''}.${deletingPermission?.actionName || deletingPermission?.ActionName || ''}"? This can't be undone.`}
        onCancel={() => {
          setDeletingPermission(null)
          setDeleteError(null)
        }}
        onConfirm={handleDelete}
        loading={deleteLoading}
      />
      {deleteError && <p className="mt-2 text-sm text-stop-500">{deleteError}</p>}
    </div>
  )
}
