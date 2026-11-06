import { apiClient } from './client.js'

export const productionLinesApi = {
  getAll: () => apiClient.get('/productionline'),
  getById: (id) => apiClient.get(`/productionline/${id}`),
  create: (line) => apiClient.post('/productionline', line),
  update: (id, line) => apiClient.put(`/productionline/${id}`, line),
  remove: (id) => apiClient.delete(`/productionline/${id}`)
}
