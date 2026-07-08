import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, TextField, Button, MenuItem, Grid, Typography, IconButton,
  Tooltip, Card, CardContent, Dialog, DialogTitle, DialogContent,
  DialogContentText, DialogActions
} from '@mui/material';
import { toast } from 'react-toastify';
import {
  Add, Download, PictureAsPdf, Edit, CheckCircle, FilterList,
  TrendingUp, AccountBalance
} from '@mui/icons-material';
import { PageHeader } from '../Common/PageHeader';
import { DataTable } from '../Common/DataTable';
import { SearchBar } from '../Common/SearchBar';
import { StatusBadge } from '../Common/StatusBadge';
import { StatCard } from '../Common/StatCard';
import { TableSkeleton } from '../Common/Loading';
import { EmptyState } from '../Common/EmptyState';
import { formatCurrency } from '../../utils/formatters';
import { months, downloadFile } from '../../utils/helpers';
import { payrollService } from '../../services/payrollService';

export const PayrollList = () => {
  const navigate = useNavigate();
  const [payrolls, setPayrolls] = useState([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [month, setMonth] = useState(new Date().getMonth() + 1);
  const [year, setYear] = useState(new Date().getFullYear());
  const [statusFilter, setStatusFilter] = useState('');
  const [deptFilter, setDeptFilter] = useState('');
  const [searchQuery, setSearchQuery] = useState('');
  const [exporting, setExporting] = useState(false);
  const [confirmDialog, setConfirmDialog] = useState({ open: false, payroll: null, action: '' });
  const [updating, setUpdating] = useState(false);

  useEffect(() => {
    setLoading(true);
    payrollService.getAll({ month, year, status: statusFilter || undefined, page: page + 1, pageSize: rowsPerPage })
      .then(res => {
        setPayrolls(res.data.items || []);
        setTotal(res.data.total || 0);
      })
      .catch(() => toast.error('Failed to fetch payroll records'))
      .finally(() => setLoading(false));
  }, [month, year, statusFilter, page, rowsPerPage]);

  const handleExportCsv = async () => {
    setExporting(true);
    try {
      const res = await payrollService.exportCsv({ month, year, status: statusFilter || undefined });
      downloadFile(res.data, `payroll_${month}_${year}.csv`);
      toast.success('Payroll exported successfully');
    } catch {
      toast.error('Failed to export payroll');
    } finally {
      setExporting(false);
    }
  };

  const handleExportPdf = async (id) => {
    try {
      const res = await payrollService.exportPdf(id);
      downloadFile(res.data, `salary_slip_${id}.html`);
      toast.success('Salary slip downloaded');
    } catch {
      toast.error('Failed to download slip');
    }
  };

  const handleAction = async () => {
    const { payroll, action } = confirmDialog;
    if (!payroll) return;
    setUpdating(true);
    try {
      if (action === 'process') {
        await payrollService.update(payroll.id, { status: 'Processed' });
        toast.success(`Payroll processed for ${payroll.employeeName}`);
      } else if (action === 'mark-paid') {
        await payrollService.update(payroll.id, { status: 'Paid' });
        toast.success(`Payroll marked as paid for ${payroll.employeeName}`);
      } else if (action === 'edit') {
        navigate(`/payroll/${payroll.id}/edit`);
        return;
      }
      setConfirmDialog({ open: false, payroll: null, action: '' });
      setPayrolls(prev => prev.map(p => p.id === payroll.id
        ? { ...p, status: action === 'process' ? 'Processed' : 'Paid' } : p));
    } catch {
      toast.error(`Failed to ${action} payroll`);
    } finally {
      setUpdating(false);
    }
  };

  const handleGenerateSlip = async (id) => {
    try {
      await payrollService.generateSlip(id);
      toast.success('Salary slip generated');
      setPayrolls(prev => prev.map(p => p.id === id ? { ...p, status: 'Processed' } : p));
    } catch {
      toast.error('Failed to generate slip');
    }
  };

  const stats = {
    total: payrolls.length,
    totalGross: payrolls.reduce((s, p) => s + p.grossSalary, 0),
    totalNet: payrolls.reduce((s, p) => s + p.netSalary, 0),
    totalTax: payrolls.reduce((s, p) => s + p.taxDeduction, 0),
    paid: payrolls.filter(p => p.status === 'Paid').length,
    processed: payrolls.filter(p => p.status === 'Processed').length,
    draft: payrolls.filter(p => p.status === 'Draft').length,
  };

  const columns = [
    { id: 'employeeCode', label: 'Emp Code', sortable: true },
    { id: 'employeeName', label: 'Employee', sortable: true },
    { id: 'department', label: 'Department', sortable: true },
    { id: 'grossSalary', label: 'Gross', render: (r) => formatCurrency(r.grossSalary), sortable: true },
    { id: 'taxDeduction', label: 'TDS', render: (r) => <Typography color="error.main">{formatCurrency(r.taxDeduction)}</Typography> },
    { id: 'netSalary', label: 'Net Pay', render: (r) => <Typography fontWeight={700} color="primary.main">{formatCurrency(r.netSalary)}</Typography>, sortable: true },
    {
      id: 'status', label: 'Status', render: (r) => <StatusBadge status={r.status} />,
    },
    {
      id: 'actions', label: '', render: (r) => (
        <Box sx={{ display: 'flex', gap: 0.5 }}>
          <Tooltip title="View Salary Slip">
            <IconButton size="small" sx={{ color: 'primary.main' }} onClick={() => handleExportPdf(r.id)}><PictureAsPdf fontSize="small" /></IconButton>
          </Tooltip>
          {r.status === 'Draft' && (
            <>
              <Tooltip title="Edit">
                <IconButton size="small" onClick={() => setConfirmDialog({ open: true, payroll: r, action: 'edit' })}>
                  <Edit fontSize="small" />
                </IconButton>
              </Tooltip>
              <Tooltip title="Process">
                <IconButton size="small" color="success" onClick={() => setConfirmDialog({ open: true, payroll: r, action: 'process' })}>
                  <CheckCircle fontSize="small" />
                </IconButton>
              </Tooltip>
            </>
          )}
          {r.status === 'Processed' && (
            <Tooltip title="Mark as Paid">
              <IconButton size="small" color="success" onClick={() => setConfirmDialog({ open: true, payroll: r, action: 'mark-paid' })}>
                <CheckCircle fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Box>
      ),
    },
  ];

  return (
    <>
      <PageHeader
        title="Payroll Management"
        subtitle={`${total} records · ${months.find(m => m.value === month)?.label} ${year}`}
        breadcrumbs={[{ label: 'Dashboard', onClick: () => navigate('/dashboard') }, { label: 'Payroll' }]}
        action={
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button variant="outlined" startIcon={<Download />} size="medium" onClick={handleExportCsv} disabled={exporting}>{exporting ? 'Exporting...' : 'Export CSV'}</Button>
            <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/payroll/process')}>
              Process Payroll
            </Button>
          </Box>
        }
      />

      <Grid container spacing={2.5} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard
            title="Total Payroll"
            value={formatCurrency(stats.totalGross)}
            icon={<AccountBalance sx={{ fontSize: 36 }} />}
            color="primary.main"
            subtitle={`${stats.total} employees`}
          />
        </Grid>
        <Grid size={{ xs: 6, sm: 3, md: 1.5 }}>
          <Card sx={{ height: '100%', border: 1, borderColor: 'divider', borderRadius: 2 }}>
            <CardContent sx={{ p: 2, textAlign: 'center' }}>
              <Typography variant="h4" fontWeight={700} color="success.main">{stats.paid}</Typography>
              <Typography variant="caption" color="text.secondary">Paid</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 6, sm: 3, md: 1.5 }}>
          <Card sx={{ height: '100%', border: 1, borderColor: 'divider', borderRadius: 2 }}>
            <CardContent sx={{ p: 2, textAlign: 'center' }}>
              <Typography variant="h4" fontWeight={700} color="info.main">{stats.processed}</Typography>
              <Typography variant="caption" color="text.secondary">Processed</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 6, sm: 3, md: 1.5 }}>
          <Card sx={{ height: '100%', border: 1, borderColor: 'divider', borderRadius: 2 }}>
            <CardContent sx={{ p: 2, textAlign: 'center' }}>
              <Typography variant="h4" fontWeight={700} color="warning.main">{stats.draft}</Typography>
              <Typography variant="caption" color="text.secondary">Draft</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard
            title="Total Tax Deducted"
            value={formatCurrency(stats.totalTax)}
            icon={<TrendingUp sx={{ fontSize: 36 }} />}
            color="error.main"
            subtitle="TDS for this period"
          />
        </Grid>
      </Grid>

      <Card sx={{ mb: 3, border: 1, borderColor: 'divider' }}>
        <CardContent sx={{ p: 2, '&:last-child': { pb: 2 } }}>
          <Grid container spacing={2} sx={{ alignItems: 'center' }}>
            <Grid size={{ xs: 12, md: 4 }}>
              <SearchBar
                value={searchQuery}
                onChange={setSearchQuery}
                placeholder="Search by name or employee code..."
                sx={{ minWidth: '100%' }}
              />
            </Grid>
            <Grid size={{ xs: 6, md: 2 }}>
              <TextField select size="small" label="Month" value={month} onChange={(e) => setMonth(e.target.value)} fullWidth>
                {months.map(m => <MenuItem key={m.value} value={m.value}>{m.label}</MenuItem>)}
              </TextField>
            </Grid>
            <Grid size={{ xs: 6, md: 1.5 }}>
              <TextField select size="small" label="Year" value={year} onChange={(e) => setYear(e.target.value)} fullWidth>
                {[2023, 2024, 2025, 2026].map(y => <MenuItem key={y} value={y}>{y}</MenuItem>)}
              </TextField>
            </Grid>
            <Grid size={{ xs: 6, md: 1.5 }}>
              <TextField select size="small" label="Status" value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setPage(0); }} fullWidth>
                <MenuItem value="">All</MenuItem>
                <MenuItem value="Draft">Draft</MenuItem>
                <MenuItem value="Processed">Processed</MenuItem>
                <MenuItem value="Paid">Paid</MenuItem>
              </TextField>
            </Grid>
            <Grid size={{ xs: 6, md: 1.5 }}>
              <TextField select size="small" label="Dept" value={deptFilter} onChange={(e) => { setDeptFilter(e.target.value); setPage(0); }} fullWidth>
                <MenuItem value="">All</MenuItem>
                {['Engineering', 'Sales', 'HR', 'Finance', 'Operations'].map(d => (
                  <MenuItem key={d} value={d}>{d}</MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid size={{ xs: 12, md: 1.5 }}>
              <Button variant="text" size="small" startIcon={<FilterList />} color="inherit"
                onClick={() => { setStatusFilter(''); setDeptFilter(''); setSearchQuery(''); setMonth(new Date().getMonth() + 1); setYear(new Date().getFullYear()); }}>
                Clear
              </Button>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {loading ? (
        <TableSkeleton rows={10} cols={8} />
      ) : payrolls.length === 0 ? (
        <EmptyState
          title="No payroll records found"
          description="Process payroll to get started"
          actionLabel="Process Payroll"
          onAction={() => navigate('/payroll/process')}
        />
      ) : (
        <DataTable
          columns={columns}
          rows={payrolls}
          page={page}
          rowsPerPage={rowsPerPage}
          total={total}
          onPageChange={setPage}
          onRowsPerPageChange={setRowsPerPage}
        />
      )}

      <Dialog open={confirmDialog.open} onClose={() => !updating && setConfirmDialog({ open: false, payroll: null, action: '' })}>
        <DialogTitle>Confirm Action</DialogTitle>
        <DialogContent>
          <DialogContentText>
            {confirmDialog.action === 'process' && `Process payroll for ${confirmDialog.payroll?.employeeName}?`}
            {confirmDialog.action === 'mark-paid' && `Mark payroll as Paid for ${confirmDialog.payroll?.employeeName}?`}
            {confirmDialog.action === 'edit' && `Edit payroll for ${confirmDialog.payroll?.employeeName}?`}
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmDialog({ open: false, payroll: null, action: '' })} disabled={updating}>Cancel</Button>
          <Button variant="contained" onClick={handleAction} disabled={updating}>
            {updating ? 'Processing...' : 'Confirm'}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};
