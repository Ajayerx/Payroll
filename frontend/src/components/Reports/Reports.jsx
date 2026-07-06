import { useState, useEffect, useCallback } from 'react';
import {
  Box, Grid, Card, CardContent, Typography, Button, MenuItem,
  TextField, Divider
} from '@mui/material';
import {
  Download, PictureAsPdf, TableChart, Assessment, People,
  AccountBalance, Receipt
} from '@mui/icons-material';
import { toast } from 'react-toastify';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
  PieChart, Pie, Cell, Legend
} from 'recharts';
import { PageHeader } from '../Common/PageHeader';
import { DataTable } from '../Common/DataTable';
import { Tabs } from '../ui/Tabs';
import { TableSkeleton } from '../Common/Loading';
import { formatCurrency } from '../../utils/formatters';
import { months, downloadFile } from '../../utils/helpers';
import { reportService } from '../../services/reportService';

const COLORS = ['#1976d2', '#dc004e', '#388e3c', '#f57c00', '#7b1fa2', '#00796b'];

const reportTabs = [
  { id: 'salary', label: 'Salary Register', icon: <Receipt /> },
  { id: 'tax', label: 'Tax Summary', icon: <AccountBalance /> },
  { id: 'earnings', label: 'Employee Earnings', icon: <People /> },
  { id: 'dept', label: 'Department Summary', icon: <Assessment /> },
];

export const Reports = () => {
  const [tab, setTab] = useState('salary');
  const [month, setMonth] = useState(new Date().getMonth() + 1);
  const [year, setYear] = useState(new Date().getFullYear());
  const [exporting, setExporting] = useState(false);
  const [loading, setLoading] = useState(false);

  const [salaryRegister, setSalaryRegister] = useState([]);
  const [taxSummary, setTaxSummary] = useState(null);
  const [employeeEarnings, setEmployeeEarnings] = useState([]);
  const [departmentSummary, setDepartmentSummary] = useState([]);

  const fetchSalaryRegister = useCallback(async () => {
    setLoading(true);
    try {
      const res = await reportService.getSalaryRegister({ month, year });
      setSalaryRegister(res.data || []);
    } catch {
      setSalaryRegister([]);
    } finally {
      setLoading(false);
    }
  }, [month, year]);

  const fetchTaxSummary = useCallback(async () => {
    setLoading(true);
    try {
      const res = await reportService.getTaxSummary({ year });
      setTaxSummary(res.data || null);
    } catch {
      setTaxSummary(null);
    } finally {
      setLoading(false);
    }
  }, [year]);

  const fetchEmployeeEarnings = useCallback(async () => {
    setLoading(true);
    try {
      const res = await reportService.getEmployeeEarnings({ month, year });
      setEmployeeEarnings(res.data || []);
    } catch {
      setEmployeeEarnings([]);
    } finally {
      setLoading(false);
    }
  }, [month, year]);

  const fetchDepartmentSummary = useCallback(async () => {
    setLoading(true);
    try {
      const res = await reportService.getDepartmentSummary({ month, year });
      setDepartmentSummary(res.data || []);
    } catch {
      setDepartmentSummary([]);
    } finally {
      setLoading(false);
    }
  }, [month, year]);

  useEffect(() => {
    if (tab === 'salary') fetchSalaryRegister();
    else if (tab === 'tax') fetchTaxSummary();
    else if (tab === 'earnings') fetchEmployeeEarnings();
    else if (tab === 'dept') fetchDepartmentSummary();
  }, [tab, fetchSalaryRegister, fetchTaxSummary, fetchEmployeeEarnings, fetchDepartmentSummary]);

  const exportColumns = [
    { id: 'employeeCode', label: 'Code' },
    { id: 'employeeName', label: 'Name' },
    { id: 'department', label: 'Department' },
    { id: 'basic', label: 'Basic', render: (r) => formatCurrency(r.basic) },
    { id: 'hra', label: 'HRA', render: (r) => formatCurrency(r.hra) },
    { id: 'grossPay', label: 'Gross', render: (r) => formatCurrency(r.grossPay) },
    { id: 'tax', label: 'Tax', render: (r) => formatCurrency(r.tax) },
    { id: 'netPay', label: 'Net', render: (r) => <Typography fontWeight={600}>{formatCurrency(r.netPay)}</Typography> },
  ];

  const deptColumns = [
    { id: 'department', label: 'Department' },
    { id: 'employeeCount', label: 'Employees' },
    { id: 'totalSalary', label: 'Total Salary', render: (r) => formatCurrency(r.totalSalary) },
    { id: 'averageSalary', label: 'Average', render: (r) => formatCurrency(r.averageSalary) },
  ];

  const handleExport = async (format) => {
    setExporting(true);
    try {
      const res = await reportService.exportReport({ month, year, reportType: tab }, format);
      downloadFile(res.data, `payroll_report_${month}_${year}.${format === 'pdf' ? 'html' : format === 'excel' ? 'xls' : 'csv'}`);
      toast.success(`Report exported as ${format.toUpperCase()}`);
    } catch {
      toast.error('Failed to export report');
    } finally {
      setExporting(false);
    }
  };

  const chartData = salaryRegister.slice(0, 10).map(r => ({
    code: r.employeeCode,
    gross: r.grossPay,
    net: r.netPay,
  }));

  const pieData = departmentSummary.map(d => ({
    name: d.department,
    employees: d.employeeCount,
  }));

  return (
    <>
      <PageHeader
        title="Reports & Analytics"
        subtitle="View and export payroll reports"
        breadcrumbs={[{ label: 'Dashboard', onClick: () => {} }, { label: 'Reports' }]}
        action={
          <Box sx={{ display: 'flex', gap: 1.5 }}>
            <Button variant="outlined" startIcon={<PictureAsPdf />} disabled={exporting} onClick={() => handleExport('pdf')}>PDF</Button>
            <Button variant="outlined" startIcon={<TableChart />} disabled={exporting} onClick={() => handleExport('excel')}>Excel</Button>
            <Button variant="outlined" startIcon={<Download />} disabled={exporting} onClick={() => handleExport('csv')}>CSV</Button>
          </Box>
        }
      />

      <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
        <TextField select size="small" label="Month" value={month} onChange={(e) => setMonth(e.target.value)} sx={{ minWidth: 130 }}>
          {months.map(m => <MenuItem key={m.value} value={m.value}>{m.label}</MenuItem>)}
        </TextField>
        <TextField select size="small" label="Year" value={year} onChange={(e) => setYear(e.target.value)} sx={{ minWidth: 100 }}>
          {[2023, 2024, 2025, 2026].map(y => <MenuItem key={y} value={y}>{y}</MenuItem>)}
        </TextField>
      </Box>

      <Box sx={{ mb: 3 }}>
        <Tabs
          tabs={reportTabs}
          activeTab={tab}
          onChange={setTab}
          ariaLabel="Report type selection"
        />
      </Box>

      {loading && <TableSkeleton rows={5} cols={8} />}

      {!loading && tab === 'salary' && (
        <Box>
          {salaryRegister.length > 0 && (
            <Grid container spacing={3} sx={{ mb: 3 }}>
              <Grid size={{ xs: 12, md: 8 }}>
                <Card sx={{ height: '100%' }}>
                  <CardContent sx={{ p: { xs: 2, sm: 3 } }}>
                    <Typography variant="h6" fontWeight={600} gutterBottom>Salary Distribution</Typography>
                    <Box sx={{ mt: 2, mx: 'auto', width: '100%' }}>
                      <ResponsiveContainer width="100%" height={320}>
                        <BarChart data={chartData} margin={{ top: 10, right: 20, left: 20, bottom: 10 }}>
                          <CartesianGrid strokeDasharray="3 3" />
                          <XAxis dataKey="code" />
                          <YAxis tickFormatter={(v) => `\u20b9${(v / 1000).toFixed(0)}K`} />
                          <Tooltip formatter={(value) => formatCurrency(value)} />
                          <Bar dataKey="gross" fill="#1976d2" name="Gross" radius={[4, 4, 0, 0]} />
                          <Bar dataKey="net" fill="#388e3c" name="Net" radius={[4, 4, 0, 0]} />
                        </BarChart>
                      </ResponsiveContainer>
                    </Box>
                  </CardContent>
                </Card>
              </Grid>
              <Grid size={{ xs: 12, md: 4 }}>
                <Card sx={{ height: '100%' }}>
                  <CardContent sx={{ p: { xs: 2, sm: 3 } }}>
                    <Typography variant="h6" fontWeight={600} gutterBottom>Department Split</Typography>
                    <Box sx={{ mt: 2, mx: 'auto', width: '100%' }}>
                      <ResponsiveContainer width="100%" height={320}>
                        <PieChart>
                          <Pie data={pieData} dataKey="employees" nameKey="name" cx="50%" cy="50%" outerRadius={90} label>
                            {pieData.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]} />)}
                          </Pie>
                          <Legend />
                        </PieChart>
                      </ResponsiveContainer>
                    </Box>
                  </CardContent>
                </Card>
              </Grid>
            </Grid>
          )}
          <DataTable columns={exportColumns} rows={salaryRegister} page={0} rowsPerPage={20} total={salaryRegister.length} />
        </Box>
      )}

      {!loading && tab === 'tax' && (
        <Card>
          <CardContent>
            <Typography variant="h6" fontWeight={600} gutterBottom>Tax Summary</Typography>
            <Typography variant="body2" color="text.secondary">Tax deduction summary for the selected period.</Typography>
            <Box sx={{ mt: 2 }}>
              <Grid container spacing={2}>
                <Grid size={{ xs: 12, sm: 3 }}>
                  <Typography variant="h5" fontWeight={700} color="primary.main">
                    {formatCurrency(taxSummary?.totalTaxDeducted || 0)}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">Total Tax Deducted</Typography>
                </Grid>
                <Grid size={{ xs: 12, sm: 3 }}>
                  <Typography variant="h5" fontWeight={700}>
                    {formatCurrency(taxSummary?.averageTax || 0)}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">Average Per Employee</Typography>
                </Grid>
                <Grid size={{ xs: 12, sm: 3 }}>
                  <Typography variant="h5" fontWeight={700}>{taxSummary?.taxedEmployees || 0}</Typography>
                  <Typography variant="caption" color="text.secondary">Employees Taxed</Typography>
                </Grid>
                <Grid size={{ xs: 12, sm: 3 }}>
                  <Typography variant="h5" fontWeight={700}>
                    {taxSummary?.totalEmployees > 0
                      ? `${((taxSummary.totalTaxDeducted / (taxSummary.totalEmployees * 100000)) * 100).toFixed(1)}%`
                      : '0%'}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">Effective Tax Rate</Typography>
                </Grid>
              </Grid>
            </Box>
          </CardContent>
        </Card>
      )}

      {!loading && tab === 'earnings' && (
        <DataTable columns={exportColumns} rows={employeeEarnings} page={0} rowsPerPage={20} total={employeeEarnings.length} />
      )}

      {!loading && tab === 'dept' && (
        <Box>
          <Grid container spacing={3} sx={{ mb: 3 }}>
            {departmentSummary.map((dept) => (
              <Grid size={{ xs: 12, sm: 6, md: 4 }} key={dept.department}>
                <Card>
                  <CardContent>
                    <Typography variant="h6" fontWeight={600}>{dept.department}</Typography>
                    <Divider sx={{ my: 1 }} />
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                      <Typography variant="body2">Employees</Typography>
                      <Typography variant="body2" fontWeight={600}>{dept.employeeCount}</Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                      <Typography variant="body2">Total Salary</Typography>
                      <Typography variant="body2" fontWeight={600}>{formatCurrency(dept.totalSalary)}</Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2">Average</Typography>
                      <Typography variant="body2" fontWeight={600}>{formatCurrency(dept.averageSalary)}</Typography>
                    </Box>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>
          <DataTable columns={deptColumns} rows={departmentSummary} page={0} rowsPerPage={20} total={departmentSummary.length} />
        </Box>
      )}
    </>
  );
};
