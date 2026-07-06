import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Box, Grid, TextField, Button, Card, CardContent, Typography,
  MenuItem, Alert, Stepper, Step, StepLabel, CircularProgress
} from '@mui/material';
import { Save, ArrowBack } from '@mui/icons-material';
import { toast } from 'react-toastify';
import { PageHeader } from '../Common/PageHeader';
import { useEmployees } from '../../hooks/useEmployees';

const departments = ['Engineering', 'Sales', 'HR', 'Finance', 'Operations', 'Marketing'];
const designations = ['Software Engineer', 'Senior Engineer', 'Lead', 'Manager', 'Director', 'Analyst', 'Coordinator'];
const genders = ['Male', 'Female', 'Other'];

const steps = ['Personal Information', 'Contact & Address', 'Banking Details', 'Review'];

export const EmployeeForm = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEdit = Boolean(id);
  const [activeStep, setActiveStep] = useState(0);
  const [form, setForm] = useState({
    firstName: '', lastName: '', email: '', phone: '', gender: '',
    dob: '', dateOfJoining: '', department: '', designation: '',
    address: '', city: '', state: '', pincode: '',
    bankName: '', bankAccount: '', ifscCode: '',
  });

  const { selected, loading, fetchEmployeeById, createEmployee, updateEmployee, clearSelected } = useEmployees();

  useEffect(() => {
    if (id) {
      fetchEmployeeById(id);
    }
    return () => clearSelected();
  }, [id, fetchEmployeeById, clearSelected]);

  useEffect(() => {
    if (isEdit && selected) {
      setForm({
        firstName: selected.firstName || '',
        lastName: selected.lastName || '',
        email: selected.email || '',
        phone: selected.phone || '',
        gender: selected.gender || '',
        dob: selected.dob ? selected.dob.split('T')[0] : '',
        dateOfJoining: selected.dateOfJoining ? selected.dateOfJoining.split('T')[0] : '',
        department: selected.department || '',
        designation: selected.designation || '',
        address: selected.address || '',
        city: selected.city || '',
        state: selected.state || '',
        pincode: selected.pincode || '',
        bankName: selected.bankName || '',
        bankAccount: selected.bankAccount || '',
        ifscCode: selected.ifscCode || '',
      });
    }
  }, [isEdit, selected]);

  const handleChange = (field) => (e) => setForm({ ...form, [field]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (activeStep < steps.length - 1) {
      setActiveStep(activeStep + 1);
      return;
    }
    try {
      if (isEdit) {
        await updateEmployee(id, form);
      } else {
        await createEmployee(form);
      }
      toast.success(isEdit ? 'Employee updated successfully' : 'Employee created successfully');
      navigate('/employees');
    } catch {
      toast.error('Failed to save employee');
    }
  };

  if (isEdit && loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <>
      <PageHeader
        title={isEdit ? 'Edit Employee' : 'Add New Employee'}
        subtitle={isEdit ? 'Update employee details' : 'Fill in the details to add a new employee'}
        breadcrumbs={[
          { label: 'Dashboard', onClick: () => navigate('/dashboard') },
          { label: 'Employees', onClick: () => navigate('/employees') },
          { label: isEdit ? 'Edit' : 'Add New' },
        ]}
        action={
          <Button variant="outlined" startIcon={<ArrowBack />} onClick={() => navigate('/employees')}>
            Back to List
          </Button>
        }
      />

      <Stepper activeStep={activeStep} alternativeLabel sx={{ mb: 4 }}>
        {steps.map(label => <Step key={label}><StepLabel>{label}</StepLabel></Step>)}
      </Stepper>

      <Card>
        <CardContent sx={{ p: 3 }}>
          <form onSubmit={handleSubmit}>
            <Grid container spacing={2.5}>
              {activeStep === 0 && (
                <>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField fullWidth label="First Name" value={form.firstName} onChange={handleChange('firstName')} required />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField fullWidth label="Last Name" value={form.lastName} onChange={handleChange('lastName')} required />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField fullWidth label="Email" type="email" value={form.email} onChange={handleChange('email')} required />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField fullWidth label="Phone" value={form.phone} onChange={handleChange('phone')} required />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 4 }}>
                    <TextField select fullWidth label="Gender" value={form.gender} onChange={handleChange('gender')}>
                      {genders.map(g => <MenuItem key={g} value={g}>{g}</MenuItem>)}
                    </TextField>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 4 }}>
                    <TextField fullWidth label="Date of Birth" type="date" value={form.dob} onChange={handleChange('dob')} slotProps={{ inputLabel: { shrink: true } }} />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 4 }}>
                    <TextField fullWidth label="Date of Joining" type="date" value={form.dateOfJoining} onChange={handleChange('dateOfJoining')} slotProps={{ inputLabel: { shrink: true } }} />
                  </Grid>
                </>
              )}

              {activeStep === 1 && (
                <>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField select fullWidth label="Department" value={form.department} onChange={handleChange('department')}>
                      {departments.map(d => <MenuItem key={d} value={d}>{d}</MenuItem>)}
                    </TextField>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField select fullWidth label="Designation" value={form.designation} onChange={handleChange('designation')}>
                      {designations.map(d => <MenuItem key={d} value={d}>{d}</MenuItem>)}
                    </TextField>
                  </Grid>
                  <Grid size={{ xs: 12 }}>
                    <TextField fullWidth label="Address" multiline rows={2} value={form.address} onChange={handleChange('address')} />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 4 }}>
                    <TextField fullWidth label="City" value={form.city} onChange={handleChange('city')} />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 4 }}>
                    <TextField fullWidth label="State" value={form.state} onChange={handleChange('state')} />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 4 }}>
                    <TextField fullWidth label="Pincode" value={form.pincode} onChange={handleChange('pincode')} />
                  </Grid>
                </>
              )}

              {activeStep === 2 && (
                <>
                  <Grid size={{ xs: 12, sm: 4 }}>
                    <TextField fullWidth label="Bank Name" value={form.bankName} onChange={handleChange('bankName')} />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 4 }}>
                    <TextField fullWidth label="Bank Account No." value={form.bankAccount} onChange={handleChange('bankAccount')} />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 4 }}>
                    <TextField fullWidth label="IFSC Code" value={form.ifscCode} onChange={handleChange('ifscCode')} />
                  </Grid>
                </>
              )}

              {activeStep === 3 && (
                <Grid size={{ xs: 12 }}>
                  <Alert severity="info" sx={{ mb: 2 }}>
                    Please review all the information before submitting
                  </Alert>
                  <Box sx={{ '& > div': { py: 0.5 } }}>
                    <Typography variant="body2"><strong>Name:</strong> {form.firstName} {form.lastName}</Typography>
                    <Typography variant="body2"><strong>Email:</strong> {form.email}</Typography>
                    <Typography variant="body2"><strong>Phone:</strong> {form.phone}</Typography>
                    <Typography variant="body2"><strong>Department:</strong> {form.department}</Typography>
                    <Typography variant="body2"><strong>Designation:</strong> {form.designation}</Typography>
                    <Typography variant="body2"><strong>Bank:</strong> {form.bankName} - {form.bankAccount}</Typography>
                  </Box>
                </Grid>
              )}
            </Grid>

            <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 4 }}>
              <Button variant="outlined" disabled={activeStep === 0} onClick={() => setActiveStep(activeStep - 1)}>
                Previous
              </Button>
              <Button type="submit" variant="contained" startIcon={<Save />}>
                {activeStep < steps.length - 1 ? 'Next' : (isEdit ? 'Update Employee' : 'Create Employee')}
              </Button>
            </Box>
          </form>
        </CardContent>
      </Card>
    </>
  );
};
