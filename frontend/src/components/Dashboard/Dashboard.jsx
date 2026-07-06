import { useNavigate } from 'react-router-dom';
import { Grid, Box, Typography, Button, MenuItem, TextField, Paper } from '@mui/material';
import {
  People, AttachMoney, AccountBalance, TrendingUp, Add, Download
} from '@mui/icons-material';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, LineChart, Line } from 'recharts';
import { StatCard } from '../Common/StatCard';
import { PageHeader } from '../Common/PageHeader';
import { StatusBadge } from '../Common/StatusBadge';
import { useUI } from '../../hooks/useUI';
import { formatCurrency, formatDate } from '../../utils/formatters';
import { months } from '../../utils/helpers';

const monthlyPayrollData = [
  { month: 'Jan', amount: 450000 },
  { month: 'Feb', amount: 445000 },
  { month: 'Mar', amount: 460000 },
  { month: 'Apr', amount: 455000 },
  { month: 'May', amount: 470000 },
  { month: 'Jun', amount: 465000 },
];

const departmentData = [
  { name: 'Engineering', count: 45, budget: 2500000 },
  { name: 'Sales', count: 30, budget: 1800000 },
  { name: 'HR', count: 12, budget: 600000 },
  { name: 'Finance', count: 15, budget: 750000 },
  { name: 'Operations', count: 20, budget: 900000 },
];

const makeId = (i) => `550e8400-e29b-41d4-a716-44665544${String(i).padStart(4, '0')}`;

const recentTransactions = [
  { id: makeId(1), employee: 'Rahul Sharma', month: 'June', amount: 75000, status: 'Paid', date: '2025-06-01' },
  { id: makeId(2), employee: 'Priya Patel', month: 'June', amount: 68000, status: 'Paid', date: '2025-06-01' },
  { id: makeId(3), employee: 'Amit Kumar', month: 'May', amount: 82000, status: 'Processed', date: '2025-05-28' },
  { id: makeId(4), employee: 'Sneha Reddy', month: 'May', amount: 55000, status: 'Draft', date: '2025-05-25' },
  { id: makeId(5), employee: 'Vikram Singh', month: 'May', amount: 95000, status: 'Processed', date: '2025-05-28' },
];

export const Dashboard = () => {
  const navigate = useNavigate();
  const { selectedMonth, selectedYear, setMonth, setYear } = useUI();
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
            <TextField
              select
              size="small"
              value={selectedMonth}
              onChange={(e) => setMonth(Number(e.target.value))}
              sx={{ minWidth: 130 }}
            >
              {months.map(m => <MenuItem key={m.value} value={m.value}>{m.label}</MenuItem>)}
            </TextField>
            <TextField
              select
              size="small"
              value={selectedYear}
              onChange={(e) => setYear(Number(e.target.value))}
              sx={{ minWidth: 100 }}
            >
              {[2023, 2024, 2025, 2026].map(y => <MenuItem key={y} value={y}>{y}</MenuItem>)}
            </TextField>
          </Box>
        }
      />

      <Grid container spacing={3} sx={{ mb: 4, mt: 1.5 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard
            title="Total Employees"
            value="122"
            icon={<People sx={{ fontSize: 40 }} />}
            color="primary.main"
            subtitle="+3 this month"
            onClick={() => navigate('/employees')}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard
            title="Pending Payroll"
            value="₹4,65,000"
            icon={<AttachMoney sx={{ fontSize: 40 }} />}
            color="warning.main"
            subtitle="For 18 employees"
            onClick={() => navigate('/payroll')}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard
            title="Total Deductions"
            value="₹1,24,500"
            icon={<AccountBalance sx={{ fontSize: 40 }} />}
            color="error.main"
            subtitle="Tax + Loan recoveries"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard
            title="Net Disbursed"
            value="₹28,50,000"
            icon={<TrendingUp sx={{ fontSize: 40 }} />}
            color="success.main"
            subtitle="This financial year"
          />
        </Grid>
      </Grid>

      <Box sx={{ display: 'flex', gap: 1.5, mb: 4, flexWrap: 'wrap' }}>
        {quickActions.map(action => (
          <Button key={action.label} variant="contained" color={action.color} startIcon={action.icon} onClick={action.onClick}>
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
                  <BarChart data={monthlyPayrollData} margin={{ top: 10, right: 20, left: 20, bottom: 10 }}>
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
                  <LineChart data={departmentData} margin={{ top: 10, right: 20, left: 20, bottom: 10 }}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="name" />
                    <YAxis />
                    <Tooltip />
                    <Line type="monotone" dataKey="count" stroke="#1976d2" strokeWidth={2} />
                  </LineChart>
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
                {recentTransactions.map(t => (
                  <Box key={t.id} sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', py: 2, borderBottom: '1px solid', borderColor: 'divider' }}>
                    <Box>
                      <Typography variant="body2" fontWeight={500}>{t.employee}</Typography>
                      <Typography variant="caption" color="text.secondary">{t.month} {t.date && `- ${formatDate(t.date)}`}</Typography>
                    </Box>
                    <Box sx={{ textAlign: 'right' }}>
                      <Typography variant="body2" fontWeight={600}>{formatCurrency(t.amount)}</Typography>
                      <StatusBadge status={t.status} />
                    </Box>
                  </Box>
                ))}
              </Box>
            </Box>
          </Paper>
        </Grid>
      </Grid>
    </>
  );
};
