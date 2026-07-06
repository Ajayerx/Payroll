import { useSelector, useDispatch } from 'react-redux';
import { useCallback } from 'react';
import {
  fetchPayrolls,
  processPayroll,
  clearSelected,
} from '../store/slices/payrollSlice';

export const usePayroll = () => {
  const dispatch = useDispatch();
  const { items, selected, total, loading, processing, error } = useSelector((state) => state.payroll);

  const handleFetch = useCallback((params) => dispatch(fetchPayrolls(params)), [dispatch]);
  const handleProcess = useCallback((data) => dispatch(processPayroll(data)), [dispatch]);
  const handleClearSelected = useCallback(() => dispatch(clearSelected()), [dispatch]);

  return {
    items,
    selected,
    total,
    loading,
    processing,
    error,
    fetchPayrolls: handleFetch,
    processPayroll: handleProcess,
    clearSelected: handleClearSelected,
  };
};
