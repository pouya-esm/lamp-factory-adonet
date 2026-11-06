import { apiClient } from './client.js'

export const qualityControlApi = {
  getAll: () => apiClient.get('/qualitycontrol'),
  getById: (id) => apiClient.get(`/qualitycontrol/${id}`),
  create: (qc) => apiClient.post('/qualitycontrol', qc),
  update: (id, qc) => apiClient.put(`/qualitycontrol/${id}`, qc),
  remove: (id) => apiClient.delete(`/qualitycontrol/${id}`)
}
