import { useState, useEffect, useCallback, useRef } from 'react';
import { notificationService } from '../services/notificationService';

/**
 * Hook for managing notifications with polling fallback.
 * Upgrade to SignalR by:
 * 1. npm install @microsoft/signalr --legacy-peer-deps
 * 2. Import HubConnectionBuilder from @microsoft/signalr
 * 3. Subscribe to "ReceiveNotification" event
 * 4. Remove polling interval
 */
export const useNotifications = (pollIntervalMs = 30000) => {
  const [notifications, setNotifications] = useState([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const intervalRef = useRef(null);

  const fetchNotifications = useCallback(async () => {
    try {
      const [listRes, countRes] = await Promise.all([
        notificationService.getAll({ page: 1, pageSize: 20 }),
        notificationService.getUnreadCount(),
      ]);
      setNotifications(listRes.data?.items || []);
      setTotal(listRes.data?.total || 0);
      setUnreadCount(countRes.data?.count || 0);
    } catch {
      // silently fail
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchNotifications();
    intervalRef.current = setInterval(fetchNotifications, pollIntervalMs);
    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, [fetchNotifications, pollIntervalMs]);

  const markAsRead = async (ids) => {
    try {
      await notificationService.markAsRead(ids);
      setNotifications(prev =>
        prev.map(n => ids.includes(n.id) ? { ...n, isRead: true } : n)
      );
      setUnreadCount(prev => Math.max(0, prev - ids.length));
    } catch {
      // silently fail
    }
  };

  const markAllAsRead = async () => {
    try {
      await notificationService.markAllAsRead();
      setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
      setUnreadCount(0);
    } catch {
      // silently fail
    }
  };

  return {
    notifications,
    unreadCount,
    total,
    loading,
    dropdownOpen,
    setDropdownOpen,
    markAsRead,
    markAllAsRead,
    refresh: fetchNotifications,
  };
};
