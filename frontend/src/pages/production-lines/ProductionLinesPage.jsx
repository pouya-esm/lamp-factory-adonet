import { useEffect, useMemo, useState } from 'react'
import AuditStamp, { AuditStampContent } from '../../components/AuditStamp.jsx'
import { Plus, Pencil, Trash2 } from 'lucide-react'
import { productionLinesApi } from '../../api/productionLinesApi.js'
import { lineStopsApi } from '../../api/lineStopsApi.js'
import { LINE_STATUS_OPTIONS, lineStatusMeta } from './lineStatus.js'
import Button from '../../components/ui/Button.jsx'
import Modal from '../../components/ui/Modal.jsx'
import ConfirmDialog from '../../components/ui/ConfirmDialog.jsx'
import ErrorBanner from '../../components/ui/ErrorBanner.jsx'
import FilterBar from '../../components/FilterBar.jsx'
import Pagination from '../../components/ui/Pagination.jsx'
import usePagedList from '../../hooks/usePagedList.js'
import ProductionLineForm from './ProductionLineForm.jsx'
import LineStopForm from '../line-stops/LineStopForm.jsx'
import { Play, Square } from 'lucide-react'

export default function ProductionLinesPage() {
  const [lines, setLines] = useState(null)
  const [loadError, setLoadError] = useState(null)
  const [search, setSearch] = useState('')
  const [filters, setFilters] = useState({ status: '' })

  const [editingLine, setEditingLine] = useState(null)
  const [deletingLine, setDeletingLine] = useState(null)
  const [stoppingLine, setStoppingLine] = useState(null)
  const [deleteLoading, setDeleteLoading] = useState(false)
  const [deleteError, setDeleteError] = useState(null)

  async function loadLines() {
    setLoadError(null)
    try {
      const data = await productionLinesApi.getAll()
      setLines(Array.isArray(data) ? data : [])
    } catch (err) {
      setLoadError(err.message || 'Failed to load production lines.')
    }
  }

  useEffect(() => {
    loadLines()
  }, [])

  const filtered = useMemo(() => {
    if (!lines) return []
    let list = lines
    const q = search.trim().toLowerCase()
    if (q) {
      list = list.filter((line) => String(line.productionLineID).includes(q))
    }
    if (filters.status !== '') {
      list = list.filter((line) => line.status === (filters.status === 'true'))
    }
    return list
  }, [lines, search, filters])

  const isFiltered = search.trim() !== '' || filters.status !== ''
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(filtered, [
    search,
    filters
  ])

  async function handleSave(payload) {
    if (editingLine?.productionLineID) {
      await productionLinesApi.update(editingLine.productionLineID, payload)
    } else {
      await productionLinesApi.create(payload)
    }
    setEditingLine(null)
    await loadLines()
  }

  async function handleStart(line) {
    try {
      await productionLinesApi.update(line.productionLineID, {
        productionLineID: line.productionLineID,
        status: 1
      })
      await loadLines()
    } catch (err) {
      setLoadError(err.message || 'Failed to start the production line.')
    }
  }

  async function handleStopSubmit(payload) {
    try {
      // Order matters: create the LineStop record first so the reason is never
      // lost if the status update that follows it fails. Re-throwing lets the
      // LineStopForm surface the error in the modal instead of closing it.
      await lineStopsApi.create(payload)
      await productionLinesApi.update(stoppingLine.productionLineID, {
        productionLineID: stoppingLine.productionLineID,
        status: 0
      })
      setStoppingLine(null)
      await loadLines()
    } catch (err) {
      throw err
    }
  }

  async function handleDelete() {
    setDeleteLoading(true)
    setDeleteError(null)
    try {
      await productionLinesApi.remove(deletingLine.productionLineID)
      setDeletingLine(null)
      await loadLines()
    } catch (err) {
      setDeleteError(err.message || 'Failed to delete production line.')
    } finally {
      setDeleteLoading(false)
    }
  }

  return (
    <div>
      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search by line number…"
        filters={[
          {
            name: 'status',
            label: 'Status',
            type: 'select',
            value: filters.status,
            placeholder: 'All statuses',
            options: LINE_STATUS_OPTIONS
          }
        ]}
        onFilterChange={(name, value) => setFilters((prev) => ({ ...prev, [name]: value }))}
        onReset={() => {
          setSearch('')
          setFilters({ status: '' })
        }}
        actions={
          <Button onClick={() => setEditingLine({})}>
            <Plus size={16} />
            New production line
          </Button>
        }
      />

      <ErrorBanner message={loadError} />

      {lines === null && (
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-3">
          {[1, 2, 3, 4].map((i) => (
            <div key={i} className="h-24 rounded-lg border border-panel-300 bg-white animate-pulse" />
          ))}
        </div>
      )}

      {lines !== null && filtered.length === 0 && (
        <div className="rounded-lg border border-dashed border-panel-300 bg-white py-16 text-center text-steel-500">
          {isFiltered ? 'No production lines match your filters.' : 'No production lines yet — create the first one.'}
        </div>
      )}

      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-3">
        {paged?.map((line) => {
          const meta = lineStatusMeta(line.status)
          return (
            <div key={line.productionLineID} className="rounded-lg border border-panel-300 bg-white p-4">
              <div className="flex items-start justify-between mb-2">
                <p className="font-mono text-steel-800 font-medium">Line {line.productionLineID}</p>
                <div className="flex items-center gap-0.5">
                  <button
                    onClick={() => setEditingLine(line)}
                    className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
                    aria-label={`Edit line ${line.productionLineID}`}
                    title="Edit"
                  >
                    <Pencil size={14} />
                  </button>
                  <button
                    onClick={() => setDeletingLine(line)}
                    className="p-1.5 rounded-md text-steel-500 hover:bg-stop-500/10 hover:text-stop-500"
                    aria-label={`Delete line ${line.productionLineID}`}
                    title="Delete"
                  >
                    <Trash2 size={14} />
                  </button>
                </div>
              </div>
              <span className={`inline-flex items-center gap-1.5 rounded px-2 py-0.5 text-xs font-medium ${meta.badge}`}>
                <span className={`h-1.5 w-1.5 rounded-full ${meta.dot}`} />
                {meta.label}
              </span>
              <div className="mt-2">
                <AuditStampContent
                  createdAt={line.createdAt}
                  createdBy={line.createdBy}
                  modifiedAt={line.modifiedAt}
                  modifiedBy={line.modifiedBy}
                />
              </div>
              <div className="flex items-center gap-2 mt-3">
                {line.status === true ? (
                  <Button
                    type="button"
                    variant="secondary"
                    onClick={() => setStoppingLine(line)}
                    className="text-stop-600 hover:text-stop-700"
                  >
                    <Square size={14} />
                    Stop line
                  </Button>
                ) : (
                  <Button
                    type="button"
                    variant="secondary"
                    onClick={() => handleStart(line)}
                    className="text-line-600 hover:text-line-700"
                  >
                    <Play size={14} />
                    Start line
                  </Button>
                )}
              </div>
            </div>
          )
        })}
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
        open={!!editingLine}
        onClose={() => setEditingLine(null)}
        title={editingLine?.productionLineID ? `Edit line ${editingLine.productionLineID}` : 'New production line'}
      >
        {editingLine && (
          <ProductionLineForm
            line={editingLine.productionLineID ? editingLine : null}
            onSubmit={handleSave}
            onCancel={() => setEditingLine(null)}
          />
        )}
      </Modal>

      <ConfirmDialog
        open={!!deletingLine}
        title="Delete production line"
        message={`Delete "Line ${deletingLine?.productionLineID}"? This can't be undone.`}
        onCancel={() => {
          setDeletingLine(null)
          setDeleteError(null)
        }}
        onConfirm={handleDelete}
        loading={deleteLoading}
      />
      {deleteError && <p className="mt-2 text-sm text-stop-500">{deleteError}</p>}

      <Modal
        open={!!stoppingLine}
        onClose={() => setStoppingLine(null)}
        title={stoppingLine ? `Stop Line ${stoppingLine.productionLineID}` : 'Stop line'}
      >
        {stoppingLine && (
          <LineStopForm
            lineStop={null}
            onSubmit={handleStopSubmit}
            onCancel={() => setStoppingLine(null)}
          />
        )}
      </Modal>
    </div>
  )
}
