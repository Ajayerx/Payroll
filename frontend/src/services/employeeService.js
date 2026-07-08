import api from './axios';

export const employeeService = {
  getAll: (params) => api.get('/employees', { params }),

  getById: (id) => api.get(`/employees/${id}`),

  getCurrentEmployee: () => api.get('/employees/me'),

  create: (data) => api.post('/employees', data),

  update: (id, data) => api.put(`/employees/${id}`, data),

  delete: (id) => api.delete(`/employees/${id}`),

  search: (query) => api.get('/employees/search', { params: { query } }),

  bulkImport: (formData) => api.post('/employees/bulk-import', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  }),

  getDocuments: (id) => api.get(`/employees/${id}/documents`),

  uploadDocument: (id, formData) => api.post(`/employees/${id}/documents`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  }),
};
