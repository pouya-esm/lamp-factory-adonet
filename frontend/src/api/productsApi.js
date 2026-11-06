import { apiClient } from './client.js'

export const productsApi = {
  getAll: () => apiClient.get('/product'),
  getById: (id) => apiClient.get(`/product/${id}`),
  create: (product) => apiClient.post('/product', product),
  update: (id, product) => apiClient.put(`/product/${id}`, product),
  remove: (id) => apiClient.delete(`/product/${id}`)
}
