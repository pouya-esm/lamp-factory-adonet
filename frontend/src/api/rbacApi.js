import { apiClient } from './client.js'

export const rbacApi = {
  resources: {
    // Sidebar driver: the resources this account can reach through its roles.
    // Queried live from the DB on login/refresh, so nothing permission-related
    // needs to live in the JWT (which carries only id + name).
    getAccessible: () => apiClient.get('/resource/my-accessible'),
    getAll: () => apiClient.get('/resource'),
    getById: (id) => apiClient.get(`/resource/${id}`),
    create: (resource) => apiClient.post('/resource', resource),
    update: (id, resource) => apiClient.put(`/resource/${id}`, resource),
    remove: (id) => apiClient.delete(`/resource/${id}`)
  },
  actions: {
    getAll: () => apiClient.get('/sysaction'),
    getById: (id) => apiClient.get(`/sysaction/${id}`),
    create: (action) => apiClient.post('/sysaction', action),
    update: (id, action) => apiClient.put(`/sysaction/${id}`, action),
    remove: (id) => apiClient.delete(`/sysaction/${id}`)
  },
  permissions: {
    getAll: () => apiClient.get('/permission'),
    getById: (id) => apiClient.get(`/permission/${id}`),
    create: (dto) => apiClient.post('/permission', dto),
    update: (id, dto) => apiClient.put(`/permission/${id}`, dto),
    remove: (id) => apiClient.delete(`/permission/${id}`)
  }
}
