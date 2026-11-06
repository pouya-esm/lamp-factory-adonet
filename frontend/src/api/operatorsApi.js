import { apiClient } from './client.js'

export const operatorsApi = {
  getAll: () => apiClient.get('/operator'),
  getById: (id) => apiClient.get(`/operator/${id}`),
  create: (operator) => apiClient.post('/operator', operator),
  update: (id, operator) => apiClient.put(`/operator/${id}`, operator),
  remove: (id) => apiClient.delete(`/operator/${id}`)
}
