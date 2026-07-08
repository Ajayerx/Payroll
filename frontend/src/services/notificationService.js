import api from './axios';

export const notificationService = {
  getAll: (params) => api.get('/notifications', { params }),

  getUnreadCount: () => api.get('/notifications/unread-count'),

  markAsRead: (ids) => api.put('/notifications/mark-read', { ids }),

  markAllAsRead: () => api.put('/notifications/mark-all-read'),

  create: (data) => api.post('/notifications', data),
};
