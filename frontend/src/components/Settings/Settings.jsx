import { useState, useEffect, useCallback } from 'react';
import {
  Box, Grid, Card, CardContent, Typography, TextField, Button,
  Tabs, Tab, Divider, List, ListItem,
  ListItemText, IconButton, Chip, Dialog, DialogTitle,
  DialogContent, DialogActions, Switch, FormControlLabel
} from '@mui/material';
import { Save, Add, Delete, Edit, Close } from '@mui/icons-material';
import { toast } from 'react-toastify';
import { PageHeader } from '../Common/PageHeader';
import { LoadingSpinner } from '../Common/Loading';
import { settingsService } from '../../services/settingsService';
import { salaryService } from '../../services/salaryService';

const settingsTabs = [
  { label: 'Company Profile' },
  { label: 'Tax Slabs' },
  { label: 'Leave Types' },
  { label: 'Salary Components' },
];

const defaultDialog = { open: false, editItem: null, type: '' };

export const Settings = () => {
  const [tab, setTab] = useState(0);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  const [company, setCompany] = useState({
    companyName: '', address: '', email: '', phone: '', gstin: '', pan: '', logoUrl: ''
  });
  const [taxSlabs, setTaxSlabs] = useState([]);
  const [leaveTypes, setLeaveTypes] = useState([]);
  const [salaryComponents, setSalaryComponents] = useState([]);
  const [dialog, setDialog] = useState(defaultDialog);
  const [dialogForm, setDialogForm] = useState({});

  useEffect(() => {
    Promise.all([
      settingsService.getCompany(),
      settingsService.getTaxSlabs(),
      settingsService.getLeaveTypes(),
      salaryService.getComponents()
    ]).then(([comp, slabs, leaves, components]) => {
      if (comp.data) setCompany(comp.data);
      setTaxSlabs(slabs.data || []);
      setLeaveTypes(leaves.data || []);
      setSalaryComponents(components.data || []);
    }).catch(() => toast.error('Failed to load settings'))
    .finally(() => setLoading(false));
  }, []);

  const handleCompanySave = async () => {
    setSaving(true);
    try {
      const res = await settingsService.updateCompany(company);
      setCompany(res.data);
      toast.success('Company settings saved');
    } catch { toast.error('Failed to save company settings'); }
    finally { setSaving(false); }
  };

  const openDialog = (type, item = null) => {
    setDialog({ open: true, type, editItem: item });
    if (item) setDialogForm({ ...item });
    else setDialogForm({});
  };

  const handleDialogSave = async () => {
    try {
      if (dialog.type === 'tax-slab') {
        if (dialog.editItem) {
          toast.info('Edit tax slab coming soon');
        } else {
          const res = await settingsService.createTaxSlab({
            name: dialogForm.name,
            fromAmount: Number(dialogForm.fromAmount),
            toAmount: dialogForm.toAmount ? Number(dialogForm.toAmount) : null,
            rate: Number(dialogForm.rate)
          });
          setTaxSlabs(prev => [...prev, res.data]);
          toast.success('Tax slab created');
        }
      } else if (dialog.type === 'leave-type') {
        if (dialog.editItem) {
          toast.info('Edit leave type coming soon');
        } else {
          const res = await settingsService.createLeaveType({
            name: dialogForm.name,
            daysPerYear: dialogForm.daysPerYear ? Number(dialogForm.daysPerYear) : null,
            isPaid: dialogForm.isPaid !== false
          });
          setLeaveTypes(prev => [...prev, res.data]);
          toast.success('Leave type created');
        }
      } else if (dialog.type === 'salary-component') {
        if (dialog.editItem) {
          const res = await salaryService.updateComponent(dialog.editItem.id, {
            name: dialogForm.name,
            type: dialogForm.type,
            isVariable: dialogForm.isVariable || false,
            description: dialogForm.description
          });
          setSalaryComponents(prev => prev.map(c => c.id === res.data.id ? res.data : c));
          toast.success('Component updated');
        } else {
          const res = await salaryService.createComponent({
            name: dialogForm.name,
            type: dialogForm.type || 'Earning',
            isVariable: dialogForm.isVariable || false,
            description: dialogForm.description
          });
          setSalaryComponents(prev => [...prev, res.data]);
          toast.success('Component created');
        }
      }
      setDialog(defaultDialog);
    } catch { toast.error('Operation failed'); }
  };

  if (loading) return <LoadingSpinner message="Loading settings..." />;

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
                <TextField fullWidth label="Company Name" value={company.companyName || ''}
                  onChange={(e) => setCompany({ ...company, companyName: e.target.value })} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField fullWidth label="Email" value={company.email || ''}
                  onChange={(e) => setCompany({ ...company, email: e.target.value })} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField fullWidth label="Phone" value={company.phone || ''}
                  onChange={(e) => setCompany({ ...company, phone: e.target.value })} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField fullWidth label="GSTIN" value={company.gstin || ''}
                  onChange={(e) => setCompany({ ...company, gstin: e.target.value })} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField fullWidth label="PAN" value={company.pan || ''}
                  onChange={(e) => setCompany({ ...company, pan: e.target.value })} />
              </Grid>
              <Grid size={{ xs: 12 }}>
                <TextField fullWidth label="Address" multiline rows={2} value={company.address || ''}
                  onChange={(e) => setCompany({ ...company, address: e.target.value })} />
              </Grid>
            </Grid>
            <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 3 }}>
              <Button variant="contained" startIcon={<Save />} onClick={handleCompanySave} disabled={saving}>
                {saving ? 'Saving...' : 'Save Changes'}
              </Button>
            </Box>
          </CardContent>
        </Card>
      )}

      {tab === 1 && (
        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6" fontWeight={600}>Tax Slabs</Typography>
              <Button variant="contained" size="small" startIcon={<Add />}
                onClick={() => openDialog('tax-slab')}>Add Slab</Button>
            </Box>
            <Divider sx={{ mb: 2 }} />
            <List>
              {taxSlabs.map(slab => (
                <ListItem key={slab.id} divider>
                  <ListItemText
                    primary={slab.name}
                    secondary={`₹${(slab.fromAmount || 0).toLocaleString()} - ${slab.toAmount ? `₹${slab.toAmount.toLocaleString()}` : 'Above'} @ ${slab.rate}%`}
                  />
                </ListItem>
              ))}
              {taxSlabs.length === 0 && (
                <ListItem><ListItemText primary="No tax slabs configured" /></ListItem>
              )}
            </List>
          </CardContent>
        </Card>
      )}

      {tab === 2 && (
        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6" fontWeight={600}>Leave Types</Typography>
              <Button variant="contained" size="small" startIcon={<Add />}
                onClick={() => openDialog('leave-type')}>Add Leave Type</Button>
            </Box>
            <Divider sx={{ mb: 2 }} />
            <List>
              {leaveTypes.map(leave => (
                <ListItem key={leave.id} divider>
                  <ListItemText
                    primary={leave.name}
                    secondary={`${leave.daysPerYear || 'Unlimited'} days per year`}
                  />
                  <Chip label={leave.isPaid ? 'Paid' : 'Unpaid'} color={leave.isPaid ? 'success' : 'default'} size="small" sx={{ mr: 1 }} />
                </ListItem>
              ))}
              {leaveTypes.length === 0 && (
                <ListItem><ListItemText primary="No leave types configured" /></ListItem>
              )}
            </List>
          </CardContent>
        </Card>
      )}

      {tab === 3 && (
        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6" fontWeight={600}>Salary Components</Typography>
              <Button variant="contained" size="small" startIcon={<Add />}
                onClick={() => openDialog('salary-component')}>Add Component</Button>
            </Box>
            <Divider sx={{ mb: 2 }} />
            <List>
              {salaryComponents.map(comp => (
                <ListItem key={comp.id} divider
                  secondaryAction={
                    <IconButton edge="end" size="small" onClick={() => openDialog('salary-component', comp)}>
                      <Edit fontSize="small" />
                    </IconButton>
                  }>
                  <ListItemText
                    primary={comp.name}
                    secondary={`Type: ${comp.type} | ${comp.isVariable ? 'Variable' : 'Fixed'}`}
                  />
                  <Chip label={comp.type} color={comp.type === 'Earning' ? 'success' : 'error'} size="small" sx={{ mr: 4 }} />
                </ListItem>
              ))}
              {salaryComponents.length === 0 && (
                <ListItem><ListItemText primary="No salary components configured" /></ListItem>
              )}
            </List>
          </CardContent>
        </Card>
      )}

      <Dialog open={dialog.open} onClose={() => setDialog(defaultDialog)} maxWidth="sm" fullWidth>
        <DialogTitle>
          {dialog.editItem ? 'Edit' : 'Add'} {
            dialog.type === 'tax-slab' ? 'Tax Slab' :
            dialog.type === 'leave-type' ? 'Leave Type' : 'Salary Component'
          }
          <IconButton onClick={() => setDialog(defaultDialog)} sx={{ position: 'absolute', right: 8, top: 8 }}>
            <Close />
          </IconButton>
        </DialogTitle>
        <DialogContent>
          {dialog.type === 'tax-slab' && (
            <Grid container spacing={2} sx={{ mt: 0.5 }}>
              <Grid size={{ xs: 12 }}><TextField fullWidth label="Name" value={dialogForm.name || ''}
                onChange={(e) => setDialogForm({ ...dialogForm, name: e.target.value })} /></Grid>
              <Grid size={{ xs: 6 }}><TextField fullWidth label="From Amount (₹)" type="number" value={dialogForm.fromAmount || ''}
                onChange={(e) => setDialogForm({ ...dialogForm, fromAmount: e.target.value })} /></Grid>
              <Grid size={{ xs: 6 }}><TextField fullWidth label="To Amount (₹)" type="number" value={dialogForm.toAmount || ''}
                onChange={(e) => setDialogForm({ ...dialogForm, toAmount: e.target.value })} /></Grid>
              <Grid size={{ xs: 12 }}><TextField fullWidth label="Rate (%)" type="number" value={dialogForm.rate || ''}
                onChange={(e) => setDialogForm({ ...dialogForm, rate: e.target.value })} /></Grid>
            </Grid>
          )}
          {dialog.type === 'leave-type' && (
            <Grid container spacing={2} sx={{ mt: 0.5 }}>
              <Grid size={{ xs: 12 }}><TextField fullWidth label="Leave Type Name" value={dialogForm.name || ''}
                onChange={(e) => setDialogForm({ ...dialogForm, name: e.target.value })} /></Grid>
              <Grid size={{ xs: 6 }}><TextField fullWidth label="Days Per Year" type="number" value={dialogForm.daysPerYear || ''}
                onChange={(e) => setDialogForm({ ...dialogForm, daysPerYear: e.target.value })} /></Grid>
              <Grid size={{ xs: 6 }}>
                <FormControlLabel control={<Switch checked={dialogForm.isPaid !== false}
                  onChange={(e) => setDialogForm({ ...dialogForm, isPaid: e.target.checked })} />} label="Paid Leave" />
              </Grid>
            </Grid>
          )}
          {dialog.type === 'salary-component' && (
            <Grid container spacing={2} sx={{ mt: 0.5 }}>
              <Grid size={{ xs: 12 }}><TextField fullWidth label="Component Name" value={dialogForm.name || ''}
                onChange={(e) => setDialogForm({ ...dialogForm, name: e.target.value })} /></Grid>
              <Grid size={{ xs: 6 }}>
                <TextField select fullWidth label="Type" value={dialogForm.type || 'Earning'}
                  onChange={(e) => setDialogForm({ ...dialogForm, type: e.target.value })}
                  slotProps={{ native: true }}>
                  <option value="Earning">Earning</option>
                  <option value="Deduction">Deduction</option>
                </TextField>
              </Grid>
              <Grid size={{ xs: 6 }}>
                <FormControlLabel control={<Switch checked={dialogForm.isVariable || false}
                  onChange={(e) => setDialogForm({ ...dialogForm, isVariable: e.target.checked })} />} label="Variable" />
              </Grid>
              <Grid size={{ xs: 12 }}><TextField fullWidth label="Description" multiline rows={2} value={dialogForm.description || ''}
                onChange={(e) => setDialogForm({ ...dialogForm, description: e.target.value })} /></Grid>
            </Grid>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialog(defaultDialog)}>Cancel</Button>
          <Button variant="contained" onClick={handleDialogSave}>Save</Button>
        </DialogActions>
      </Dialog>
    </>
  );
};
