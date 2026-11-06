import { apiClient } from './client.js'

export const salesVouchersApi = {
  getAll: () => apiClient.get('/salesvoucher'),
  getById: (id) => apiClient.get(`/salesvoucher/${id}`),
  create: (dto) => apiClient.post('/salesvoucher', dto),
  update: (id, dto) => apiClient.put(`/salesvoucher/${id}`, dto),
  remove: (id) => apiClient.delete(`/salesvoucher/${id}`)
}
