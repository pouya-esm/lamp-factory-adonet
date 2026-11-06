// Mirrors Lamp_Factory.Enums.ProductType. System.Text.Json serializes C#
// enums as their underlying int by default (no JsonStringEnumConverter is
// registered in Program.cs), so the API sends/expects 0 or 1, not a string.
export const PRODUCT_TYPE_OPTIONS = [
  { value: 0, label: 'Final' },
  { value: 1, label: 'Component' }
]

export const PRODUCT_TYPE_LABELS = Object.fromEntries(
  PRODUCT_TYPE_OPTIONS.map(({ value, label }) => [value, label])
)

// Lampholder types. Unlike ProductType these are stored as a plain string
// column, so the wire values are the labels themselves.
export const LAMPHOLDER_TYPE_OPTIONS = [
  { value: 'Small', label: 'Small' },
  { value: 'Medium', label: 'Medium' },
  { value: 'Big', label: 'Big' }
]

export const LAMPHOLDER_TYPE_LABELS = Object.fromEntries(
  LAMPHOLDER_TYPE_OPTIONS.map(({ value, label }) => [value, label])
)
