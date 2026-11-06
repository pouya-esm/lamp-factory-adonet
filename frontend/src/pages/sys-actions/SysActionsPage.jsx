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

export default function SysActionsPage() {
  const [actions, setActions] = useState(null)
  const [loadError, setLoadError] = useState(null)
  const [search, setSearch] = useState('')

  const [editingAction, setEditingAction] = useState(null)
  const [deletingAction, setDeletingAction] = useState(null)
  const [deleteLoading, setDeleteLoading] = useState(false)
  const [deleteError, setDeleteError] = useState(null)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState(null)
  const [formName, setFormName] = useState('')

  async function loadActions() {
    setLoadError(null)
    try {
      const data = await rbacApi.actions.getAll()
      setActions(Array.isArray(data) ? data : [])
    } catch (err) {
      setLoadError(err.message || 'Failed to load actions.')
    }
  }

  useEffect(() => {
    loadActions()
  }, [])

  const filtered = useMemo(() => {
    if (!actions) return []
    const q = search.trim().toLowerCase()
    if (!q) return actions
    return actions.filter((a) =>
      [a.name, a.Name].filter(Boolean).some((v) => v.toLowerCase().includes(q))
    )
  }, [actions, search])

  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    id: (a) => a.actionID ?? a.ActionID,
    name: (a) => a.name ?? a.Name
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    sort
  ])

  async function handleSave() {
    setFormError(null)
    if (!formName.trim()) {
      setFormError('Action name is required.')
      return
    }
    setSaving(true)
    try {
      if (editingAction) {
        await rbacApi.actions.update(editingAction.actionID ?? editingAction.ActionID, { name: formName })
      } else {
        await rbacApi.actions.create({ name: formName })
      }
      setEditingAction(null)
      setFormName('')
      await loadActions()
    } catch (err) {
      setFormError(err.message || 'Failed to save action.')
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete() {
    setDeleteLoading(true)
    setDeleteError(null)
    try {
      await rbacApi.actions.remove(deletingAction.actionID ?? deletingAction.ActionID)
      setDeletingAction(null)
      await loadActions()
    } catch (err) {
      setDeleteError(err.message || 'Failed to delete action.')
    } finally {
      setDeleteLoading(false)
    }
  }

  function openEdit(action) {
    setEditingAction(action)
    setFormName(action.name ?? action.Name ?? '')
    setFormError(null)
  }

  function openNew() {
    setEditingAction({})
    setFormName('')
    setFormError(null)
  }

  return (
    <div>
      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search by action name…"
        actions={
          <Button onClick={openNew}>
            <Plus size={16} />
            New Action
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
                <SortableTh label="Name" sortKey="name" sort={sort} onSort={toggleSort} />
                <th className="px-4 py-3 font-medium">Last change</th>
                <th className="px-4 py-3 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {actions === null &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={4} className="px-4 py-3">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {actions !== null && filtered.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-4 py-10 text-center text-steel-500">
                    {search.trim() ? 'No actions match your search.' : 'No actions yet — create the first one.'}
                  </td>
                </tr>
              )}

              {paged?.map((a) => (
                <tr key={a.actionID ?? a.ActionID} className="hover:bg-panel-50">
                  <td className="px-4 py-3 font-mono text-steel-800">{a.actionID ?? a.ActionID}</td>
                  <td className="px-4 py-3 text-steel-800 font-medium">{a.name ?? a.Name}</td>
                                    <AuditStamp
                    createdAt={a.createdAt}
                    createdBy={a.createdBy}
                    modifiedAt={a.modifiedAt}
                    modifiedBy={a.modifiedBy}
                  />
<td className="px-4 py-3">
                    <div className="flex items-center justify-end gap-1">
                      <button
                        onClick={() => openEdit(a)}
                        className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
                        title="Edit"
                      >
                        <Pencil size={16} />
                      </button>
                      <button
                        onClick={() => setDeletingAction(a)}
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

      <Modal open={!!editingAction} onClose={() => setEditingAction(null)} title={editingAction ? 'Edit action' : 'New action'}>
        {editingAction && (
          <div className="space-y-4">
            <ErrorBanner message={formError} />
            <Field label="Name" required>
              <input className={inputClass} value={formName} onChange={(e) => setFormName(e.target.value)} />
            </Field>
            <div className="flex items-center justify-end gap-2 pt-2">
              <Button type="button" variant="secondary" onClick={() => setEditingAction(null)} disabled={saving}>
                Cancel
              </Button>
              <Button onClick={handleSave} disabled={saving}>
                {saving ? 'Saving…' : editingAction ? 'Save changes' : 'Create action'}
              </Button>
            </div>
          </div>
        )}
      </Modal>

      <ConfirmDialog
        open={!!deletingAction}
        title="Delete action"
        message={`Delete action "${deletingAction?.name ?? deletingAction?.Name}"? This can't be undone.`}
        onCancel={() => {
          setDeletingAction(null)
          setDeleteError(null)
        }}
        onConfirm={handleDelete}
        loading={deleteLoading}
      />
      {deleteError && <p className="mt-2 text-sm text-stop-500">{deleteError}</p>}
    </div>
  )
}
