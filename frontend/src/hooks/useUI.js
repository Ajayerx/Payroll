import { useSelector, useDispatch } from 'react-redux';
import { useCallback } from 'react';
import { toggleSidebar, setTheme, setSelectedMonth, setSelectedYear } from '../store/slices/uiSlice';

export const useUI = () => {
  const dispatch = useDispatch();
  const { sidebarOpen, theme, selectedMonth, selectedYear } = useSelector((state) => state.ui);

  const handleToggleSidebar = useCallback(() => dispatch(toggleSidebar()), [dispatch]);
  const handleSetTheme = useCallback((t) => dispatch(setTheme(t)), [dispatch]);
  const handleSetMonth = useCallback((m) => dispatch(setSelectedMonth(m)), [dispatch]);
  const handleSetYear = useCallback((y) => dispatch(setSelectedYear(y)), [dispatch]);

  return {
    sidebarOpen,
    theme,
    selectedMonth,
    selectedYear,
    toggleSidebar: handleToggleSidebar,
    setTheme: handleSetTheme,
    setMonth: handleSetMonth,
    setYear: handleSetYear,
  };
};
