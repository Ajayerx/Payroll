import { useSelector, useDispatch } from 'react-redux';
import { useCallback } from 'react';
import {
  fetchEmployees,
  fetchEmployeeById,
  createEmployee,
  updateEmployee,
  clearSelected,
} from '../store/slices/employeeSlice';

export const useEmployees = () => {
  const dispatch = useDispatch();
  const { items, selected, total, loading, error } = useSelector((state) => state.employees);

  const handleFetch = useCallback((params) => dispatch(fetchEmployees(params)), [dispatch]);
  const handleFetchById = useCallback((id) => dispatch(fetchEmployeeById(id)), [dispatch]);
  const handleCreate = useCallback((data) => dispatch(createEmployee(data)), [dispatch]);
  const handleUpdate = useCallback((id, data) => dispatch(updateEmployee({ id, data })), [dispatch]);
  const handleClearSelected = useCallback(() => dispatch(clearSelected()), [dispatch]);

  return {
    items,
    selected,
    total,
    loading,
    error,
    fetchEmployees: handleFetch,
    fetchEmployeeById: handleFetchById,
    createEmployee: handleCreate,
    updateEmployee: handleUpdate,
    clearSelected: handleClearSelected,
  };
};
