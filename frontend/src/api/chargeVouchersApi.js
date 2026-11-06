import { apiClient } from './client.js'

export const chargeVouchersApi = {
  getAll: () => apiClient.get('/chargevoucher'),
  getById: (id) => apiClient.get(`/chargevoucher/${id}`),
  create: (dto) => apiClient.post('/chargevoucher', dto),
  update: (id, dto) => apiClient.put(`/chargevoucher/${id}`, dto),
  remove: (id) => apiClient.delete(`/chargevoucher/${id}`)
}
