// Mirrors Lamp_Factory.Enums.WarehouseType. Same camelCase/numeric-enum
// wire format as ProductType — see the note in pages/products/productTypes.js.
export const WAREHOUSE_TYPE_OPTIONS = [
  { value: 0, label: 'Entry Storage' },
  { value: 1, label: 'Final Warehouse' }
]

export const WAREHOUSE_TYPE_LABELS = Object.fromEntries(
  WAREHOUSE_TYPE_OPTIONS.map(({ value, label }) => [value, label])
)
