import { useEffect, useMemo, useState } from 'react'
import { Plus, Pencil, Trash2 } from 'lucide-react'
import { productsApi } from '../../api/productsApi.js'
import {
  PRODUCT_TYPE_LABELS,
  PRODUCT_TYPE_OPTIONS,
  LAMPHOLDER_TYPE_OPTIONS,
  LAMPHOLDER_TYPE_LABELS
} from './productTypes.js'
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
import ProductForm from './ProductForm.jsx'

// NOTE: the backend serializes JSON with camelCase property names
// (productID, standardNumber, ...) even though the C# models are
// PascalCase — this is System.Text.Json's default. Every field read
// here must match the camelCase wire format, not the C# class.
function TypeBadge({ type }) {
  const label = PRODUCT_TYPE_LABELS[type] ?? 'Unknown'
  const isFinal = label === 'Final'
  return (
    <span
      className={`inline-flex items-center rounded px-2 py-0.5 text-xs font-medium ${
        isFinal ? 'bg-line-500/10 text-line-600' : 'bg-steel-500/10 text-steel-600'
      }`}
    >
      {label}
    </span>
  )
}

export default function ProductsPage() {
  const [products, setProducts] = useState(null)
  const [loadError, setLoadError] = useState(null)
  const [search, setSearch] = useState('')
  const [filters, setFilters] = useState({ type: '', lampholder: '' })

  const [editingProduct, setEditingProduct] = useState(null) // null = closed, {} = new, {...} = edit
  const [deletingProduct, setDeletingProduct] = useState(null)
  const [deleteLoading, setDeleteLoading] = useState(false)
  const [deleteError, setDeleteError] = useState(null)

  async function loadProducts() {
    setLoadError(null)
    try {
      const data = await productsApi.getAll()
      setProducts(Array.isArray(data) ? data : [])
    } catch (err) {
      setLoadError(err.message || 'Failed to load products.')
    }
  }

  useEffect(() => {
    loadProducts()
  }, [])

  const filtered = useMemo(() => {
    if (!products) return []
    let list = products
    const q = search.trim().toLowerCase()
    if (q) {
      list = list.filter((p) =>
        [p.standardNumber, p.material, p.productColor, p.lampholderType]
          .filter(Boolean)
          .some((v) => v.toLowerCase().includes(q))
      )
    }
    if (filters.type !== '') {
      list = list.filter((p) => String(p.productType) === filters.type)
    }
    if (filters.lampholder !== '') {
      list = list.filter((p) => String(p.lampholderType ?? '') === filters.lampholder)
    }
    return list
  }, [products, search, filters])

  const isFiltered = search.trim() !== '' || filters.type !== '' || filters.lampholder !== ''
  const { sort, toggleSort, applySort } = useSortable()
  const sortAccessors = {
    standardNumber: (p) => p.standardNumber,
    type: (p) => p.productType,
    material: (p) => p.material,
    color: (p) => p.productColor,
    weight: (p) => p.weight,
    voltage: (p) => p.voltage,
    power: (p) => p.power,
    unit: (p) => p.consumptionUnit,
    lampholder: (p) => p.lampholderType ?? ''
  }
  const sorted = applySort(filtered, sortAccessors)
  const { paged, page, setPage, pageSize, setPageSize, totalPages, totalItems } = usePagedList(sorted, [
    search,
    filters,
    sort
  ])

  async function handleSave(payload) {
    if (editingProduct?.productID) {
      await productsApi.update(editingProduct.productID, payload)
    } else {
      await productsApi.create(payload)
    }
    setEditingProduct(null)
    await loadProducts()
  }

  async function handleDelete() {
    setDeleteLoading(true)
    setDeleteError(null)
    try {
      await productsApi.remove(deletingProduct.productID)
      setDeletingProduct(null)
      await loadProducts()
    } catch (err) {
      setDeleteError(err.message || 'Failed to delete product.')
    } finally {
      setDeleteLoading(false)
    }
  }

  return (
    <div>
      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search by standard number, material, color…"
        filters={[
          {
            name: 'type',
            label: 'Type',
            type: 'select',
            value: filters.type,
            placeholder: 'All types',
            options: PRODUCT_TYPE_OPTIONS
          },
          {
            name: 'lampholder',
            label: 'Lampholder Type',
            type: 'select',
            value: filters.lampholder,
            placeholder: 'All lampholders',
            options: LAMPHOLDER_TYPE_OPTIONS
          }
        ]}
        onFilterChange={(name, value) => setFilters((prev) => ({ ...prev, [name]: value }))}
        onReset={() => {
          setSearch('')
          setFilters({ type: '', lampholder: '' })
        }}
         actions={
           <Button onClick={() => setEditingProduct({})}>
             <Plus size={16} />
             New product
           </Button>
         }
      />

      <ErrorBanner message={loadError} />

      <div className="rounded-lg border border-panel-300 bg-white overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                <SortableTh label="Standard Number" sortKey="standardNumber" sort={sort} onSort={toggleSort} />
                <SortableTh label="Type" sortKey="type" sort={sort} onSort={toggleSort} />
                <SortableTh label="Material" sortKey="material" sort={sort} onSort={toggleSort} />
                <SortableTh label="Color" sortKey="color" sort={sort} onSort={toggleSort} />
                <SortableTh label="Weight" sortKey="weight" sort={sort} onSort={toggleSort} />
                <SortableTh label="Voltage / Power" sortKey="voltage" sort={sort} onSort={toggleSort} />
                <SortableTh label="Unit" sortKey="unit" sort={sort} onSort={toggleSort} />
                <SortableTh label="Lampholder Type" sortKey="lampholder" sort={sort} onSort={toggleSort} />
                <th className="px-4 py-3 font-medium">Last change</th>
                <th className="px-4 py-3 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {products === null &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={10} className="px-4 py-3">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {products !== null && filtered.length === 0 && (
                <tr>
                    <td colSpan={10} className="px-4 py-10 text-center text-steel-500">
                      {isFiltered ? 'No products match your filters.' : 'No products yet — create the first one.'}
                  </td>
                </tr>
              )}

              {paged.map((p) => (
                <tr key={p.productID} className="hover:bg-panel-50">
                  <td className="px-4 py-3 font-mono text-steel-800">{p.standardNumber || '—'}</td>
                  <td className="px-4 py-3">
                    <TypeBadge type={p.productType} />
                  </td>
                  <td className="px-4 py-3 text-steel-700">{p.material || '—'}</td>
                  <td className="px-4 py-3 text-steel-700">{p.productColor || '—'}</td>
                  <td className="px-4 py-3 text-steel-700">{p.weight ?? '—'}</td>
                  <td className="px-4 py-3 text-steel-700">
                    {p.voltage ?? '—'} V / {p.power ?? '—'} W
                  </td>
                  <td className="px-4 py-3 text-steel-700">{p.consumptionUnit || '—'}</td>
                  <td className="px-4 py-3 text-steel-700">
                    {p.lampholderType ? (
                      <span className="inline-flex items-center rounded bg-panel-100 px-2 py-0.5 text-xs font-medium text-steel-700">
                        {LAMPHOLDER_TYPE_LABELS[p.lampholderType] ?? p.lampholderType}
                      </span>
                    ) : (
                      '—'
                    )}
                  </td>
                  <AuditStamp
                    createdAt={p.createdAt}
                    createdBy={p.createdBy}
                    modifiedAt={p.modifiedAt}
                    modifiedBy={p.modifiedBy}
                  />
                  <td className="px-4 py-3">
                    <div className="flex items-center justify-end gap-1">
                      <button
                        onClick={() => setEditingProduct(p)}
                        className="p-1.5 rounded-md text-steel-500 hover:bg-panel-100 hover:text-steel-800"
                        aria-label={`Edit ${p.standardNumber}`}
                        title="Edit"
                      >
                        <Pencil size={16} />
                      </button>
                      <button
                        onClick={() => setDeletingProduct(p)}
                        className="p-1.5 rounded-md text-steel-500 hover:bg-stop-500/10 hover:text-stop-500"
                        aria-label={`Delete ${p.standardNumber}`}
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
        open={!!editingProduct}
        onClose={() => setEditingProduct(null)}
        title={editingProduct?.productID ? 'Edit product' : 'New product'}
        wide
      >
        {editingProduct && (
          <ProductForm
            product={editingProduct.productID ? editingProduct : null}
            onSubmit={handleSave}
            onCancel={() => setEditingProduct(null)}
          />
        )}
      </Modal>

      <ConfirmDialog
        open={!!deletingProduct}
        title="Delete product"
        message={`Delete "${deletingProduct?.standardNumber || 'this product'}"? This can't be undone.`}
        onCancel={() => {
          setDeletingProduct(null)
          setDeleteError(null)
        }}
        onConfirm={handleDelete}
        loading={deleteLoading}
      />
      {deleteError && <p className="mt-2 text-sm text-stop-500">{deleteError}</p>}
    </div>
  )
}
