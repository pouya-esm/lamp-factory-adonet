import { apiClient } from './client.js'

export const warehousesApi = {
  getAll: () => apiClient.get('/warehouse'),
  getById: (id) => apiClient.get(`/warehouse/${id}`),
  create: (warehouse) => apiClient.post('/warehouse', warehouse),
  update: (id, warehouse) => apiClient.put(`/warehouse/${id}`, warehouse),
  remove: (id) => apiClient.delete(`/warehouse/${id}`)
}
