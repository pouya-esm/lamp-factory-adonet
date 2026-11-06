import { apiClient } from './client.js'

// Admin-only audit view. The backend unions the audit stamps of the 18 audited
// tables into one row per business record describing only that record's latest
// state change. Both endpoints are gated by the AuditLog.Read permission, which
// the seed grants solely to the Admin role.
export const auditApi = {
  // GET /auditlog?entityName=&page=&pageSize= — one row per record, newest first.
  getAll: ({ entityName, page, pageSize } = {}) =>
    apiClient.get('/auditlog', {
      params: {
        entityName: entityName || undefined,
        page: page || undefined,
        pageSize: pageSize || undefined
      }
    }),

  // GET /auditlog/entities — the audited entity names, for the filter dropdown.
  getEntityNames: () => apiClient.get('/auditlog/entities')
}
