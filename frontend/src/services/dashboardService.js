import api from './axios';

export const dashboardService = {
  getDashboard: (params) => api.get('/dashboard', { params }),

  exportDashboard: (params) => api.get('/dashboard/export', {
    params,
    responseType: 'blob',
  }),
};
