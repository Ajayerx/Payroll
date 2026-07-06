import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, Grid, Card, CardContent, TextField, Button,
  MenuItem, Alert, CircularProgress
} from '@mui/material';
import { Save, ArrowBack } from '@mui/icons-material';
import { toast } from 'react-toastify';
import { PageHeader } from '../Common/PageHeader';
import { leaveService } from '../../services/leaveService';
import { useAuth } from '../../hooks/useAuth';

export const LeaveForm = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [leaveTypes, setLeaveTypes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [form, setForm] = useState({
    leaveTypeId: '',
    fromDate: '',
    toDate: '',
    reason: '',
  });
  const [errors, setErrors] = useState({});

  useEffect(() => {
    leaveService.getLeaveTypes()
      .then(res => setLeaveTypes(res.data || []))
      .catch(() => toast.error('Failed to load leave types'))
      .finally(() => setLoading(false));
  }, []);

  const handleChange = (field) => (e) => {
    const value = e.target.value;
    setForm(prev => {
      const updated = { ...prev, [field]: value };
      if (updated.fromDate && updated.toDate) {
        const from = new Date(updated.fromDate);
        const to = new Date(updated.toDate);
        if (to >= from) {
          updated.totalDays = Math.ceil((to - from) / (1000 * 60 * 60 * 24)) + 1;
        }
      }
      return updated;
    });
    setErrors(prev => ({ ...prev, [field]: '' }));
  };

  const validate = () => {
    const err = {};
    if (!form.leaveTypeId) err.leaveTypeId = 'Select leave type';
    if (!form.fromDate) err.fromDate = 'Select from date';
    if (!form.toDate) err.toDate = 'Select to date';
    if (form.fromDate && form.toDate && new Date(form.toDate) < new Date(form.fromDate)) {
      err.toDate = 'To date must be after from date';
    }
    if (!form.reason) err.reason = 'Provide a reason';
    setErrors(err);
    return Object.keys(err).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validate()) return;
    setSubmitting(true);
    try {
      await leaveService.create({
        employeeId: user?.employeeId,
        leaveTypeId: form.leaveTypeId,
        fromDate: form.fromDate,
        toDate: form.toDate,
        reason: form.reason,
      });
      toast.success('Leave request submitted successfully');
      navigate('/leaves');
    } catch {
      toast.error('Failed to submit leave request');
    } finally {
      setSubmitting(false);
    }
  };

  const selectedLeave = leaveTypes.find(l => l.id === form.leaveTypeId);
  const totalDays = form.fromDate && form.toDate
    ? Math.ceil((new Date(form.toDate) - new Date(form.fromDate)) / (1000 * 60 * 60 * 24)) + 1
    : 0;

  if (loading) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  }

  return (
    <>
      <PageHeader
        title="Apply for Leave"
        subtitle="Submit a leave request for approval"
        breadcrumbs={[
          { label: 'Dashboard', onClick: () => navigate('/dashboard') },
          { label: 'Leaves', onClick: () => navigate('/leaves') },
          { label: 'Apply' },
        ]}
        action={
          <Button variant="outlined" startIcon={<ArrowBack />} disabled={submitting} onClick={() => navigate('/leaves')}>
            Back to Leaves
          </Button>
        }
      />

      <Card>
        <CardContent sx={{ p: 3 }}>
          <form onSubmit={handleSubmit}>
            <Grid container spacing={2.5}>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  select fullWidth label="Leave Type"
                  value={form.leaveTypeId}
                  onChange={handleChange('leaveTypeId')}
                  required
                  error={Boolean(errors.leaveTypeId)}
                  helperText={errors.leaveTypeId}
                >
                  <MenuItem value="">Select leave type</MenuItem>
                  {leaveTypes.map(lt => (
                    <MenuItem key={lt.id} value={lt.id}>
                      {lt.name} {lt.daysPerYear ? `(Max ${lt.daysPerYear} days)` : '(Unlimited)'}
                    </MenuItem>
                  ))}
                </TextField>
              </Grid>
              <Grid size={{ xs: 12, sm: 3 }}>
                <TextField
                  fullWidth label="From Date" type="date"
                  value={form.fromDate} onChange={handleChange('fromDate')}
                  slotProps={{ inputLabel: { shrink: true } }} required
                  error={Boolean(errors.fromDate)}
                  helperText={errors.fromDate}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 3 }}>
                <TextField
                  fullWidth label="To Date" type="date"
                  value={form.toDate} onChange={handleChange('toDate')}
                  slotProps={{ inputLabel: { shrink: true } }} required
                  error={Boolean(errors.toDate)}
                  helperText={errors.toDate}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  fullWidth label="Reason" multiline rows={3}
                  value={form.reason} onChange={handleChange('reason')}
                  required error={Boolean(errors.reason)}
                  helperText={errors.reason}
                />
              </Grid>
              {totalDays > 0 && (
                <Grid size={{ xs: 12 }}>
                  <Alert severity="info" sx={{ borderRadius: 2 }}>
                    Total: <strong>{totalDays} day{totalDays > 1 ? 's' : ''}</strong>
                    {selectedLeave?.daysPerYear && totalDays > selectedLeave.daysPerYear && (
                      <span> — <strong style={{ color: '#d32f2f' }}>Exceeds maximum of {selectedLeave.daysPerYear} days</strong></span>
                    )}
                  </Alert>
                </Grid>
              )}
            </Grid>

            <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1.5, mt: 3 }}>
              <Button variant="outlined" disabled={submitting} onClick={() => navigate('/leaves')}>Cancel</Button>
              <Button type="submit" variant="contained" startIcon={<Save />} disabled={submitting}>
                {submitting ? 'Submitting...' : 'Submit Request'}
              </Button>
            </Box>
          </form>
        </CardContent>
      </Card>
    </>
  );
};
