import api from './axios';

export const settingsService = {
  getCompany: () => api.get('/settings/company'),

  updateCompany: (data) => api.put('/settings/company', data),

  getTaxSlabs: () => api.get('/settings/tax-slabs'),

  createTaxSlab: (data) => api.post('/settings/tax-slabs', data),

  getLeaveTypes: () => api.get('/settings/leave-types'),

  createLeaveType: (data) => api.post('/settings/leave-types', data),
};
