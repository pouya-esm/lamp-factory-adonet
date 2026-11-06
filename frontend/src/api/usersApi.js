import { apiClient } from './client.js'

export const usersApi = {
  getAll: () => apiClient.get('/user'),
  getById: (id) => apiClient.get(`/user/${id}`),
  create: (user) => apiClient.post('/user', user),
  update: (id, user) => apiClient.put(`/user/${id}`, user),
  remove: (id) => apiClient.delete(`/user/${id}`)
}
