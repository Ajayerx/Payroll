import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, TextField, Button, MenuItem, IconButton, Tooltip,
  Grid, Dialog, DialogTitle,
  DialogContent, DialogContentText, DialogActions
} from '@mui/material';
import {
  Add, CheckCircle, Cancel, Visibility
} from '@mui/icons-material';
import { toast } from 'react-toastify';
import { PageHeader } from '../Common/PageHeader';
import { DataTable } from '../Common/DataTable';
import { StatusBadge } from '../Common/StatusBadge';
import { StatCard } from '../Common/StatCard';
import { TableSkeleton } from '../Common/Loading';
import { EmptyState } from '../Common/EmptyState';
import { formatDate } from '../../utils/formatters';
import { useAuth } from '../../hooks/useAuth';
import { ROLES } from '../../constants';
import { leaveService } from '../../services/leaveService';

export const LeaveList = () => {
  const navigate = useNavigate();
  const { user, hasRole } = useAuth();
  const [leaves, setLeaves] = useState([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [statusFilter, setStatusFilter] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [actionDialog, setActionDialog] = useState(null);
  const [refreshTrigger, setRefreshTrigger] = useState(0);
  const isAdmin = hasRole(ROLES.ADMIN, ROLES.HR_MANAGER);

  useEffect(() => {
    setLoading(true);
    const params = { status: statusFilter || undefined, type: typeFilter || undefined, page: page + 1, pageSize: rowsPerPage };
    const fetcher = isAdmin
      ? leaveService.getAll(params)
      : leaveService.getByEmployee(user?.employeeId, { status: statusFilter || undefined });
    fetcher
      .then(res => {
        const data = res.data.items || res.data || [];
        setLeaves(Array.isArray(data) ? data : []);
        setTotal(res.data.total || data.length || 0);
      })
      .catch(() => toast.error('Failed to fetch leave requests'))
      .finally(() => setLoading(false));
  }, [isAdmin, statusFilter, typeFilter, page, rowsPerPage, user?.employeeId, refreshTrigger]);

  const handleApprove = async (id) => {
    try {
      await leaveService.approve(id, 'Approved');
      toast.success('Leave request approved');
      setActionDialog(null);
      setRefreshTrigger(t => t + 1);
    } catch { toast.error('Failed to approve'); }
  };

  const handleReject = async (id) => {
    try {
      await leaveService.reject(id, 'Rejected');
      toast.success('Leave request rejected');
      setActionDialog(null);
      setRefreshTrigger(t => t + 1);
    } catch { toast.error('Failed to reject'); }
  };

  const columns = [
    { id: 'employeeName', label: 'Employee' },
    { id: 'leaveType', label: 'Leave Type' },
    { id: 'fromDate', label: 'From', render: (r) => formatDate(r.fromDate) },
    { id: 'toDate', label: 'To', render: (r) => formatDate(r.toDate) },
    { id: 'totalDays', label: 'Days' },
    { id: 'reason', label: 'Reason' },
    { id: 'status', label: 'Status', render: (r) => <StatusBadge status={r.status} /> },
    ...(isAdmin ? [{
      id: 'actions', label: 'Actions', render: (r) => (
        <Box>
          {r.status === 'Pending' && (
            <>
              <Tooltip title="Approve"><IconButton size="small" color="success" onClick={() => setActionDialog({ id: r.id, action: 'approve' })}><CheckCircle fontSize="small" /></IconButton></Tooltip>
              <Tooltip title="Reject"><IconButton size="small" color="error" onClick={() => setActionDialog({ id: r.id, action: 'reject' })}><Cancel fontSize="small" /></IconButton></Tooltip>
            </>
          )}
          <Tooltip title="View"><IconButton size="small"><Visibility fontSize="small" /></IconButton></Tooltip>
        </Box>
      ),
    }] : []),
  ];

  const stats = {
    total: leaves.length,
    pending: leaves.filter(l => l.status === 'Pending').length,
    approved: leaves.filter(l => l.status === 'Approved').length,
    rejected: leaves.filter(l => l.status === 'Rejected').length,
  };

  return (
    <>
      <PageHeader
        title="Leave Management"
        subtitle={isAdmin ? 'Review and manage employee leave requests' : 'View and apply for leave'}
        breadcrumbs={[{ label: 'Dashboard', onClick: () => navigate('/dashboard') }, { label: 'Leaves' }]}
        action={
          <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/leaves/new')}>
            Apply for Leave
          </Button>
        }
      />

      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 6, sm: 3 }}>
          <StatCard title="Total Requests" value={stats.total} />
        </Grid>
        <Grid size={{ xs: 6, sm: 3 }}>
          <StatCard title="Pending" value={stats.pending} color="warning.main" />
        </Grid>
        <Grid size={{ xs: 6, sm: 3 }}>
          <StatCard title="Approved" value={stats.approved} color="success.main" />
        </Grid>
        <Grid size={{ xs: 6, sm: 3 }}>
          <StatCard title="Rejected" value={stats.rejected} color="error.main" />
        </Grid>
      </Grid>

      <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
        <TextField select size="small" label="Status" value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setPage(0); }} sx={{ minWidth: 150 }}>
          <MenuItem value="">All</MenuItem>
          <MenuItem value="Pending">Pending</MenuItem>
          <MenuItem value="Approved">Approved</MenuItem>
          <MenuItem value="Rejected">Rejected</MenuItem>
        </TextField>
        <TextField select size="small" label="Leave Type" value={typeFilter} onChange={(e) => { setTypeFilter(e.target.value); setPage(0); }} sx={{ minWidth: 150 }}>
          <MenuItem value="">All</MenuItem>
          <MenuItem value="Annual Leave">Annual Leave</MenuItem>
          <MenuItem value="Sick Leave">Sick Leave</MenuItem>
          <MenuItem value="Casual Leave">Casual Leave</MenuItem>
          <MenuItem value="Maternity Leave">Maternity Leave</MenuItem>
        </TextField>
      </Box>

      {loading ? (
        <TableSkeleton />
      ) : leaves.length === 0 ? (
        <EmptyState title="No leave requests" actionLabel="Apply for Leave" onAction={() => navigate('/leaves/new')} />
      ) : (
        <DataTable
          columns={columns}
          rows={leaves}
          page={page}
          rowsPerPage={rowsPerPage}
          total={total}
          onPageChange={setPage}
          onRowsPerPageChange={setRowsPerPage}
        />
      )}

      <Dialog open={Boolean(actionDialog)} onClose={() => setActionDialog(null)} maxWidth="xs" fullWidth>
        <DialogTitle>{actionDialog?.action === 'approve' ? 'Approve' : 'Reject'} Leave Request</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to {actionDialog?.action} this leave request?
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setActionDialog(null)} color="inherit">Cancel</Button>
          <Button onClick={() => actionDialog?.action === 'approve' ? handleApprove(actionDialog.id) : handleReject(actionDialog.id)}
            variant="contained" color={actionDialog?.action === 'approve' ? 'success' : 'error'}>
            {actionDialog?.action === 'approve' ? 'Approve' : 'Reject'}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};
