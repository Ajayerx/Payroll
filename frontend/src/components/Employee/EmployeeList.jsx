import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, TextField, Button, MenuItem, InputAdornment, IconButton, Tooltip
} from '@mui/material';
import {
  Add, Search, Upload, Edit, Visibility
} from '@mui/icons-material';
import { PageHeader } from '../Common/PageHeader';
import { DataTable } from '../Common/DataTable';
import { StatusBadge } from '../Common/StatusBadge';
import { TableSkeleton } from '../Common/Loading';
import { EmptyState } from '../Common/EmptyState';
import { formatDate, formatPhone } from '../../utils/formatters';
import { useEmployees } from '../../hooks/useEmployees';

const departments = ['Engineering', 'Sales', 'HR', 'Finance', 'Operations', 'Marketing'];

export const EmployeeList = () => {
  const navigate = useNavigate();
  const { items, total, loading, fetchEmployees } = useEmployees();
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [search, setSearch] = useState('');
  const [deptFilter, setDeptFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [selected, setSelected] = useState([]);

  useEffect(() => {
    fetchEmployees({ page: page + 1, pageSize: rowsPerPage, search: search || undefined, department: deptFilter || undefined, status: statusFilter || undefined });
  }, [fetchEmployees, page, rowsPerPage, search, deptFilter, statusFilter]);

  const columns = [
    { id: 'employeeCode', label: 'Emp Code', sortable: true },
    { id: 'firstName', label: 'Name', sortable: true, render: (r) => `${r.firstName} ${r.lastName}` },
    { id: 'department', label: 'Department', sortable: true },
    { id: 'designation', label: 'Designation' },
    { id: 'phone', label: 'Phone', render: (r) => formatPhone(r.phone) },
    { id: 'dateOfJoining', label: 'Joined', render: (r) => formatDate(r.dateOfJoining) },
    {
      id: 'isActive', label: 'Status', render: (r) => (
        <StatusBadge status={r.isActive ? 'Active' : 'Inactive'} />
      ),
    },
    {
      id: 'actions', label: 'Actions', render: (r) => (
        <Box>
          <Tooltip title="View"><IconButton size="small" onClick={() => navigate(`/employees/${r.id}`)}><Visibility fontSize="small" /></IconButton></Tooltip>
          <Tooltip title="Edit"><IconButton size="small" color="primary" onClick={() => navigate(`/employees/${r.id}/edit`)}><Edit fontSize="small" /></IconButton></Tooltip>
        </Box>
      ),
    },
  ];

  return (
    <>
      <PageHeader
        title="Employees"
        subtitle={`${total} employees found`}
        breadcrumbs={[{ label: 'Dashboard', onClick: () => navigate('/dashboard') }, { label: 'Employees' }]}
        action={
          <Box display="flex" gap={1}>
            <Button variant="outlined" startIcon={<Upload />} component="label">
              Import CSV
              <input type="file" hidden accept=".csv" />
            </Button>
            <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/employees/new')}>
              Add Employee
            </Button>
          </Box>
        }
      />

      <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
        <TextField
          size="small" placeholder="Search employees..."
          value={search} onChange={(e) => { setSearch(e.target.value); setPage(0); }}
          sx={{ minWidth: 280 }}
          slotProps={{ input: { startAdornment: <InputAdornment position="start"><Search /></InputAdornment> } }}
        />
        <TextField select size="small" label="Department" value={deptFilter} onChange={(e) => { setDeptFilter(e.target.value); setPage(0); }} sx={{ minWidth: 160 }}>
          <MenuItem value="">All</MenuItem>
          {departments.map(d => <MenuItem key={d} value={d}>{d}</MenuItem>)}
        </TextField>
        <TextField select size="small" label="Status" value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setPage(0); }} sx={{ minWidth: 120 }}>
          <MenuItem value="">All</MenuItem>
          <MenuItem value="Active">Active</MenuItem>
          <MenuItem value="Inactive">Inactive</MenuItem>
        </TextField>
      </Box>

      {loading ? (
        <TableSkeleton />
      ) : items.length === 0 ? (
        <EmptyState
          title="No employees found"
          description={search || deptFilter || statusFilter ? 'Try adjusting your filters' : 'Get started by adding your first employee'}
          actionLabel="Add Employee"
          onAction={() => navigate('/employees/new')}
        />
      ) : (
        <DataTable
          columns={columns}
          rows={items}
          page={page}
          rowsPerPage={rowsPerPage}
          total={total}
          onPageChange={setPage}
          onRowsPerPageChange={setRowsPerPage}
          selectable
          selected={selected}
          onSelectAllClick={() => setSelected(selected.length === items.length ? [] : items.map(r => r.id))}
          onSelectClick={(id) => setSelected(selected.includes(id) ? selected.filter(s => s !== id) : [...selected, id])}
        />
      )}
    </>
  );
};
