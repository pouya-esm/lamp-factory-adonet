import { apiClient } from './client.js'

export const productionPlansApi = {
  getAll: () => apiClient.get('/productionplan'),
  getById: (id) => apiClient.get(`/productionplan/${id}`),
  create: (dto) => apiClient.post('/productionplan', dto),
  update: (id, dto) => apiClient.put(`/productionplan/${id}`, dto),
  remove: (id) => apiClient.delete(`/productionplan/${id}`)
}
