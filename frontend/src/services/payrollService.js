import api from './axios';

export const payrollService = {
  getAll: (params) => api.get('/payroll', { params }),

  getById: (id) => api.get(`/payroll/${id}`),

  process: (data) => api.post('/payroll/process', data),

  update: (id, data) => api.put(`/payroll/${id}`, data),

  getSalarySlip: (id) => api.get(`/payroll/${id}/salary-slip`),

  generateSlip: (id) => api.post(`/payroll/${id}/generate-slip`),

  getByMonthYear: (month, year) => api.get(`/payroll/month/${month}/year/${year}`),

  bulkProcess: (data) => api.post('/payroll/bulk-process', data),

  exportCsv: (params) => api.get('/payroll/export/csv', { params, responseType: 'blob' }),

  exportPdf: (id) => api.get(`/payroll/${id}/export/pdf`, { responseType: 'blob' }),
};
