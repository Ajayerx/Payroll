import api from './axios';

export const authService = {
  login: (credentials) => api.post('/auth/login', credentials),

  register: (userData) => api.post('/auth/register', userData),

  refreshToken: (refreshToken) => api.post('/auth/refresh-token', { refreshToken }),

  logout: () => api.post('/auth/logout'),

  changePassword: (data) => api.post('/auth/change-password', data),

  forgotPassword: (email) => api.post('/auth/forgot-password', { email }),
};
