import { useState } from 'react';
import {
  Box, Grid, Card, CardContent, Typography, Button, Table,
  TableBody, TableCell, TableContainer, TableHead, TableRow,
  Divider, MenuItem, TextField, Chip, LinearProgress
} from '@mui/material';
import { PictureAsPdf, Download, AccountBalance, TrendingUp, TrendingDown } from '@mui/icons-material';
import { PageHeader } from '../components/Common/PageHeader';
import { StatCard } from '../components/Common/StatCard';
import { formatCurrency } from '../utils/formatters';
import { months } from '../utils/helpers';

const salaryComponents = [
  { name: 'Basic Salary', amount: 50000, type: 'Earning', percentage: 52 },
  { name: 'House Rent Allowance', amount: 25000, type: 'Earning', percentage: 26 },
  { name: 'Dearness Allowance', amount: 10000, type: 'Earning', percentage: 10 },
  { name: 'Conveyance Allowance', amount: 3000, type: 'Earning', percentage: 3 },
  { name: 'Medical Allowance', amount: 2000, type: 'Earning', percentage: 2 },
  { name: 'Special Allowance', amount: 5000, type: 'Earning', percentage: 5 },
  { name: 'Performance Bonus', amount: 2000, type: 'Earning', percentage: 2 },
  { name: 'Professional Tax', amount: -200, type: 'Deduction' },
  { name: 'Provident Fund', amount: -6000, type: 'Deduction' },
  { name: 'Income Tax', amount: -5000, type: 'Deduction' },
];

const salaryHistory = [
  { month: 'Jun', year: 2025, gross: 97000, net: 85800, status: 'Paid' },
  { month: 'May', year: 2025, gross: 97000, net: 85800, status: 'Paid' },
  { month: 'Apr', year: 2025, gross: 97000, net: 85800, status: 'Paid' },
  { month: 'Mar', year: 2025, gross: 95000, net: 84000, status: 'Paid' },
  { month: 'Feb', year: 2025, gross: 95000, net: 84000, status: 'Paid' },
  { month: 'Jan', year: 2025, gross: 95000, net: 84000, status: 'Paid' },
];

const yearToDate = salaryHistory.reduce((acc, s) => ({
  gross: acc.gross + s.gross,
  net: acc.net + s.net,
  tax: acc.tax + (s.gross - s.net),
}), { gross: 0, net: 0, tax: 0 });

const MySalaryPage = () => {
  const [month, setMonth] = useState(new Date().getMonth() + 1);
  const [year, setYear] = useState(new Date().getFullYear());

  const totalEarnings = salaryComponents.filter(c => c.type === 'Earning').reduce((s, c) => s + c.amount, 0);
  const totalDeductions = Math.abs(salaryComponents.filter(c => c.type === 'Deduction').reduce((s, c) => s + c.amount, 0));
  const netSalary = totalEarnings - totalDeductions;

  return (
    <>
      <PageHeader
        title="My Salary"
        subtitle="View salary breakdown, payslips, and year-to-date earnings"
        action={
          <Box sx={{ display: 'flex', gap: 1 }}>
            <TextField select size="small" value={month} onChange={(e) => setMonth(e.target.value)} sx={{ minWidth: 120 }}>
              {months.map(m => <MenuItem key={m.value} value={m.value}>{m.label}</MenuItem>)}
            </TextField>
            <TextField select size="small" value={year} onChange={(e) => setYear(e.target.value)} sx={{ minWidth: 100 }}>
              {[2023, 2024, 2025, 2026].map(y => <MenuItem key={y} value={y}>{y}</MenuItem>)}
            </TextField>
            <Button variant="contained" startIcon={<PictureAsPdf />}>Download Slip</Button>
          </Box>
        }
      />

      {/* KPI Row */}
      <Grid container spacing={2.5} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard title="Gross Salary" value={formatCurrency(totalEarnings)} icon={<TrendingUp sx={{ fontSize: 36 }} />} color="primary.main" />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard title="Total Deductions" value={formatCurrency(totalDeductions)} icon={<TrendingDown sx={{ fontSize: 36 }} />} color="error.main" />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard title="Net Salary" value={formatCurrency(netSalary)} icon={<AccountBalance sx={{ fontSize: 36 }} />} color="success.main" subtitle="Take home" />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard title="YTD Net" value={formatCurrency(yearToDate.net)} icon={<AccountBalance sx={{ fontSize: 36 }} />} color="secondary.main" subtitle="Financial year" />
        </Grid>
      </Grid>

      <Grid container spacing={3}>
        {/* Salary Breakdown */}
        <Grid size={{ xs: 12, md: 7 }}>
          <Card>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>Salary Breakdown</Typography>
              <Divider sx={{ mb: 2 }} />
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell sx={{ fontWeight: 600 }}>Component</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Type</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>%</TableCell>
                      <TableCell align="right" sx={{ fontWeight: 600 }}>Amount</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {salaryComponents.map((c) => (
                      <TableRow key={c.name} sx={{ '&:last-child td': { border: 0 } }}>
                        <TableCell>{c.name}</TableCell>
                        <TableCell>
                          <Chip
                            label={c.type}
                            size="small"
                            color={c.type === 'Earning' ? 'success' : 'error'}
                            variant="outlined"
                          />
                        </TableCell>
                        <TableCell>
                          {c.percentage ? (
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                              <LinearProgress
                                variant="determinate"
                                value={c.percentage}
                                sx={{ width: 60, height: 6, borderRadius: 3 }}
                                color={c.type === 'Earning' ? 'primary' : 'error'}
                              />
                              <Typography variant="caption">{c.percentage}%</Typography>
                            </Box>
                          ) : '-'}
                        </TableCell>
                        <TableCell align="right">
                          <Typography fontWeight={500} color={c.amount > 0 ? 'text.primary' : 'error.main'}>
                            {c.amount > 0 ? formatCurrency(c.amount) : `-${formatCurrency(Math.abs(c.amount))}`}
                          </Typography>
                        </TableCell>
                      </TableRow>
                    ))}
                    <TableRow>
                      <TableCell colSpan={3} sx={{ fontWeight: 700, borderBottom: 'none' }}>Net Salary</TableCell>
                      <TableCell align="right" sx={{ fontWeight: 700, color: 'success.main', fontSize: 16, borderBottom: 'none' }}>
                        {formatCurrency(netSalary)}
                      </TableCell>
                    </TableRow>
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </Grid>

        {/* Right column: History + YTD */}
        <Grid size={{ xs: 12, md: 5 }}>
          {/* Salary History */}
          <Card sx={{ mb: 3 }}>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>Salary History</Typography>
              <Divider sx={{ mb: 2 }} />
              {salaryHistory.map((s, i) => (
                <Box key={i} sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', py: 1.5,
                  borderBottom: i < salaryHistory.length - 1 ? '1px solid' : 'none',
                  borderColor: 'divider' }}>
                  <Box>
                    <Typography variant="body2" fontWeight={500}>{s.month} {s.year}</Typography>
                    <Typography variant="caption" color="text.secondary">Gross: {formatCurrency(s.gross)}</Typography>
                  </Box>
                  <Box sx={{ textAlign: 'right' }}>
                    <Typography variant="body2" fontWeight={600} color="success.main">{formatCurrency(s.net)}</Typography>
                    <Chip label={s.status} size="small" color="success" variant="outlined" />
                  </Box>
                </Box>
              ))}
            </CardContent>
          </Card>

          {/* Year to Date */}
          <Card>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>Year to Date (YTD)</Typography>
              <Divider sx={{ mb: 2 }} />
              <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1.5 }}>
                <Typography variant="body2" color="text.secondary">Total Gross</Typography>
                <Typography variant="body2" fontWeight={600}>{formatCurrency(yearToDate.gross)}</Typography>
              </Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1.5 }}>
                <Typography variant="body2" color="text.secondary">Total Tax</Typography>
                <Typography variant="body2" fontWeight={600} color="error.main">{formatCurrency(yearToDate.tax)}</Typography>
              </Box>
              <Divider sx={{ my: 1 }} />
              <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                <Typography variant="body1" fontWeight={700}>Net YTD</Typography>
                <Typography variant="body1" fontWeight={700} color="primary.main">{formatCurrency(yearToDate.net)}</Typography>
              </Box>
              <Box sx={{ mt: 2 }}>
                <Button fullWidth variant="outlined" startIcon={<Download />}>
                  Download Annual Summary
                </Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </>
  );
};

export default MySalaryPage;
