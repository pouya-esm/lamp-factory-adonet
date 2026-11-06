import { Routes, Route } from 'react-router-dom'
import Layout from './components/Layout.jsx'
import ProtectedRoute from './components/ProtectedRoute.jsx'
import LoginPage from './pages/LoginPage.jsx'
import DashboardPage from './pages/DashboardPage.jsx'
import PlaceholderPage from './pages/PlaceholderPage.jsx'
import ProductsPage from './pages/products/ProductsPage.jsx'
import WarehousesPage from './pages/warehouses/WarehousesPage.jsx'
import OperatorsPage from './pages/operators/OperatorsPage.jsx'
import ProductionLinesPage from './pages/production-lines/ProductionLinesPage.jsx'
import PurchaseVouchersPage from './pages/purchase-vouchers/PurchaseVouchersPage.jsx'
import RequestVouchersPage from './pages/request-vouchers/RequestVouchersPage.jsx'
import SalesVouchersPage from './pages/sales-vouchers/SalesVouchersPage.jsx'
import ChargeVouchersPage from './pages/charge-vouchers/ChargeVouchersPage.jsx'
import BOMsPage from './pages/boms/BOMsPage.jsx'
import ProductionPlansPage from './pages/production-plans/ProductionPlansPage.jsx'
import ProductionDocumentsPage from './pages/production-documents/ProductionDocumentsPage.jsx'
import QualityControlPage from './pages/quality-control/QualityControlPage.jsx'
import LineStopsPage from './pages/line-stops/LineStopsPage.jsx'
import UsersPage from './pages/users/UsersPage.jsx'
import RolesPage from './pages/roles/RolesPage.jsx'
import ResourcesPage from './pages/resources/ResourcesPage.jsx'
import SysActionsPage from './pages/sys-actions/SysActionsPage.jsx'
import PermissionsPage from './pages/permissions/PermissionsPage.jsx'
import AuditLogPage from './pages/audit/AuditLogPage.jsx'
import ProductionTraceabilityPage from './pages/reports/ProductionTraceabilityPage.jsx'
import MaterialConsumptionVariancePage from './pages/reports/MaterialConsumptionVariancePage.jsx'
import { navSections } from './components/navConfig.js'

// Modules with a real screen built already — everything else in navConfig
// still falls back to PlaceholderPage until we get to it.
const builtRoutes = {
  '/products': ProductsPage,
  '/warehouses': WarehousesPage,
  '/operators': OperatorsPage,
  '/production-lines': ProductionLinesPage,
  '/purchase-vouchers': PurchaseVouchersPage,
  '/request-vouchers': RequestVouchersPage,
  '/sales-vouchers': SalesVouchersPage,
  '/charge-vouchers': ChargeVouchersPage,
  '/boms': BOMsPage,
  '/production-plans': ProductionPlansPage,
  '/production-documents': ProductionDocumentsPage,
  '/quality-control': QualityControlPage,
  '/line-stops': LineStopsPage,
  '/users': UsersPage,
  '/roles': RolesPage,
  '/resources': ResourcesPage,
  '/actions': SysActionsPage,
  '/permissions': PermissionsPage,
  '/audit-log': AuditLogPage,
  '/reports/traceability': ProductionTraceabilityPage,
  '/reports/material-variance': MaterialConsumptionVariancePage
}

const placeholderRoutes = navSections
  .filter((s) => s.label !== 'Overview')
  .flatMap((s) => s.items)
  .filter((item) => !builtRoutes[item.to])

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route
        path="/"
        element={
          <ProtectedRoute>
            <Layout />
          </ProtectedRoute>
        }
      >
        <Route index element={<DashboardPage />} />
        <Route path="products" element={<ProductsPage />} />
        <Route path="warehouses" element={<WarehousesPage />} />
        <Route path="operators" element={<OperatorsPage />} />
        <Route path="production-lines" element={<ProductionLinesPage />} />
        <Route path="purchase-vouchers" element={<PurchaseVouchersPage />} />
        <Route path="request-vouchers" element={<RequestVouchersPage />} />
        <Route path="sales-vouchers" element={<SalesVouchersPage />} />
        <Route path="charge-vouchers" element={<ChargeVouchersPage />} />
        <Route path="boms" element={<BOMsPage />} />
        <Route path="production-plans" element={<ProductionPlansPage />} />
        <Route path="production-documents" element={<ProductionDocumentsPage />} />
        <Route path="quality-control" element={<QualityControlPage />} />
        <Route path="line-stops" element={<LineStopsPage />} />
        <Route path="users" element={<UsersPage />} />
        <Route path="roles" element={<RolesPage />} />
        <Route path="resources" element={<ResourcesPage />} />
        <Route path="actions" element={<SysActionsPage />} />
        <Route path="permissions" element={<PermissionsPage />} />
        <Route path="audit-log" element={<AuditLogPage />} />
        <Route path="reports/traceability" element={<ProductionTraceabilityPage />} />
        <Route path="reports/material-variance" element={<MaterialConsumptionVariancePage />} />
        {placeholderRoutes.map(({ to, label }) => (
          <Route key={to} path={to.slice(1)} element={<PlaceholderPage title={label} />} />
        ))}
      </Route>
    </Routes>
  )
}
