import api from './axios';

export const leaveService = {
  getAll: (params) => api.get('/leaves', { params }),

  getById: (id) => api.get(`/leaves/${id}`),

  create: (data) => api.post('/leaves', data),

  update: (id, data) => api.put(`/leaves/${id}`, data),

  cancel: (id) => api.put(`/leaves/${id}/cancel`),

  approve: (id, comments) => api.put(`/leaves/${id}/approve`, { comments }),

  reject: (id, comments) => api.put(`/leaves/${id}/reject`, { comments }),

  getByEmployee: (employeeId, params) => api.get(`/leaves/employee/${employeeId}`, { params }),

  getLeaveTypes: () => api.get('/leaves/types'),

  getLeaveBalance: (employeeId) => api.get(`/leaves/balance/${employeeId}`),
};
