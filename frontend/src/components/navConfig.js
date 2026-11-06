import {
  LayoutDashboard,
  Package,
  Warehouse,
  Users,
  ShieldCheck,
  ListTree,
  ClipboardList,
  Factory,
  UserRound,
  Wrench,
  Ban,
  ClipboardCheck,
  ArrowDownToLine,
  ArrowUpFromLine,
  FileInput,
  FlaskConical,
  GitBranch,
  BarChart3,
  Route,
  KeyRound,
  ScrollText
} from 'lucide-react'

// Grouped to mirror the SKD production flow: set up master data, plan &
// define BOMs, run production, move materials via vouchers, then report.
//
// `resource` ties a nav item to the row with that Name in the backend
// [Resource] table. The sidebar filters items against the list returned by
// GET /resource/my-accessible, so an item without `resource` (Dashboard) is
// always shown and every other one appears only when the user holds at least
// one permission on it through their roles.
export const navSections = [
  {
    label: 'Overview',
    items: [{ to: '/', label: 'Dashboard', icon: LayoutDashboard, end: true }]
  },
  {
    label: 'Master Data',
    items: [
      { to: '/products', label: 'Products', icon: Package, resource: 'Product' },
      { to: '/warehouses', label: 'Warehouses', icon: Warehouse, resource: 'Warehouse' },
      { to: '/operators', label: 'Operators', icon: UserRound, resource: 'Operator' },
      { to: '/production-lines', label: 'Production Lines', icon: Wrench, resource: 'ProductionLine' }
    ]
  },
  {
    label: 'Planning & BOM',
    items: [
      { to: '/boms', label: 'Bills of Material', icon: ListTree, resource: 'BOM' },
      { to: '/production-plans', label: 'Production Plans', icon: ClipboardList, resource: 'ProductionPlan' }
    ]
  },
  {
    label: 'Production',
    items: [
      { to: '/production-documents', label: 'Production Documents', icon: Factory, resource: 'ProductionDocument' },
      { to: '/line-stops', label: 'Line Stops', icon: Ban, resource: 'LineStop' },
      { to: '/quality-control', label: 'Quality Control', icon: ClipboardCheck, resource: 'QualityControl' }
    ]
  },
  {
    label: 'Vouchers',
    items: [
      { to: '/purchase-vouchers', label: 'Purchase Vouchers', icon: ArrowDownToLine, resource: 'PurchaseVoucher' },
      { to: '/request-vouchers', label: 'Request Vouchers', icon: FileInput, resource: 'RequestVoucher' },
      { to: '/charge-vouchers', label: 'Charge Vouchers', icon: FlaskConical, resource: 'ChargeVoucher' },
      { to: '/sales-vouchers', label: 'Sales Vouchers', icon: ArrowUpFromLine, resource: 'SalesVoucher' }
    ]
  },
  {
    label: 'Reports',
    items: [
      { to: '/reports/traceability', label: 'Production Traceability', icon: Route, resource: 'ProductionTraceability' },
      { to: '/reports/material-variance', label: 'Material Consumption Variance', icon: BarChart3, resource: 'MaterialConsumptionVariance' }
    ]
  },
  {
    label: 'Administration',
    items: [
      { to: '/users', label: 'Users', icon: Users, resource: 'SystemUser' },
      { to: '/roles', label: 'Roles', icon: KeyRound, resource: 'Role' },
      { to: '/resources', label: 'Resources', icon: ShieldCheck, resource: 'Resource' },
      { to: '/actions', label: 'Actions', icon: ListTree, resource: 'SysAction' },
      { to: '/permissions', label: 'Permissions', icon: ShieldCheck, resource: 'Permission' },
      {
        to: '/audit-log',
        label: 'Audit Log',
        icon: ScrollText,
        resource: 'AuditLog'
      }
    ]
  }
]

export const allNavItems = navSections.flatMap((s) => s.items)
