import api from './axios';

export const reportService = {
  getSalaryRegister: (params) => api.get('/reports/salary-register', { params }),

  getTaxSummary: (params) => api.get('/reports/tax-summary', { params }),

  getEmployeeEarnings: (params) => api.get('/reports/employee-earnings', { params }),

  getDepartmentSummary: (params) => api.get('/reports/department-summary', { params }),

  exportReport: (data, format) => api.post('/reports/export', data, {
    params: { format },
    responseType: 'blob',
  }),
};
