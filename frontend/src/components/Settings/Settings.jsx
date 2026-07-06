import { useState } from 'react';
import {
  Box, Grid, Card, CardContent, Typography, TextField, Button,
  Tabs, Tab, Divider, List, ListItem,
  ListItemText, IconButton, Chip
} from '@mui/material';
import { Save, Add, Delete, Edit } from '@mui/icons-material';
import { toast } from 'react-toastify';
import { PageHeader } from '../Common/PageHeader';

const settingsTabs = [
  { label: 'Company Profile' },
  { label: 'Tax Slabs' },
  { label: 'Leave Types' },
  { label: 'Salary Components' },
];

export const Settings = () => {
  const [tab, setTab] = useState(0);
  const [company, setCompany] = useState({
    name: 'TechCorp Solutions Pvt Ltd',
    address: '456, Brigade Road, Bangalore - 560001',
    email: 'hr@techcorp.com',
    phone: '080-45678901',
    gstin: '29ABCDE1234F1Z5',
    pan: 'ABCDE1234F',
  });

  const mId = (i) => `550e8400-e29b-41d4-a716-44665544${String(i).padStart(4, '0')}`;

  const taxSlabs = [
    { id: mId(1), from: 0, to: 250000, rate: 0, name: 'Nil Slab' },
    { id: mId(2), from: 250001, to: 500000, rate: 5, name: '5% Slab' },
    { id: mId(3), from: 500001, to: 1000000, rate: 20, name: '20% Slab' },
    { id: mId(4), from: 1000001, to: null, rate: 30, name: '30% Slab' },
  ];

  const leaveTypes = [
    { id: mId(1), name: 'Annual Leave', days: 18, paid: true },
    { id: mId(2), name: 'Sick Leave', days: 12, paid: true },
    { id: mId(3), name: 'Casual Leave', days: 10, paid: true },
    { id: mId(4), name: 'Maternity Leave', days: 180, paid: true },
    { id: mId(5), name: 'Loss of Pay', days: null, paid: false },
  ];

  const salaryComponents = [
    { id: mId(1), name: 'Basic Salary', type: 'Earning', variable: false },
    { id: mId(2), name: 'House Rent Allowance', type: 'Earning', variable: false },
    { id: mId(3), name: 'Dearness Allowance', type: 'Earning', variable: false },
    { id: mId(4), name: 'Performance Bonus', type: 'Earning', variable: true },
    { id: mId(5), name: 'Provident Fund', type: 'Deduction', variable: false },
    { id: mId(6), name: 'Professional Tax', type: 'Deduction', variable: false },
    { id: mId(7), name: 'Income Tax', type: 'Deduction', variable: false },
  ];

  const handleCompanySave = () => {
    toast.success('Company settings updated successfully');
  };

  return (
    <>
      <PageHeader
        title="Settings"
        subtitle="Configure company settings and payroll rules"
        breadcrumbs={[{ label: 'Dashboard', onClick: () => {} }, { label: 'Settings' }]}
      />

      <Tabs value={tab} onChange={(e, v) => setTab(v)} sx={{ mb: 3 }}>
        {settingsTabs.map(t => <Tab key={t.label} label={t.label} />)}
      </Tabs>

      {tab === 0 && (
        <Card>
          <CardContent>
            <Typography variant="h6" fontWeight={600} gutterBottom>Company Information</Typography>
            <Divider sx={{ mb: 3 }} />
            <Grid container spacing={2.5}>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField fullWidth label="Company Name" value={company.name} onChange={(e) => setCompany({ ...company, name: e.target.value })} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField fullWidth label="Email" value={company.email} onChange={(e) => setCompany({ ...company, email: e.target.value })} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField fullWidth label="Phone" value={company.phone} onChange={(e) => setCompany({ ...company, phone: e.target.value })} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField fullWidth label="GSTIN" value={company.gstin} onChange={(e) => setCompany({ ...company, gstin: e.target.value })} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField fullWidth label="PAN" value={company.pan} onChange={(e) => setCompany({ ...company, pan: e.target.value })} />
              </Grid>
              <Grid size={{ xs: 12 }}>
                <TextField fullWidth label="Address" multiline rows={2} value={company.address} onChange={(e) => setCompany({ ...company, address: e.target.value })} />
              </Grid>
            </Grid>
            <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 3 }}>
              <Button variant="contained" startIcon={<Save />} onClick={handleCompanySave}>Save Changes</Button>
            </Box>
          </CardContent>
        </Card>
      )}

      {tab === 1 && (
        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6" fontWeight={600}>Tax Slabs</Typography>
              <Button variant="contained" size="small" startIcon={<Add />}>Add Slab</Button>
            </Box>
            <Divider sx={{ mb: 2 }} />
            <List>
              {taxSlabs.map(slab => (
                <ListItem key={slab.id} divider>
                  <ListItemText
                    primary={slab.name}
                    secondary={`₹${slab.from?.toLocaleString() || 0} - ${slab.to ? `₹${slab.to.toLocaleString()}` : 'Above'} @ ${slab.rate}%`}
                  />
                  <IconButton size="small"><Edit fontSize="small" /></IconButton>
                  <IconButton size="small" color="error"><Delete fontSize="small" /></IconButton>
                </ListItem>
              ))}
            </List>
          </CardContent>
        </Card>
      )}

      {tab === 2 && (
        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6" fontWeight={600}>Leave Types</Typography>
              <Button variant="contained" size="small" startIcon={<Add />}>Add Leave Type</Button>
            </Box>
            <Divider sx={{ mb: 2 }} />
            <List>
              {leaveTypes.map(leave => (
                <ListItem key={leave.id} divider>
                  <ListItemText
                    primary={leave.name}
                    secondary={`${leave.days || 'Unlimited'} days per year - ${leave.paid ? 'Paid' : 'Unpaid'}`}
                  />
                  <Chip label={leave.paid ? 'Paid' : 'Unpaid'} color={leave.paid ? 'success' : 'default'} size="small" sx={{ mr: 1 }} />
                  <IconButton size="small"><Edit fontSize="small" /></IconButton>
                  <IconButton size="small" color="error"><Delete fontSize="small" /></IconButton>
                </ListItem>
              ))}
            </List>
          </CardContent>
        </Card>
      )}

      {tab === 3 && (
        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6" fontWeight={600}>Salary Components</Typography>
              <Button variant="contained" size="small" startIcon={<Add />}>Add Component</Button>
            </Box>
            <Divider sx={{ mb: 2 }} />
            <List>
              {salaryComponents.map(comp => (
                <ListItem key={comp.id} divider>
                  <ListItemText
                    primary={comp.name}
                    secondary={`Type: ${comp.type} | ${comp.variable ? 'Variable' : 'Fixed'}`}
                  />
                  <Chip label={comp.type} color={comp.type === 'Earning' ? 'success' : 'error'} size="small" sx={{ mr: 1 }} />
                  <IconButton size="small"><Edit fontSize="small" /></IconButton>
                  <IconButton size="small" color="error"><Delete fontSize="small" /></IconButton>
                </ListItem>
              ))}
            </List>
          </CardContent>
        </Card>
      )}
    </>
  );
};
