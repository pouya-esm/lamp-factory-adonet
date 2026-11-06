// Mirrors Lamp_Factory.Enums.QualityControlType. Same camelCase/numeric-enum
// wire format as ProductType/WarehouseType.
export const QC_TYPE_OPTIONS = [
  { value: 0, label: 'Final' },
  { value: 1, label: 'Period' }
]

export const QC_TYPE_LABELS = Object.fromEntries(QC_TYPE_OPTIONS.map(({ value, label }) => [value, label]))
