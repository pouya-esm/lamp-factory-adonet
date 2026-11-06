import { useEffect, useState, Fragment, useMemo } from 'react'
import { Plus, Pencil, Trash2, ChevronRight, ChevronDown, Layers } from 'lucide-react'
import { bomsApi } from '../../api/bomsApi.js'
import { useProducts } from '../../lib/useProducts.js'
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
import BOMForm from './BOMForm.jsx'

// See the casing note in BOMForm.jsx — the backend's "BOM" property
// serializes as "bom" on the wire, so every entry here is { bom, components }.
export default function BOMsPage() {
  const [entries, setEntries] = useState(null)
  const [loadError, setLoadError] = useState(null)
  const { products } = useProducts()
  const [expandedId, setExpandedId] = useState(null)
  const [search, setSearch] = useState('')

  const [editingEntry, setEditingEntry] = useState(null)
  const [deletingEntry, setDeletingEntry] = useState(null)
  const [deleteLoading, setDeleteLoading] = useState(false)
  const [deleteError, setDeleteError] = useState(null)

  async function loadEntries() {
    setLoadError(null)
    try {
      const data = await bomsApi.getAll()
      setEntries(Array.isArray(data) ? data : [])
    } catch (err) {
      setLoadError(err.message || 'Failed to load BOMs.')
    }
  }

  useEffect(() => {
    loadEntries()
  }, [])

  function findProduct(id) {
    return products?.find((p) => p.productID === id)
  }

  function productLabel(id) {
    const p = findProduct(id)
    if (!p) return `Product ${id}`
    return p.standardNumber ? `${p.standardNumber} — ${p.material || ''}`.trim() : p.material || `Product ${id}`
  }

  const filtered = useMemo(() => {
    if (!entries) return []
    const q = search.trim().toLowerCase()
    if (!q) return entries
    return entries.filter((entry) =>
      [String(entry.bom.boM_ID), productLabel(entry.bom.productID)].some((v) => v.toLowerCase().includes(q))
    )
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [entries, search, products])

  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    id: (entry) => entry.bom.boM_ID,
    product: (entry) => productLabel(entry.bom.productID),
    components: (entry) => entry.components?.length ?? 0
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    sort
  ])

  function toggleExpand(id) {
    setExpandedId((prev) => (prev === id ? null : id))
  }

  async function handleSave(payload) {
    if (editingEntry?.bom?.boM_ID) {
      await bomsApi.update(editingEntry.bom.boM_ID, payload)
    } else {
      await bomsApi.create(payload)
    }
    setEditingEntry(null)
    await loadEntries()
  }

  async function handleDelete() {
    setDeleteLoading(true)
    setDeleteError(null)
    try {
      await bomsApi.remove(deletingEntry.bom.boM_ID)
      setDeletingEntry(null)
      await loadEntries()
    } catch (err) {
      setDeleteError(err.message || 'Failed to delete BOM.')
    } finally {
      setDeleteLoading(false)
    }
  }

  return (
    <div>
      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search by # or product…"
        actions={
          <Button onClick={() => setEditingEntry({})}>
            <Plus size={16} />
            New BOM
          </Button>
        }
      />

      <ErrorBanner message={loadError} />

      <div className="rounded-lg border border-panel-300 bg-white overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                <th className="px-4 py-3 font-medium w-10" />
                <SortableTh label="#" sortKey="id" sort={sort} onSort={toggleSort} />
                <SortableTh label="Builds Product" sortKey="product" sort={sort} onSort={toggleSort} />
                <SortableTh label="Components" sortKey="components" sort={sort} onSort={toggleSort} />
                <th className="px-4 py-3 font-medium">Last change</th>
                <th className="px-4 py-3 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {entries === null &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={6} className="px-4 py-3">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {entries !== null && filtered.length === 0 && (
                <tr>
                  <td colSpan={6} className="px-4 py-10 text-center text-steel-500">
                    {search.trim() ? 'No BOMs match your filters.' : 'No BOMs yet — create the first one.'}
                  </td>
                </tr>
              )}

              {paged?.map((entry) => {
                const id = entry.bom.boM_ID
                const isExpanded = expandedId === id
                const hasComponents = (entry.components?.length ?? 0) > 0

                return (
                  <Fragment key={id}>
                    <tr
                      onClick={() => hasComponents && toggleExpand(id)}
                      className={`hover:bg-panel-50 ${hasComponents ? 'cursor-pointer' : ''}`}
                    >
                      <td className="px-4 py-3">
                        {hasComponents && (
                          <span className="text-steel-400">
                            {isExpanded ? <ChevronDown size={16} /> : <ChevronRight size={16} />}
                          </span>
                        )}
                      </td>
                      <td className="px-4 py-3 font-mono text-steel-800">{id}</td>
                      <td className="px-4 py-3 text-steel-700">{productLabel(entry.bom.productID)}</td>
                      <td className="px-4 py-3 text-steel-700">
                        {entry.components?.length ?? 0} component(s)
                      </td>
                                            <AuditStamp
                        createdAt={entry.bom.createdAt}
                        createdBy={entry.bom.createdBy}
                        modifiedAt={entry.bom.modifiedAt}
                        modifiedBy={entry.bom.modifiedBy}
                      />
<td className="px-4 py-3">
                        <div className="flex items-center justify-end gap-1" onClick={(e) => e.stopPropagation()}>
                          <button
                            onClick={() => setEditingEntry(entry)}
                            className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
                            aria-label={`Edit BOM ${id}`}
                            title="Edit"
                          >
                            <Pencil size={16} />
                          </button>
                          <button
                            onClick={() => setDeletingEntry(entry)}
                            className="p-1.5 rounded-md text-steel-500 hover:bg-stop-500/10 hover:text-stop-500"
                            aria-label={`Delete BOM ${id}`}
                            title="Delete"
                          >
                            <Trash2 size={16} />
                          </button>
                        </div>
                      </td>
                    </tr>

                    {isExpanded && (
                      <tr className="bg-panel-50">
                        <td colSpan={6} className="px-4 pb-4 pt-0">
                          <div className="ml-8 rounded-md border border-panel-300 bg-white overflow-hidden">
                            <div className="flex items-center gap-1.5 px-3 py-2 border-b border-panel-200 text-xs uppercase tracking-wider text-steel-500 font-medium">
                              <Layers size={13} />
                              Components consumed per unit
                            </div>
                            <table className="w-full text-sm">
                              <thead>
                                <tr className="text-left text-xs text-steel-500">
                                  <th className="px-3 py-2 font-medium">Standard Number</th>
                                  <th className="px-3 py-2 font-medium">Material</th>
                                  <th className="px-3 py-2 font-medium text-right">Consumption / unit</th>
                                </tr>
                              </thead>
                              <tbody className="divide-y divide-panel-200">
                                {entry.components.map((c, i) => {
                                  const p = findProduct(c.productID)
                                  return (
                                    <tr key={i}>
                                      <td className="px-3 py-2 font-mono text-steel-800">
                                        {p?.standardNumber || `Product ${c.productID}`}
                                      </td>
                                      <td className="px-3 py-2 text-steel-700">{p?.material || '—'}</td>
                                      <td className="px-3 py-2 text-steel-700 text-right font-mono">
                                        {c.consumptionAmount ?? '—'}
                                        {p?.consumptionUnit ? ` ${p.consumptionUnit}` : ''}
                                      </td>
                                    </tr>
                                  )
                                })}
                              </tbody>
                            </table>
                          </div>
                        </td>
                      </tr>
                    )}
                  </Fragment>
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

      <Modal
        open={!!editingEntry}
        onClose={() => setEditingEntry(null)}
        title={editingEntry?.bom?.boM_ID ? 'Edit BOM' : 'New BOM'}
        wide
      >
        {editingEntry && (
          <BOMForm
            entry={editingEntry.bom ? editingEntry : null}
            onSubmit={handleSave}
            onCancel={() => setEditingEntry(null)}
          />
        )}
      </Modal>

      <ConfirmDialog
        open={!!deletingEntry}
        title="Delete BOM"
        message={`Delete BOM #${deletingEntry?.bom?.boM_ID}? This can't be undone.`}
        onCancel={() => {
          setDeletingEntry(null)
          setDeleteError(null)
        }}
        onConfirm={handleDelete}
        loading={deleteLoading}
      />
      {deleteError && <p className="mt-2 text-sm text-stop-500">{deleteError}</p>}
    </div>
  )
}
