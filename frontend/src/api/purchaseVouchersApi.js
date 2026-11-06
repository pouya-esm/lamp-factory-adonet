import { apiClient } from './client.js'

export const purchaseVouchersApi = {
  getAll: () => apiClient.get('/purchasevoucher'),
  getById: (id) => apiClient.get(`/purchasevoucher/${id}`),
  create: (dto) => apiClient.post('/purchasevoucher', dto),
  update: (id, dto) => apiClient.put(`/purchasevoucher/${id}`, dto),
  remove: (id) => apiClient.delete(`/purchasevoucher/${id}`)
}
