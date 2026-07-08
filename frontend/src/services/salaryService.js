import api from './axios';

export const salaryService = {
  getComponents: () => api.get('/salary/components'),

  createComponent: (data) => api.post('/salary/components', data),

  updateComponent: (id, data) => api.put(`/salary/components/${id}`, data),

  getEmployeeStructure: (employeeId) => api.get(`/salary/employees/${employeeId}/structure`),

  updateEmployeeStructure: (employeeId, data) => api.put(`/salary/employees/${employeeId}/structure`, data),

  getEmployeeDeductions: (employeeId) => api.get(`/salary/employees/${employeeId}/deductions`),

  addEmployeeDeduction: (employeeId, data) => api.post(`/salary/employees/${employeeId}/deductions`, data),

  previewCalculation: (employeeId, month, year) =>
    api.get(`/salary/employees/${employeeId}/calculation`, { params: { month, year } }),
};
