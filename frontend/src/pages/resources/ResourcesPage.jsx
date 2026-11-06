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

export default function ResourcesPage() {
  const [resources, setResources] = useState(null)
  const [loadError, setLoadError] = useState(null)
  const [search, setSearch] = useState('')

  const [editingResource, setEditingResource] = useState(null)
  const [deletingResource, setDeletingResource] = useState(null)
  const [deleteLoading, setDeleteLoading] = useState(false)
  const [deleteError, setDeleteError] = useState(null)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState(null)
  const [formName, setFormName] = useState('')

  async function loadResources() {
    setLoadError(null)
    try {
      const data = await rbacApi.resources.getAll()
      setResources(Array.isArray(data) ? data : [])
    } catch (err) {
      setLoadError(err.message || 'Failed to load resources.')
    }
  }

  useEffect(() => {
    loadResources()
  }, [])

  const filtered = useMemo(() => {
    if (!resources) return []
    const q = search.trim().toLowerCase()
    if (!q) return resources
    return resources.filter((r) =>
      [r.name, r.Name].filter(Boolean).some((v) => v.toLowerCase().includes(q))
    )
  }, [resources, search])

  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    id: (r) => r.resourceID ?? r.ResourceID,
    name: (r) => r.name ?? r.Name
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    sort
  ])

  async function handleSave() {
    setFormError(null)
    if (!formName.trim()) {
      setFormError('Resource name is required.')
      return
    }
    setSaving(true)
    try {
      if (editingResource) {
        await rbacApi.resources.update(editingResource.resourceID ?? editingResource.ResourceID, { name: formName })
      } else {
        await rbacApi.resources.create({ name: formName })
      }
      setEditingResource(null)
      setFormName('')
      await loadResources()
    } catch (err) {
      setFormError(err.message || 'Failed to save resource.')
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete() {
    setDeleteLoading(true)
    setDeleteError(null)
    try {
      await rbacApi.resources.remove(deletingResource.resourceID ?? deletingResource.ResourceID)
      setDeletingResource(null)
      await loadResources()
    } catch (err) {
      setDeleteError(err.message || 'Failed to delete resource.')
    } finally {
      setDeleteLoading(false)
    }
  }

  function openEdit(resource) {
    setEditingResource(resource)
    setFormName(resource.name ?? resource.Name ?? '')
    setFormError(null)
  }

  function openNew() {
    setEditingResource({})
    setFormName('')
    setFormError(null)
  }

  return (
    <div>
      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search by resource name…"
        actions={
          <Button onClick={openNew}>
            <Plus size={16} />
            New Resource
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
              {resources === null &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={4} className="px-4 py-3">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {resources !== null && filtered.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-4 py-10 text-center text-steel-500">
                    {search.trim() ? 'No resources match your search.' : 'No resources yet — create the first one.'}
                  </td>
                </tr>
              )}

              {paged?.map((r) => (
                <tr key={r.resourceID ?? r.ResourceID} className="hover:bg-panel-50">
                  <td className="px-4 py-3 font-mono text-steel-800">{r.resourceID ?? r.ResourceID}</td>
                  <td className="px-4 py-3 text-steel-800 font-medium">{r.name ?? r.Name}</td>
                                    <AuditStamp
                    createdAt={r.createdAt}
                    createdBy={r.createdBy}
                    modifiedAt={r.modifiedAt}
                    modifiedBy={r.modifiedBy}
                  />
<td className="px-4 py-3">
                    <div className="flex items-center justify-end gap-1">
                      <button
                        onClick={() => openEdit(r)}
                        className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
                        title="Edit"
                      >
                        <Pencil size={16} />
                      </button>
                      <button
                        onClick={() => setDeletingResource(r)}
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

      <Modal open={!!editingResource} onClose={() => setEditingResource(null)} title={editingResource ? 'Edit resource' : 'New resource'}>
        {editingResource && (
          <div className="space-y-4">
            <ErrorBanner message={formError} />
            <div>
              <label className="block text-sm font-medium text-steel-700">Name</label>
              <input className={inputClass} value={formName} onChange={(e) => setFormName(e.target.value)} />
            </div>
            <div className="flex items-center justify-end gap-2 pt-2">
              <Button type="button" variant="secondary" onClick={() => setEditingResource(null)} disabled={saving}>
                Cancel
              </Button>
              <Button onClick={handleSave} disabled={saving}>
                {saving ? 'Saving…' : editingResource ? 'Save changes' : 'Create resource'}
              </Button>
            </div>
          </div>
        )}
      </Modal>

      <ConfirmDialog
        open={!!deletingResource}
        title="Delete resource"
        message={`Delete resource "${deletingResource?.name ?? deletingResource?.Name}"? This can't be undone.`}
        onCancel={() => {
          setDeletingResource(null)
          setDeleteError(null)
        }}
        onConfirm={handleDelete}
        loading={deleteLoading}
      />
      {deleteError && <p className="mt-2 text-sm text-stop-500">{deleteError}</p>}
    </div>
  )
}
