import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, Grid, Card, CardContent, Typography, Button, MenuItem,
  TextField, Divider, LinearProgress, Alert, Stepper, Step, StepLabel, Chip
} from '@mui/material';
import {
  ArrowBack, Send, AttachMoney, AccountBalance, CheckCircle
} from '@mui/icons-material';
import { toast } from 'react-toastify';
import { PageHeader } from '../Common/PageHeader';
import { DataTable } from '../Common/DataTable';
import { SearchBar } from '../Common/SearchBar';
import { LoadingSpinner } from '../Common/Loading';
import { formatCurrency } from '../../utils/formatters';
import { months } from '../../utils/helpers';
import { payrollService } from '../../services/payrollService';
import { employeeService } from '../../services/employeeService';
import { salaryService } from '../../services/salaryService';

const steps = ['Select Employees', 'Review & Calculate', 'Confirm & Process'];

export const PayrollProcess = () => {
  const navigate = useNavigate();
  const [activeStep, setActiveStep] = useState(0);
  const [month, setMonth] = useState(new Date().getMonth() + 1);
  const [year, setYear] = useState(new Date().getFullYear());
  const [selected, setSelected] = useState([]);
  const [processing, setProcessing] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [allEmployees, setAllEmployees] = useState([]);
  const [loadingEmployees, setLoadingEmployees] = useState(false);
  const [salaryBreakdowns, setSalaryBreakdowns] = useState({});
  const [loadingBreakdown, setLoadingBreakdown] = useState(false);

  useEffect(() => {
    setLoadingEmployees(true);
    employeeService.getAll({ page: 1, pageSize: 100 })
      .then(res => setAllEmployees(res.data.items || []))
      .catch(() => toast.error('Failed to load employees'))
      .finally(() => setLoadingEmployees(false));
  }, []);

  const fetchBreakdowns = useCallback(async () => {
    if (selected.length === 0) return;
    setLoadingBreakdown(true);
    const results = {};
    await Promise.all(
      selected.map(async (empId) => {
        try {
          const res = await salaryService.previewCalculation(empId, month, year);
          results[empId] = res.data;
        } catch {
          const emp = allEmployees.find(e => e.id === empId);
          results[empId] = {
            employeeId: empId,
            employeeName: emp ? `${emp.firstName} ${emp.lastName}` : 'Unknown',
            grossEarnings: 0,
            totalDeductions: 0,
            netSalary: 0,
            earnings: [],
            deductions: [],
          };
        }
      })
    );
    setSalaryBreakdowns(results);
    setLoadingBreakdown(false);
  }, [selected, month, year, allEmployees]);

  useEffect(() => {
    if (activeStep === 1) {
      fetchBreakdowns();
    }
  }, [activeStep, fetchBreakdowns]);

  const searchedEmployees = allEmployees.filter(e =>
    !searchQuery || `${e.firstName} ${e.lastName} ${e.employeeCode}`.toLowerCase().includes(searchQuery.toLowerCase())
  ).map(e => ({
    id: e.id,
    code: e.employeeCode,
    name: `${e.firstName} ${e.lastName}`,
    department: e.department || '',
  }));

  const selectedEmployees = allEmployees.filter(e => selected.includes(e.id)).map(e => ({
    id: e.id,
    code: e.employeeCode,
    name: `${e.firstName} ${e.lastName}`,
    department: e.department || '',
  }));

  const allBreakdowns = selectedEmployees.map(e => salaryBreakdowns[e.id]).filter(Boolean);
  const totalGross = allBreakdowns.reduce((s, b) => s + (b.grossEarnings || 0), 0);
  const totalDeductions = allBreakdowns.reduce((s, b) => s + (b.totalDeductions || 0), 0);
  const totalNet = allBreakdowns.reduce((s, b) => s + (b.netSalary || 0), 0);

  const handleNext = () => {
    if (activeStep === 0 && selected.length === 0) {
      toast.error('Select at least one employee');
      return;
    }
    setActiveStep(prev => prev + 1);
  };

  const handleProcess = async () => {
    setProcessing(true);
    try {
      await payrollService.process({
        employeeIds: selected,
        month,
        year,
        remarks: `Payroll for ${months.find(m => m.value === month)?.label} ${year}`
      });
      toast.success(`Payroll for ${selected.length} employees processed successfully`);
      navigate('/payroll');
    } catch {
      toast.error('Failed to process payroll');
    } finally {
      setProcessing(false);
    }
  };

  const columns = [
    { id: 'code', label: 'Code' },
    { id: 'name', label: 'Employee' },
    { id: 'department', label: 'Department' },
  ];

  return (
    <>
      <PageHeader
        title="Process Payroll"
        subtitle={`Step ${activeStep + 1} of ${steps.length}: ${steps[activeStep]}`}
        breadcrumbs={[
          { label: 'Dashboard', onClick: () => navigate('/dashboard') },
          { label: 'Payroll', onClick: () => navigate('/payroll') },
          { label: 'Process' },
        ]}
        action={
          <Button variant="outlined" startIcon={<ArrowBack />} onClick={() => activeStep === 0 ? navigate('/payroll') : setActiveStep(prev => prev - 1)}>
            {activeStep === 0 ? 'Back' : 'Previous'}
          </Button>
        }
      />

      <Stepper activeStep={activeStep} sx={{ mb: 4 }}>
        {steps.map(label => <Step key={label}><StepLabel>{label}</StepLabel></Step>)}
      </Stepper>

      {processing && <LinearProgress sx={{ mb: 2, borderRadius: 1 }} />}

      {activeStep === 0 && (
        <Card>
          <CardContent sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Box>
                <Typography variant="h6" fontWeight={600}>Select Employees</Typography>
                <Typography variant="body2" color="text.secondary">Choose employees for {months.find(m => m.value === month)?.label} {year} payroll</Typography>
              </Box>
              <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
                <SearchBar value={searchQuery} onChange={setSearchQuery} placeholder="Search employees..." sx={{ minWidth: 250 }} />
                <Box sx={{ textAlign: 'right' }}>
                  <Typography variant="h5" fontWeight={700} color="primary.main">{selected.length}</Typography>
                  <Typography variant="caption" color="text.secondary">Selected</Typography>
                </Box>
              </Box>
            </Box>

            <Box sx={{ display: 'flex', gap: 1.5, mb: 2 }}>
              <TextField select size="small" label="Month" value={month} onChange={(e) => setMonth(e.target.value)} sx={{ minWidth: 130 }}>
                {months.map(m => <MenuItem key={m.value} value={m.value}>{m.label}</MenuItem>)}
              </TextField>
              <TextField select size="small" label="Year" value={year} onChange={(e) => setYear(e.target.value)} sx={{ minWidth: 100 }}>
                {[2023, 2024, 2025, 2026].map(y => <MenuItem key={y} value={y}>{y}</MenuItem>)}
              </TextField>
            </Box>

            {loadingEmployees ? (
              <LinearProgress />
            ) : (
              <DataTable
                columns={columns}
                rows={searchedEmployees}
                page={0}
                rowsPerPage={50}
                total={searchedEmployees.length}
                selectable
                selected={selected}
                onSelectAllClick={() => setSelected(selected.length === searchedEmployees.length ? [] : searchedEmployees.map(e => e.id))}
                onSelectClick={(id) => setSelected(selected.includes(id) ? selected.filter(s => s !== id) : [...selected, id])}
              />
            )}

            <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 3 }}>
              <Button variant="contained" size="large" onClick={handleNext} disabled={selected.length === 0}>
                Review Payroll ({selected.length} employees)
              </Button>
            </Box>
          </CardContent>
        </Card>
      )}

      {activeStep === 1 && (
        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 8 }}>
            <Card>
              <CardContent sx={{ p: 3 }}>
                <Typography variant="h6" fontWeight={600} gutterBottom>Payroll Breakdown</Typography>
                <Divider sx={{ mb: 2 }} />
                {loadingBreakdown ? (
                  <LoadingSpinner />
                ) : (
                  selectedEmployees.map(emp => {
                    const breakdown = salaryBreakdowns[emp.id];
                    if (!breakdown) return null;
                    return (
                      <Box key={emp.id} sx={{ mb: 2 }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', py: 1.5, px: 2, bgcolor: 'grey.50', borderRadius: 2, mb: 1 }}>
                          <Box>
                            <Typography variant="body2" fontWeight={600}>{breakdown.employeeName}</Typography>
                            <Typography variant="caption" color="text.secondary">{emp.code} \u00b7 {emp.department}</Typography>
                          </Box>
                          <Box sx={{ textAlign: 'right' }}>
                            <Typography variant="body2" fontWeight={600} color="primary.main">{formatCurrency(breakdown.grossEarnings)}</Typography>
                            <Typography variant="caption" color="text.secondary">Gross</Typography>
                          </Box>
                        </Box>
                        <Grid container spacing={1} sx={{ px: 2 }}>
                          {breakdown.earnings?.slice(0, 3).map((e, i) => (
                            <Grid size={{ xs: 4 }} key={i}>
                              <Typography variant="caption" color="text.secondary">{e.componentName}</Typography>
                              <Typography variant="body2">{formatCurrency(e.amount)}</Typography>
                            </Grid>
                          ))}
                        </Grid>
                        <Divider sx={{ mt: 1 }} />
                      </Box>
                    );
                  })
                )}
              </CardContent>
            </Card>
          </Grid>

          <Grid size={{ xs: 12, md: 4 }}>
            <Card sx={{ position: 'sticky', top: 88 }}>
              <CardContent sx={{ p: 3 }}>
                <Typography variant="h6" fontWeight={600} gutterBottom>Summary</Typography>
                <Divider sx={{ mb: 2 }} />
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1.5 }}>
                  <Typography variant="body2" color="text.secondary">Employees</Typography>
                  <Chip label={selectedEmployees.length} size="small" color="primary" />
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                  <Typography variant="body2" color="text.secondary">Total Gross</Typography>
                  <Typography variant="body2" fontWeight={600}>{formatCurrency(totalGross)}</Typography>
                </Box>
                <Divider sx={{ my: 1 }} />
                <Typography variant="subtitle2" color="error.main" gutterBottom sx={{ mt: 1 }}>Deductions</Typography>
                {loadingBreakdown ? (
                  <LinearProgress />
                ) : (
                  allBreakdowns.length > 0 && (
                    <>
                      {allBreakdowns[0]?.deductions?.slice(0, 4).map((d, i) => (
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }} key={i}>
                          <Typography variant="caption">{d.componentName}</Typography>
                          <Typography variant="caption" color="error.main">{formatCurrency(
                            allBreakdowns.reduce((s, b) => s + (b.deductions?.find(dd => dd.componentName === d.componentName)?.amount || 0), 0)
                          )}</Typography>
                        </Box>
                      ))}
                    </>
                  )
                )}
                <Divider sx={{ my: 1.5 }} />
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                  <Typography variant="body1" fontWeight={700}>Total Deductions</Typography>
                  <Typography variant="body1" fontWeight={700} color="error.main">{formatCurrency(totalDeductions)}</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
                  <Typography variant="h6" fontWeight={700}>Net Payable</Typography>
                  <Typography variant="h6" fontWeight={700} color="primary.main">{formatCurrency(totalNet)}</Typography>
                </Box>
                <Button fullWidth variant="contained" size="large" onClick={() => setActiveStep(2)} disabled={processing || loadingBreakdown}>
                  Continue to Process
                </Button>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {activeStep === 2 && (
        <Card>
          <CardContent sx={{ p: 3 }}>
            <Box sx={{ textAlign: 'center', mb: 3 }}>
              <CheckCircle sx={{ fontSize: 64, color: 'success.main', mb: 2 }} />
              <Typography variant="h5" fontWeight={700}>Ready to Process</Typography>
              <Typography variant="body2" color="text.secondary">
                You are about to process payroll for {selectedEmployees.length} employees
              </Typography>
            </Box>

            <Alert severity="info" sx={{ mb: 3, borderRadius: 2 }}>
              <Typography variant="body2">
                <strong>Payroll Period:</strong> {months.find(m => m.value === month)?.label} {year} |
                <strong> Net Amount:</strong> {formatCurrency(totalNet)} |
                <strong> Bank Transfers:</strong> {selectedEmployees.length}
              </Typography>
            </Alert>

            <Grid container spacing={3} mb={3}>
              <Grid size={{ xs: 12, sm: 4 }}>
                <Card variant="outlined" sx={{ textAlign: 'center', py: 2 }}>
                  <AttachMoney sx={{ fontSize: 36, color: 'primary.main', mb: 1 }} />
                  <Typography variant="h5" fontWeight={700}>{formatCurrency(totalGross)}</Typography>
                  <Typography variant="caption" color="text.secondary">Total Gross</Typography>
                </Card>
              </Grid>
              <Grid size={{ xs: 12, sm: 4 }}>
                <Card variant="outlined" sx={{ textAlign: 'center', py: 2 }}>
                  <AccountBalance sx={{ fontSize: 36, color: 'error.main', mb: 1 }} />
                  <Typography variant="h5" fontWeight={700} color="error.main">{formatCurrency(totalDeductions)}</Typography>
                  <Typography variant="caption" color="text.secondary">Total Deductions</Typography>
                </Card>
              </Grid>
              <Grid size={{ xs: 12, sm: 4 }}>
                <Card variant="outlined" sx={{ textAlign: 'center', py: 2 }}>
                  <CheckCircle sx={{ fontSize: 36, color: 'success.main', mb: 1 }} />
                  <Typography variant="h5" fontWeight={700} color="success.main">{formatCurrency(totalNet)}</Typography>
                  <Typography variant="caption" color="text.secondary">Net Payable</Typography>
                </Card>
              </Grid>
            </Grid>

            <Box sx={{ display: 'flex', justifyContent: 'center', gap: 2 }}>
              <Button variant="outlined" size="large" onClick={() => setActiveStep(1)} disabled={processing}>
                Back to Review
              </Button>
              <Button variant="contained" size="large" startIcon={<Send />} onClick={handleProcess} disabled={processing}>
                {processing ? 'Processing...' : 'Confirm & Process Payroll'}
              </Button>
            </Box>
          </CardContent>
        </Card>
      )}
    </>
  );
};
