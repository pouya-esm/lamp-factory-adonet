import { useEffect, useMemo, useState } from 'react'
import { Plus, Pencil, Trash2 } from 'lucide-react'
import { operatorsApi } from '../../api/operatorsApi.js'
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
import OperatorForm from './OperatorForm.jsx'

function fullName(op) {
  return [op.firstName, op.lastName].filter(Boolean).join(' ') || '—'
}

function formatDate(isoString) {
  if (!isoString) return '—'
  return isoString.slice(0, 10)
}

export default function OperatorsPage() {
  const [operators, setOperators] = useState(null)
  const [loadError, setLoadError] = useState(null)
  const [search, setSearch] = useState('')
  const [filters, setFilters] = useState({ hiredFrom: '', hiredTo: '' })

  const [editingOperator, setEditingOperator] = useState(null)
  const [deletingOperator, setDeletingOperator] = useState(null)
  const [deleteLoading, setDeleteLoading] = useState(false)
  const [deleteError, setDeleteError] = useState(null)

  async function loadOperators() {
    setLoadError(null)
    try {
      const data = await operatorsApi.getAll()
      setOperators(Array.isArray(data) ? data : [])
    } catch (err) {
      setLoadError(err.message || 'Failed to load operators.')
    }
  }

  useEffect(() => {
    loadOperators()
  }, [])

  const filtered = useMemo(() => {
    if (!operators) return []
    let list = operators
    const q = search.trim().toLowerCase()
    if (q) {
      list = list.filter((op) =>
        [op.firstName, op.lastName, op.nationalID].filter(Boolean).some((v) => v.toLowerCase().includes(q))
      )
    }
    const hired = (op) => (op.hiringDate || '').slice(0, 10)
    if (filters.hiredFrom) list = list.filter((op) => hired(op) >= filters.hiredFrom)
    if (filters.hiredTo) list = list.filter((op) => hired(op) <= filters.hiredTo)
    return list
  }, [operators, search, filters])

  const isFiltered =
    search.trim() !== '' || filters.hiredFrom !== '' || filters.hiredTo !== ''
  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    name: (op) => [op.firstName, op.lastName].filter(Boolean).join(' '),
    nationalID: (op) => op.nationalID,
    hiringDate: (op) => op.hiringDate
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    filters,
    sort
  ])

  async function handleSave(payload) {
    if (editingOperator?.operatorID) {
      await operatorsApi.update(editingOperator.operatorID, payload)
    } else {
      await operatorsApi.create(payload)
    }
    setEditingOperator(null)
    await loadOperators()
  }

  async function handleDelete() {
    setDeleteLoading(true)
    setDeleteError(null)
    try {
      await operatorsApi.remove(deletingOperator.operatorID)
      setDeletingOperator(null)
      await loadOperators()
    } catch (err) {
      setDeleteError(err.message || 'Failed to delete operator.')
    } finally {
      setDeleteLoading(false)
    }
  }

  return (
    <div>
      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search by name or national ID…"
        filters={[
          { name: 'hiredFrom', label: 'Hired from', type: 'date', value: filters.hiredFrom },
          { name: 'hiredTo', label: 'Hired to', type: 'date', value: filters.hiredTo }
        ]}
        onFilterChange={(name, value) => setFilters((prev) => ({ ...prev, [name]: value }))}
        onReset={() => {
          setSearch('')
          setFilters({ hiredFrom: '', hiredTo: '' })
        }}
        actions={
          <Button onClick={() => setEditingOperator({})}>
            <Plus size={16} />
            New operator
          </Button>
        }
      />

      <ErrorBanner message={loadError} />

      <div className="rounded-lg border border-panel-300 bg-white overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                <SortableTh label="Name" sortKey="name" sort={sort} onSort={toggleSort} />
                <SortableTh label="National ID" sortKey="nationalID" sort={sort} onSort={toggleSort} />
                <SortableTh label="Hiring Date" sortKey="hiringDate" sort={sort} onSort={toggleSort} />
                <th className="px-4 py-3 font-medium">Last change</th>
                <th className="px-4 py-3 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {operators === null &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={5} className="px-4 py-3">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {operators !== null && filtered.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-10 text-center text-steel-500">
                    {isFiltered ? 'No operators match your filters.' : 'No operators yet — add the first one.'}
                  </td>
                </tr>
              )}

              {paged.map((op) => (
                <tr key={op.operatorID} className="hover:bg-panel-50">
                  <td className="px-4 py-3 text-steel-800 font-medium">{fullName(op)}</td>
                  <td className="px-4 py-3 font-mono text-steel-700">{op.nationalID}</td>
                  <td className="px-4 py-3 text-steel-700">{formatDate(op.hiringDate)}</td>
                                    <AuditStamp
                    createdAt={op.createdAt}
                    createdBy={op.createdBy}
                    modifiedAt={op.modifiedAt}
                    modifiedBy={op.modifiedBy}
                  />
<td className="px-4 py-3">
                    <div className="flex items-center justify-end gap-1">
                      <button
                        onClick={() => setEditingOperator(op)}
                        className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
                        aria-label={`Edit ${fullName(op)}`}
                        title="Edit"
                      >
                        <Pencil size={16} />
                      </button>
                      <button
                        onClick={() => setDeletingOperator(op)}
                        className="p-1.5 rounded-md text-steel-500 hover:bg-stop-500/10 hover:text-stop-500"
                        aria-label={`Delete ${fullName(op)}`}
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

      <Modal
        open={!!editingOperator}
        onClose={() => setEditingOperator(null)}
        title={editingOperator?.operatorID ? 'Edit operator' : 'New operator'}
      >
        {editingOperator && (
          <OperatorForm
            operator={editingOperator.operatorID ? editingOperator : null}
            onSubmit={handleSave}
            onCancel={() => setEditingOperator(null)}
          />
        )}
      </Modal>

      <ConfirmDialog
        open={!!deletingOperator}
        title="Delete operator"
        message={`Delete "${deletingOperator ? fullName(deletingOperator) : 'this operator'}"? This can't be undone.`}
        onCancel={() => {
          setDeletingOperator(null)
          setDeleteError(null)
        }}
        onConfirm={handleDelete}
        loading={deleteLoading}
      />
      {deleteError && <p className="mt-2 text-sm text-stop-500">{deleteError}</p>}
    </div>
  )
}
