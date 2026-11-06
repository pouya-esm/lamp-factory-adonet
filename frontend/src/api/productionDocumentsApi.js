import { apiClient } from './client.js'

export const productionDocumentsApi = {
  getAll: () => apiClient.get('/productiondocument'),
  getById: (id) => apiClient.get(`/productiondocument/${id}`),
  create: (dto) => apiClient.post('/productiondocument', dto),
  update: (id, dto) => apiClient.put(`/productiondocument/${id}`, dto),
  remove: (id) => apiClient.delete(`/productiondocument/${id}`)
}
