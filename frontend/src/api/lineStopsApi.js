import { apiClient } from './client.js'

export const lineStopsApi = {
  getAll: () => apiClient.get('/linestop'),
  getById: (id) => apiClient.get(`/linestop/${id}`),
  create: (lineStop) => apiClient.post('/linestop', lineStop),
  update: (id, lineStop) => apiClient.put(`/linestop/${id}`, lineStop),
  remove: (id) => apiClient.delete(`/linestop/${id}`)
}
