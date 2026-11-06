import { apiClient } from './client.js'

export const bomsApi = {
  getAll: () => apiClient.get('/bom'),
  getById: (id) => apiClient.get(`/bom/${id}`),
  create: (dto) => apiClient.post('/bom', dto),
  update: (id, dto) => apiClient.put(`/bom/${id}`, dto),
  remove: (id) => apiClient.delete(`/bom/${id}`)
}
