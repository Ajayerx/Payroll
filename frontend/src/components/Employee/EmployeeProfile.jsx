import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Box, Grid, Card, CardContent, Typography, Avatar, Button, Divider, CircularProgress
} from '@mui/material';
import { ArrowBack, Edit } from '@mui/icons-material';
import { PageHeader } from '../Common/PageHeader';
import { StatusBadge } from '../Common/StatusBadge';
import { formatDate } from '../../utils/formatters';
import { useEmployees } from '../../hooks/useEmployees';

export const EmployeeProfile = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const { selected, loading, fetchEmployeeById, clearSelected } = useEmployees();

  useEffect(() => {
    if (id) fetchEmployeeById(id);
    return () => clearSelected();
  }, [id, fetchEmployeeById, clearSelected]);

  if (loading) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  }

  if (!selected) {
    return (
      <Box sx={{ textAlign: 'center', py: 8 }}>
        <Typography variant="h5" color="text.secondary">Employee not found</Typography>
        <Button variant="contained" sx={{ mt: 2 }} onClick={() => navigate('/employees')}>Back to Employees</Button>
      </Box>
    );
  }

  return (
    <>
      <PageHeader
        title="Employee Profile"
        subtitle={`${selected.firstName} ${selected.lastName} - ${selected.employeeCode}`}
        breadcrumbs={[
          { label: 'Dashboard', onClick: () => navigate('/dashboard') },
          { label: 'Employees', onClick: () => navigate('/employees') },
          { label: selected.firstName },
        ]}
        action={
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button variant="outlined" startIcon={<ArrowBack />} onClick={() => navigate('/employees')}>Back</Button>
            <Button variant="contained" startIcon={<Edit />} onClick={() => navigate(`/employees/${id}/edit`)}>Edit</Button>
          </Box>
        }
      />

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 4 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center', py: 4 }}>
              <Avatar sx={{ width: 100, height: 100, mx: 'auto', mb: 2, bgcolor: 'primary.main', fontSize: 36 }}>
                {selected.firstName?.[0]}{selected.lastName?.[0]}
              </Avatar>
              <Typography variant="h5" fontWeight={600}>{selected.firstName} {selected.lastName}</Typography>
              <Typography variant="body2" color="text.secondary">{selected.designation}</Typography>
              <Box mt={1}><StatusBadge status={selected.isActive ? 'Active' : 'Inactive'} /></Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 8 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" fontWeight={600} gutterBottom>Personal Details</Typography>
              <Divider sx={{ mb: 2 }} />
              <Grid container spacing={2}>
                {[
                  ['Employee Code', selected.employeeCode],
                  ['Email', selected.email],
                  ['Phone', selected.phone],
                  ['Gender', selected.gender],
                  ['Date of Birth', formatDate(selected.dob)],
                  ['Date of Joining', formatDate(selected.dateOfJoining)],
                  ['Department', selected.department],
                  ['Designation', selected.designation],
                  ['Address', `${selected.address ?? ''}, ${selected.city ?? ''}, ${selected.state ?? ''} - ${selected.pincode ?? ''}`],
                  ['Bank', `${selected.bankName ?? ''} - ${selected.bankAccount ?? ''}`],
                  ['IFSC', selected.ifscCode],
                ].map(([label, value]) => (
                  <Grid size={{ xs: 12, sm: 6 }} key={label}>
                    <Typography variant="caption" color="text.secondary">{label}</Typography>
                    <Typography variant="body2">{value || '-'}</Typography>
                  </Grid>
                ))}
              </Grid>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </>
  );
};
