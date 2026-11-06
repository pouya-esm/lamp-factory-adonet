import { useEffect, useMemo, useState } from 'react'
import { Plus, Pencil, Trash2 } from 'lucide-react'
import { usersApi } from '../../api/usersApi.js'
import { useRoles } from '../../lib/useRoles.js'
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
import UserForm from './UserForm.jsx'

function normalizeUser(dto) {
  if (!dto) return { userID: null, username: '', userpass: '', fullname: '', phone: '', roleIds: [], createdAt: null, createdBy: null, modifiedAt: null, modifiedBy: null }
  const u = dto.user ?? dto.User ?? {}
  return {
    userID: u.userID ?? u.UserID ?? dto.userID ?? dto.UserID,
    username: u.username ?? u.Username ?? '',
    fullname: u.fullname ?? u.Fullname ?? '',
    phone: u.phone ?? u.Phone ?? '',
    roleIds: dto.roleIds ?? dto.RoleIds ?? [],
    // The audit stamp lives on the nested user object the API returns, so it
    // has to be carried through here for the table's "Last change" column.
    createdAt: u.createdAt ?? u.CreatedAt ?? dto.createdAt ?? dto.CreatedAt,
    createdBy: u.createdBy ?? u.CreatedBy ?? dto.createdBy ?? dto.CreatedBy,
    modifiedAt: u.modifiedAt ?? u.ModifiedAt ?? dto.modifiedAt ?? dto.ModifiedAt,
    modifiedBy: u.modifiedBy ?? u.ModifiedBy ?? dto.modifiedBy ?? dto.ModifiedBy
  }
}

export default function UsersPage() {
  const [users, setUsers] = useState(null)
  const [loadError, setLoadError] = useState(null)
  const [search, setSearch] = useState('')
  const [filters, setFilters] = useState({ role: '' })
  const { roles } = useRoles()

  const [editingUser, setEditingUser] = useState(null)
  const [deletingUser, setDeletingUser] = useState(null)
  const [deleteLoading, setDeleteLoading] = useState(false)
  const [deleteError, setDeleteError] = useState(null)

  async function loadUsers() {
    setLoadError(null)
    try {
      const data = await usersApi.getAll()
      setUsers(Array.isArray(data) ? data : [])
    } catch (err) {
      setLoadError(err.message || 'Failed to load users.')
    }
  }

  useEffect(() => {
    loadUsers()
  }, [])

  function roleNames(dto) {
    const ids = normalizeUser(dto).roleIds
    return (ids ?? [])
      .map((id) => roles?.find((r) => r.roleID === id || r.roleID === Number(id))?.roleName || `Role ${id}`)
      .join(', ') || '—'
  }

  const filtered = useMemo(() => {
    if (!users) return []
    let list = users
    const q = search.trim().toLowerCase()
    if (q) {
      list = list.filter((dto) => {
        const u = normalizeUser(dto)
        return [u.username, u.fullname, u.phone].filter(Boolean).some((v) => v.toLowerCase().includes(q))
      })
    }
    if (filters.role !== '') {
      list = list.filter((dto) => {
        const ids = normalizeUser(dto).roleIds
        return ids?.some((id) => String(id) === filters.role)
      })
    }
    return list
  }, [users, search, filters])

  const isFiltered = search.trim() !== '' || filters.role !== ''
  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    username: (dto) => normalizeUser(dto).username,
    fullname: (dto) => normalizeUser(dto).fullname,
    phone: (dto) => normalizeUser(dto).phone,
    role: (dto) => roleNames(dto)
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    filters,
    sort,
    roles
  ])

  async function handleSave(payload) {
    const existingId = editingUser ? normalizeUser(editingUser).userID : null
    if (existingId) {
      await usersApi.update(existingId, payload)
    } else {
      await usersApi.create(payload)
    }
    setEditingUser(null)
    await loadUsers()
  }

  async function handleDelete() {
    setDeleteLoading(true)
    setDeleteError(null)
    try {
      const id = normalizeUser(deletingUser).userID
      await usersApi.remove(id)
      setDeletingUser(null)
      await loadUsers()
    } catch (err) {
      setDeleteError(err.message || 'Failed to delete user.')
    } finally {
      setDeleteLoading(false)
    }
  }

  return (
    <div>
      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search by username, name or phone…"
        filters={[
          {
            name: 'role',
            label: 'Role',
            type: 'select',
            value: filters.role,
            placeholder: 'All roles',
            options: (roles ?? []).map((r) => ({ value: String(r.roleID), label: r.roleName }))
          }
        ]}
        onFilterChange={(name, value) => setFilters((prev) => ({ ...prev, [name]: value }))}
        onReset={() => {
          setSearch('')
          setFilters({ role: '' })
        }}
        actions={
          <Button onClick={() => setEditingUser({})}>
            <Plus size={16} />
            New user
          </Button>
        }
      />

      <ErrorBanner message={loadError} />

      <div className="rounded-lg border border-panel-300 bg-white overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                <SortableTh label="Username" sortKey="username" sort={sort} onSort={toggleSort} />
                <SortableTh label="Full Name" sortKey="fullname" sort={sort} onSort={toggleSort} />
                <SortableTh label="Phone" sortKey="phone" sort={sort} onSort={toggleSort} />
                <SortableTh label="Role(s)" sortKey="role" sort={sort} onSort={toggleSort} />
                <th className="px-4 py-3 font-medium">Last change</th>
                <th className="px-4 py-3 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {users === null &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={6} className="px-4 py-3">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {users !== null && filtered.length === 0 && (
                <tr>
                  <td colSpan={6} className="px-4 py-10 text-center text-steel-500">
                    {isFiltered ? 'No users match your filters.' : 'No users yet — create the first one.'}
                  </td>
                </tr>
              )}

              {paged?.map((dto) => {
                const u = normalizeUser(dto)
                return (
                  <tr key={u.userID} className="hover:bg-panel-50">
                    <td className="px-4 py-3 font-mono text-steel-800">{u.username}</td>
                    <td className="px-4 py-3 text-steel-700">{u.fullname || '—'}</td>
                    <td className="px-4 py-3 text-steel-700">{u.phone || '—'}</td>
                    <td className="px-4 py-3 text-steel-700">{roleNames(dto)}</td>
                    <AuditStamp
                      createdAt={u.createdAt}
                      createdBy={u.createdBy}
                      modifiedAt={u.modifiedAt}
                      modifiedBy={u.modifiedBy}
                    />
                    <td className="px-4 py-3">
                      <div className="flex items-center justify-end gap-1">
                        <button
                          onClick={() => setEditingUser(dto)}
                          className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
                          aria-label={`Edit user ${u.username}`}
                          title="Edit"
                        >
                          <Pencil size={16} />
                        </button>
                        <button
                          onClick={() => setDeletingUser(dto)}
                          className="p-1.5 rounded-md text-steel-500 hover:bg-stop-500/10 hover:text-stop-500"
                          aria-label={`Delete user ${u.username}`}
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

      <Modal open={!!editingUser} onClose={() => setEditingUser(null)} title={editingUser ? 'Edit user' : 'New user'}>
        {editingUser && (
          <UserForm user={editingUser} onSubmit={handleSave} onCancel={() => setEditingUser(null)} />
        )}
      </Modal>

      <ConfirmDialog
        open={!!deletingUser}
        title="Delete user"
        message={deletingUser ? `Delete user "${normalizeUser(deletingUser).username}"? This can't be undone.` : ''}
        onCancel={() => {
          setDeletingUser(null)
          setDeleteError(null)
        }}
        onConfirm={handleDelete}
        loading={deleteLoading}
      />
      {deleteError && <p className="mt-2 text-sm text-stop-500">{deleteError}</p>}
    </div>
  )
}
