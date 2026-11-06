import { apiClient } from './client.js'

export const rolesApi = {
  getAll: () => apiClient.get('/role'),
  getById: (id) => apiClient.get(`/role/${id}`),
  create: (role) => apiClient.post('/role', role),
  update: (id, role) => apiClient.put(`/role/${id}`, role),
  remove: (id) => apiClient.delete(`/role/${id}`)
}
