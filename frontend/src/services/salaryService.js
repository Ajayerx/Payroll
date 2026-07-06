import api from './axios';

export const salaryService = {
  getComponents: () => api.get('/salary-components'),

  createComponent: (data) => api.post('/salary-components', data),

  updateComponent: (id, data) => api.put(`/salary-components/${id}`, data),

  getEmployeeStructure: (employeeId) => api.get(`/employees/${employeeId}/salary-structure`),

  updateEmployeeStructure: (employeeId, data) => api.put(`/employees/${employeeId}/salary-structure`, data),

  getEmployeeDeductions: (employeeId) => api.get(`/employees/${employeeId}/deductions`),

  addEmployeeDeduction: (employeeId, data) => api.post(`/employees/${employeeId}/deductions`, data),

  previewCalculation: (employeeId, month, year) =>
    api.get(`/employees/${employeeId}/salary-calculation`, { params: { month, year } }),
};
