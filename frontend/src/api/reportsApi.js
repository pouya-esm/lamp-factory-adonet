import { apiClient } from './client.js'

export const reportsApi = {
  getProductionTraceability: (params) => apiClient.get('/productiontraceability', { params }),
  getMaterialConsumptionVariance: (params) => apiClient.get('/materialconsumptionvariance', { params })
}
