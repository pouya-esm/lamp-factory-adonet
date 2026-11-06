import { useEffect, useMemo, useState } from 'react'
import { Plus, Eye, Pencil, Trash2 } from 'lucide-react'
import { rolesApi } from '../../api/rolesApi.js'
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
import RoleForm from './RoleForm.jsx'

export default function RolesPage() {
  const [roles, setRoles] = useState(null)
  const [permissions, setPermissions] = useState(null)
  const [loadError, setLoadError] = useState(null)
  const [search, setSearch] = useState('')

  const [editingRole, setEditingRole] = useState(null)
  const [deletingRole, setDeletingRole] = useState(null)
  const [viewingRole, setViewingRole] = useState(null)
  const [deleteLoading, setDeleteLoading] = useState(false)
  const [deleteError, setDeleteError] = useState(null)

  async function loadRoles() {
    setLoadError(null)
    try {
      const data = await rolesApi.getAll()
      setRoles(Array.isArray(data) ? data : [])
    } catch (err) {
      setLoadError(err.message || 'Failed to load roles.')
    }
  }

  async function loadPermissions() {
    try {
      const data = await rbacApi.permissions.getAll()
      const permissionsList = Array.isArray(data) ? data : []
      const permissionsWithNames = permissionsList.map((p) => ({
        ...p,
        resourceName: p.resourceName ?? p.ResourceName ?? '',
        actionName: p.actionName ?? p.ActionName ?? '',
        permissionID: p.permissionID ?? p.PermissionID
      }))
      setPermissions(permissionsWithNames)
    } catch (err) {
      console.error('Failed to load permissions:', err)
    }
  }

  async function loadAll() {
    await Promise.all([loadRoles(), loadPermissions()])
  }

  useEffect(() => {
    loadAll()
  }, [])

  const filtered = useMemo(() => {
    if (!roles) return []
    const q = search.trim().toLowerCase()
    if (!q) return roles
    return roles.filter((role) =>
      [role.roleName, role.description].filter(Boolean).some((v) => v.toLowerCase().includes(q))
    )
  }, [roles, search])

  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    id: (role) => role.roleID ?? role.RoleID,
    roleName: (role) => role.roleName ?? role.RoleName,
    description: (role) => role.description ?? role.Description,
    permissions: (role) => (role.permissionIds ?? role.PermissionIds ?? []).length
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    sort
  ])

  async function handleSave(payload) {
    const roleId = editingRole?.roleID ?? editingRole?.RoleID
    if (roleId) {
      await rolesApi.update(roleId, payload)
    } else {
      await rolesApi.create(payload)
    }
    setEditingRole(null)
    await loadRoles()
  }

  async function handleDelete() {
    setDeleteLoading(true)
    setDeleteError(null)
    try {
      await rolesApi.remove(deletingRole?.roleID ?? deletingRole?.RoleID)
      setDeletingRole(null)
      await loadRoles()
    } catch (err) {
      setDeleteError(err.message || 'Failed to delete role.')
    } finally {
      setDeleteLoading(false)
    }
  }

  function getRolePermissionNames(role) {
    const permIds = role.permissionIds ?? role.PermissionIds ?? []
    if (!permissions) return []
    return permissions.filter((p) => permIds.includes(p.permissionID ?? p.PermissionID))
  }

  return (
    <div>
      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search by role name or description…"
        actions={
          <Button onClick={() => setEditingRole({})}>
            <Plus size={16} />
            New role
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
                <SortableTh label="Role Name" sortKey="roleName" sort={sort} onSort={toggleSort} />
                <SortableTh label="Description" sortKey="description" sort={sort} onSort={toggleSort} />
                <SortableTh label="Permissions" sortKey="permissions" sort={sort} onSort={toggleSort} />
                <th className="px-4 py-3 font-medium">Last change</th>
                <th className="px-4 py-3 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {roles === null &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={6} className="px-4 py-3">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {roles !== null && filtered.length === 0 && (
                <tr>
                  <td colSpan={6} className="px-4 py-10 text-center text-steel-500">
                    {search.trim() ? 'No roles match your filters.' : 'No roles yet — create the first one.'}
                  </td>
                </tr>
              )}

              {paged?.map((role) => {
                const roleId = role.roleID ?? role.RoleID
                const roleName = role.roleName ?? role.RoleName
                const roleDesc = role.description ?? role.Description
                const permIds = role.permissionIds ?? role.PermissionIds ?? []
                const permCount = permIds.length
                const totalPerms = permissions?.length ?? 0
                return (
                  <tr key={roleId} className="hover:bg-panel-50">
                    <td className="px-4 py-3 font-mono text-steel-800">{roleId}</td>
                    <td className="px-4 py-3 text-steel-800 font-medium">{roleName}</td>
                    <td className="px-4 py-3 text-steel-700">{roleDesc || '—'}</td>
                    <td className="px-4 py-3 text-steel-700">
                      {totalPerms > 0 ? `${permCount} / ${totalPerms}` : `${permCount}`}
                    </td>
                    <AuditStamp
                      createdAt={role.createdAt}
                      createdBy={role.createdBy}
                      modifiedAt={role.modifiedAt}
                      modifiedBy={role.modifiedBy}
                    />
                    <td className="px-4 py-3">
                      <div className="flex items-center justify-end gap-1">
                        <button
                          onClick={() => setViewingRole(role)}
                          className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
                          aria-label={`View permissions for ${roleName}`}
                          title="View Permissions"
                        >
                          <Eye size={16} />
                        </button>
                        <button
                          onClick={() => setEditingRole(role)}
                          className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
                          aria-label={`Edit role ${roleName}`}
                          title="Edit"
                        >
                          <Pencil size={16} />
                        </button>
                        <button
                          onClick={() => setDeletingRole(role)}
                          className="p-1.5 rounded-md text-steel-500 hover:bg-stop-500/10 hover:text-stop-500"
                          aria-label={`Delete role ${roleName}`}
                          title="Delete"
                        >
                          <Trash2 size={16} />
                        </button>
                      </div>
                    </td>
                  </tr>
                )
              })}
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

      <Modal open={!!editingRole} onClose={() => setEditingRole(null)} title={editingRole ? 'Edit role' : 'New role'}>
        {editingRole && (
          <RoleForm role={editingRole} permissions={permissions} onSubmit={handleSave} onCancel={() => setEditingRole(null)} />
        )}
      </Modal>

      <Modal open={!!viewingRole} onClose={() => setViewingRole(null)} title={`Permissions for ${viewingRole?.roleName ?? viewingRole?.RoleName}`}>
        {viewingRole && (
          <div className="max-h-96 overflow-y-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                  <th className="px-3 py-2">#</th>
                  <th className="px-3 py-2">Resource</th>
                  <th className="px-3 py-2">Action</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-panel-200">
                {getRolePermissionNames(viewingRole).length === 0 && (
                  <tr>
                    <td colSpan={4} className="px-3 py-4 text-center text-steel-500">No permissions assigned.</td>
                  </tr>
                )}
                {getRolePermissionNames(viewingRole).map((p) => (
                  <tr key={p.permissionID ?? p.PermissionID} className="hover:bg-panel-50">
                    <td className="px-3 py-2 text-steel-400">{p.permissionID ?? p.PermissionID}</td>
                    <td className="px-3 py-2 text-steel-800">{p.resourceName}</td>
                    <td className="px-3 py-2 text-steel-700">{p.actionName}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Modal>

      <ConfirmDialog
        open={!!deletingRole}
        title="Delete role"
        message={`Delete role "${deletingRole?.roleName ?? deletingRole?.RoleName}"? This can't be undone.`}
        onCancel={() => {
          setDeletingRole(null)
          setDeleteError(null)
        }}
        onConfirm={handleDelete}
        loading={deleteLoading}
      />
      {deleteError && <p className="mt-2 text-sm text-stop-500">{deleteError}</p>}
    </div>
  )
}
