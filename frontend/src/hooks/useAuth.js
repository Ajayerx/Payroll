import { useSelector, useDispatch } from 'react-redux';
import { useCallback } from 'react';
import { login, logoutUser, changePassword, clearError } from '../store/slices/authSlice';

export const useAuth = () => {
  const dispatch = useDispatch();
  const { user, isAuthenticated, loading, error } = useSelector((state) => state.auth);

  const handleLogin = useCallback((credentials) => dispatch(login(credentials)), [dispatch]);
  const handleLogout = useCallback(() => dispatch(logoutUser()), [dispatch]);
  const handleChangePassword = useCallback((data) => dispatch(changePassword(data)), [dispatch]);
  const handleClearError = useCallback(() => dispatch(clearError()), [dispatch]);

  const hasRole = useCallback((...roles) => {
    if (!user) return false;
    return roles.includes(user.role);
  }, [user]);

  return {
    user,
    isAuthenticated,
    loading,
    error,
    login: handleLogin,
    logout: handleLogout,
    changePassword: handleChangePassword,
    clearError: handleClearError,
    hasRole,
  };
};
