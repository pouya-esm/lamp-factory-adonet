import { apiClient } from './client.js'

export const requestVouchersApi = {
  getAll: () => apiClient.get('/requestvoucher'),
  getById: (id) => apiClient.get(`/requestvoucher/${id}`),
  create: (dto) => apiClient.post('/requestvoucher', dto),
  update: (id, dto) => apiClient.put(`/requestvoucher/${id}`, dto),
  remove: (id) => apiClient.delete(`/requestvoucher/${id}`)
}
