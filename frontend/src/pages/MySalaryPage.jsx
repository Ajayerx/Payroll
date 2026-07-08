import { useState, useEffect, useCallback } from 'react';
import { useSelector } from 'react-redux';
import {
  Box, Grid, Card, CardContent, Typography, Button, Table,
  TableBody, TableCell, TableContainer, TableHead, TableRow,
  Divider, MenuItem, TextField, Chip, LinearProgress, Alert
} from '@mui/material';
import { PictureAsPdf, Download, AccountBalance, TrendingUp, TrendingDown } from '@mui/icons-material';
import { PageHeader } from '../components/Common/PageHeader';
import { StatCard } from '../components/Common/StatCard';
import { TableSkeleton } from '../components/Common/Loading';
import { formatCurrency } from '../utils/formatters';
import { months, downloadFile } from '../utils/helpers';
import { employeeService } from '../services/employeeService';
import { salaryService } from '../services/salaryService';
import { payrollService } from '../services/payrollService';
import { toast } from 'react-toastify';

const MySalaryPage = () => {
  const user = useSelector((state) => state.auth.user);
  const [employee, setEmployee] = useState(null);
  const [salaryData, setSalaryData] = useState(null);
  const [payrollHistory, setPayrollHistory] = useState([]);
  const [ydData, setYdData] = useState({ gross: 0, net: 0, tax: 0 });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [month, setMonth] = useState(new Date().getMonth() + 1);
  const [year, setYear] = useState(new Date().getFullYear());
  const [downloading, setDownloading] = useState(false);

  const fetchData = useCallback(async () => {
    if (!user) return;
    setLoading(true);
    setError(null);
    try {
      const empRes = await employeeService.getCurrentEmployee();
      const emp = empRes.data;
      setEmployee(emp);

      const [structureRes, historyRes] = await Promise.all([
        salaryService.getEmployeeStructure(emp.id).catch(() => null),
        payrollService.getByMonthYear(month, year).catch(() => null),
      ]);

      if (structureRes?.data) {
        setSalaryData(structureRes.data);
      }

      if (historyRes?.data?.items) {
        setPayrollHistory(historyRes.data.items);
        const ytd = historyRes.data.items.reduce((acc, p) => ({
          gross: acc.gross + (p.grossSalary || 0),
          net: acc.net + (p.netSalary || 0),
          tax: acc.tax + (p.taxDeduction || 0),
        }), { gross: 0, net: 0, tax: 0 });
        setYdData(ytd);
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to load salary data');
    } finally {
      setLoading(false);
    }
  }, [user, month, year]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleDownloadSlip = async () => {
    if (!payrollHistory.length) {
      toast.info('No payroll records found for the selected period');
      return;
    }
    const latestPayroll = payrollHistory[0];
    setDownloading(true);
    try {
      const res = await payrollService.exportPdf(latestPayroll.id);
      downloadFile(res.data, `salary_slip_${latestPayroll.month}_${latestPayroll.year}.html`);
      toast.success('Salary slip downloaded');
    } catch {
      toast.error('Failed to download salary slip');
    } finally {
      setDownloading(false);
    }
  };

  const earnings = salaryData?.earnings || [];
  const deductions = salaryData?.deductions || [];
  const totalEarnings = salaryData?.grossEarnings || earnings.reduce((s, c) => s + (c.amount || 0), 0);
  const totalDeductions = salaryData?.totalDeductions || deductions.reduce((s, c) => s + (c.amount || 0), 0);
  const netSalary = salaryData?.netSalary || (totalEarnings - totalDeductions);

  const allComponents = [
    ...earnings.map(c => ({ ...c, type: 'Earning' })),
    ...deductions.map(c => ({ ...c, type: 'Deduction', amount: Math.abs(c.amount) })),
  ];

  if (loading) {
    return (
      <>
        <PageHeader title="My Salary" subtitle="View salary breakdown, payslips, and year-to-date earnings" />
        <TableSkeleton rows={6} columns={4} />
      </>
    );
  }

  if (error) {
    return (
      <>
        <PageHeader title="My Salary" subtitle="View salary breakdown, payslips, and year-to-date earnings" />
        <Alert severity="error">{error}</Alert>
      </>
    );
  }

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
            <Button variant="contained" startIcon={<PictureAsPdf />} onClick={handleDownloadSlip} disabled={downloading}>
              {downloading ? 'Downloading...' : 'Download Slip'}
            </Button>
          </Box>
        }
      />

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
          <StatCard title="YTD Net" value={formatCurrency(ydData.net)} icon={<AccountBalance sx={{ fontSize: 36 }} />} color="secondary.main" subtitle="Financial year" />
        </Grid>
      </Grid>

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 7 }}>
          <Card>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>Salary Breakdown</Typography>
              <Divider sx={{ mb: 2 }} />
              {allComponents.length === 0 ? (
                <Typography color="text.secondary">No salary structure configured</Typography>
              ) : (
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
                      {allComponents.map((c) => (
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
                            <Typography fontWeight={500} color={c.type === 'Earning' ? 'text.primary' : 'error.main'}>
                              {c.type === 'Earning' ? formatCurrency(c.amount) : `-${formatCurrency(c.amount)}`}
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
              )}
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 5 }}>
          <Card sx={{ mb: 3 }}>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>Payroll History</Typography>
              <Divider sx={{ mb: 2 }} />
              {payrollHistory.length === 0 ? (
                <Typography color="text.secondary">No payroll records for this period</Typography>
              ) : (
                payrollHistory.map((s, i) => (
                  <Box key={s.id} sx={{
                    display: 'flex', justifyContent: 'space-between', alignItems: 'center', py: 1.5,
                    borderBottom: i < payrollHistory.length - 1 ? '1px solid' : 'none',
                    borderColor: 'divider'
                  }}>
                    <Box>
                      <Typography variant="body2" fontWeight={500}>{months[s.month - 1]?.label} {s.year}</Typography>
                      <Typography variant="caption" color="text.secondary">Gross: {formatCurrency(s.grossSalary)}</Typography>
                    </Box>
                    <Box sx={{ textAlign: 'right' }}>
                      <Typography variant="body2" fontWeight={600} color="success.main">{formatCurrency(s.netSalary)}</Typography>
                      <Chip label={s.status} size="small" color={s.status === 'Paid' ? 'success' : 'warning'} variant="outlined" />
                    </Box>
                  </Box>
                ))
              )}
            </CardContent>
          </Card>

          <Card>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>Year to Date (YTD)</Typography>
              <Divider sx={{ mb: 2 }} />
              <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1.5 }}>
                <Typography variant="body2" color="text.secondary">Total Gross</Typography>
                <Typography variant="body2" fontWeight={600}>{formatCurrency(ydData.gross)}</Typography>
              </Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1.5 }}>
                <Typography variant="body2" color="text.secondary">Total Tax</Typography>
                <Typography variant="body2" fontWeight={600} color="error.main">{formatCurrency(ydData.tax)}</Typography>
              </Box>
              <Divider sx={{ my: 1 }} />
              <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                <Typography variant="body1" fontWeight={700}>Net YTD</Typography>
                <Typography variant="body1" fontWeight={700} color="primary.main">{formatCurrency(ydData.net)}</Typography>
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
