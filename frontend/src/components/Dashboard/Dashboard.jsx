import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Grid, Box, Typography, Button, MenuItem, TextField, Paper } from '@mui/material';
import {
  People, AttachMoney, AccountBalance, TrendingUp, Add, Download
} from '@mui/icons-material';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, LineChart, Line, PieChart, Pie, Cell } from 'recharts';
import { StatCard } from '../Common/StatCard';
import { PageHeader } from '../Common/PageHeader';
import { StatusBadge } from '../Common/StatusBadge';
import { LoadingSpinner } from '../Common/Loading';
import { useUI } from '../../hooks/useUI';
import { formatCurrency, formatDate } from '../../utils/formatters';
import { months } from '../../utils/helpers';
import { dashboardService } from '../../services/dashboardService';
import { downloadFile } from '../../utils/helpers';
import { toast } from 'react-toastify';

const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

const COLORS = ['#1976d2', '#388e3c', '#f57c00', '#d32f2f', '#7b1fa2', '#00796b'];

export const Dashboard = () => {
  const navigate = useNavigate();
  const { selectedMonth, selectedYear, setMonth, setYear } = useUI();
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    dashboardService.getDashboard({ month: selectedMonth, year: selectedYear })
      .then(res => setData(res.data))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [selectedMonth, selectedYear]);

  if (loading) return <LoadingSpinner message="Loading dashboard..." />;

  const monthlyTrend = (data?.monthlyTrend || []).map(t => ({
    month: monthNames[parseInt(t.month) - 1] || t.month,
    amount: t.amount
  }));

  const departmentPieData = (data?.departmentOverview || []).map(d => ({
    name: d.department,
    value: d.employeeCount
  }));

  const handleExport = async () => {
    try {
      const res = await dashboardService.exportDashboard({ month: selectedMonth, year: selectedYear });
      downloadFile(res.data, `dashboard_${selectedMonth}_${selectedYear}.csv`);
      toast.success('Dashboard data exported');
    } catch {
      toast.error('Failed to export');
    }
  };

  const quickActions = [
    { label: 'Add Employee', icon: <Add />, color: 'primary', onClick: () => navigate('/employees/new') },
    { label: 'Process Payroll', icon: <AttachMoney />, color: 'secondary', onClick: () => navigate('/payroll') },
    { label: 'Generate Reports', icon: <Download />, color: 'success', onClick: () => navigate('/reports') },
  ];

  return (
    <>
      <PageHeader
        title="Dashboard"
        subtitle="Payroll overview and metrics"
        action={
          <Box sx={{ display: 'flex', gap: 1.5 }}>
            <TextField select size="small" value={selectedMonth}
              onChange={(e) => setMonth(Number(e.target.value))} sx={{ minWidth: 130 }}>
              {months.map(m => <MenuItem key={m.value} value={m.value}>{m.label}</MenuItem>)}
            </TextField>
            <TextField select size="small" value={selectedYear}
              onChange={(e) => setYear(Number(e.target.value))} sx={{ minWidth: 100 }}>
              {[2023, 2024, 2025, 2026].map(y => <MenuItem key={y} value={y}>{y}</MenuItem>)}
            </TextField>
            <Button variant="outlined" startIcon={<Download />} onClick={handleExport}>
              Export
            </Button>
          </Box>
        }
      />

      <Grid container spacing={3} sx={{ mb: 4, mt: 1.5 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard title="Total Employees" value={data?.activeEmployees || 0}
            icon={<People sx={{ fontSize: 40 }} />} color="primary.main"
            subtitle={`+${data?.newHiresThisMonth || 0} this month`}
            onClick={() => navigate('/employees')} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard title="Pending Payroll" value={formatCurrency(data?.pendingPayrollAmount || 0)}
            icon={<AttachMoney sx={{ fontSize: 40 }} />} color="warning.main"
            subtitle={`For ${data?.pendingPayrollCount || 0} employees`}
            onClick={() => navigate('/payroll')} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard title="Total Deductions" value={formatCurrency(data?.totalDeductions || 0)}
            icon={<AccountBalance sx={{ fontSize: 40 }} />} color="error.main"
            subtitle="Tax + Loan recoveries" />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard title="Net Disbursed" value={formatCurrency(data?.netDisbursedYTD || 0)}
            icon={<TrendingUp sx={{ fontSize: 40 }} />} color="success.main"
            subtitle="This financial year" />
        </Grid>
      </Grid>

      <Box sx={{ display: 'flex', gap: 1.5, mb: 4, flexWrap: 'wrap' }}>
        {quickActions.map(action => (
          <Button key={action.label} variant="contained" color={action.color}
            startIcon={action.icon} onClick={action.onClick}>
            {action.label}
          </Button>
        ))}
      </Box>

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 8 }}>
          <Paper elevation={0} sx={{ border: 1, borderColor: 'divider', borderRadius: 2, height: '100%' }}>
            <Box sx={{ p: { xs: 2.5, sm: 3 } }}>
              <Typography variant="h6" gutterBottom fontWeight={600}>Monthly Payroll Trend</Typography>
              <Box sx={{ mt: 2, mx: 'auto', width: '100%' }}>
                <ResponsiveContainer width="100%" height={320}>
                  <BarChart data={monthlyTrend} margin={{ top: 10, right: 20, left: 20, bottom: 10 }}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="month" />
                    <YAxis tickFormatter={(v) => `₹${(v / 1000).toFixed(0)}K`} />
                    <Tooltip formatter={(value) => formatCurrency(value)} />
                    <Bar dataKey="amount" fill="#1976d2" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </Box>
            </Box>
          </Paper>
        </Grid>
        <Grid size={{ xs: 12, md: 4 }}>
          <Paper elevation={0} sx={{ border: 1, borderColor: 'divider', borderRadius: 2, height: '100%' }}>
            <Box sx={{ p: { xs: 2.5, sm: 3 } }}>
              <Typography variant="h6" gutterBottom fontWeight={600}>Department Overview</Typography>
              <Box sx={{ mt: 2, mx: 'auto', width: '100%' }}>
                <ResponsiveContainer width="100%" height={320}>
                  <PieChart>
                    <Pie data={departmentPieData} cx="50%" cy="50%" outerRadius={100}
                      dataKey="value" label={({ name, value }) => `${name}: ${value}`}>
                      {departmentPieData.map((_, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
              </Box>
            </Box>
          </Paper>
        </Grid>
        <Grid size={{ xs: 12 }}>
          <Paper elevation={0} sx={{ border: 1, borderColor: 'divider', borderRadius: 2 }}>
            <Box sx={{ p: { xs: 2, sm: 3 } }}>
              <Typography variant="h6" gutterBottom fontWeight={600}>Recent Payroll Transactions</Typography>
              <Box sx={{ mt: 1 }}>
                {data?.recentTransactions?.length > 0 ? data.recentTransactions.map(t => (
                  <Box key={t.id} sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', py: 2, borderBottom: '1px solid', borderColor: 'divider' }}>
                    <Box>
                      <Typography variant="body2" fontWeight={500}>{t.employeeName}</Typography>
                      <Typography variant="caption" color="text.secondary">
                        {monthNames[t.month - 1]} {t.year} {t.processedDate && `- ${formatDate(t.processedDate)}`}
                      </Typography>
                    </Box>
                    <Box sx={{ textAlign: 'right' }}>
                      <Typography variant="body2" fontWeight={600}>{formatCurrency(t.amount)}</Typography>
                      <StatusBadge status={t.status} />
                    </Box>
                  </Box>
                )) : (
                  <Typography variant="body2" color="text.secondary" sx={{ py: 2, textAlign: 'center' }}>
                    No recent transactions
                  </Typography>
                )}
              </Box>
            </Box>
          </Paper>
        </Grid>
      </Grid>
    </>
  );
};
