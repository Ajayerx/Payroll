import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Box, Grid, Card, CardContent, Typography, Avatar, Button,
  Divider, CircularProgress, Tabs, Tab, Table, TableBody,
  TableCell, TableContainer, TableHead, TableRow, Chip,
  Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, MenuItem, Alert
} from '@mui/material';
import { ArrowBack, Edit, Add, Save, Close } from '@mui/icons-material';
import { toast } from 'react-toastify';
import { PageHeader } from '../Common/PageHeader';
import { StatusBadge } from '../Common/StatusBadge';
import { formatDate, formatCurrency } from '../../utils/formatters';
import { useEmployees } from '../../hooks/useEmployees';
import { salaryService } from '../../services/salaryService';

export const EmployeeProfile = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const { selected, loading, fetchEmployeeById, clearSelected } = useEmployees();
  const [tab, setTab] = useState(0);
  const [structure, setStructure] = useState(null);
  const [components, setComponents] = useState([]);
  const [structLoading, setStructLoading] = useState(false);
  const [editDialog, setEditDialog] = useState({ open: false, items: [] });
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (id) fetchEmployeeById(id);
    return () => clearSelected();
  }, [id, fetchEmployeeById, clearSelected]);

  useEffect(() => {
    if (id && tab === 1) {
      loadStructure();
    }
  }, [id, tab]);

  const loadStructure = async () => {
    setStructLoading(true);
    try {
      const [structRes, compRes] = await Promise.all([
        salaryService.getEmployeeStructure(id).catch(() => ({ data: null })),
        salaryService.getComponents().catch(() => ({ data: [] })),
      ]);
      setStructure(structRes.data);
      setComponents(compRes.data || []);
    } catch {
      toast.error('Failed to load salary structure');
    } finally {
      setStructLoading(false);
    }
  };

  const handleOpenEdit = () => {
    const existing = (structure || []).map(i => ({
      componentId: i.salaryComponentId,
      componentName: i.componentName,
      amount: i.amount,
      type: i.componentType,
    }));
    setEditDialog({ open: true, items: existing.length ? existing : [{ componentId: '', amount: 0 }] });
  };

  const handleAddRow = () => {
    setEditDialog(prev => ({ ...prev, items: [...prev.items, { componentId: '', amount: 0 }] }));
  };

  const handleRemoveRow = (index) => {
    setEditDialog(prev => ({
      ...prev,
      items: prev.items.filter((_, i) => i !== index),
    }));
  };

  const handleItemChange = (index, field, value) => {
    setEditDialog(prev => ({
      ...prev,
      items: prev.items.map((item, i) => i === index ? { ...item, [field]: value } : item),
    }));
  };

  const handleSaveStructure = async () => {
    setSaving(true);
    try {
      const data = {
        components: editDialog.items.map(item => ({
          salaryComponentId: item.componentId,
          amount: parseFloat(item.amount) || 0,
        })),
      };
      await salaryService.updateEmployeeStructure(id, data);
      toast.success('Salary structure updated');
      setEditDialog({ open: false, items: [] });
      loadStructure();
    } catch {
      toast.error('Failed to update salary structure');
    } finally {
      setSaving(false);
    }
  };

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

      <Card sx={{ mb: 3 }}>
        <Tabs value={tab} onChange={(_, v) => setTab(v)}>
          <Tab label="Personal Details" />
          <Tab label="Salary Structure" />
        </Tabs>
      </Card>

      {tab === 0 && (
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
      )}

      {tab === 1 && (
        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6" fontWeight={600}>Salary Components</Typography>
              <Button variant="contained" startIcon={<Add />} onClick={handleOpenEdit}>
                {structure?.length ? 'Edit' : 'Add'} Structure
              </Button>
            </Box>
            <Divider sx={{ mb: 2 }} />

            {structLoading ? (
              <Box sx={{ textAlign: 'center', py: 4 }}><CircularProgress /></Box>
            ) : !structure?.length ? (
              <Alert severity="info">No salary structure configured for this employee</Alert>
            ) : (
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell sx={{ fontWeight: 600 }}>Component</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Type</TableCell>
                      <TableCell align="right" sx={{ fontWeight: 600 }}>Amount</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {structure.map((item, i) => (
                      <TableRow key={item.salaryComponentId || i}>
                        <TableCell>{item.componentName}</TableCell>
                        <TableCell>
                          <Chip
                            label={item.componentType || 'Earning'}
                            size="small"
                            color={item.componentType === 'Deduction' ? 'error' : 'success'}
                            variant="outlined"
                          />
                        </TableCell>
                        <TableCell align="right">
                          <Typography fontWeight={500} color={item.componentType === 'Deduction' ? 'error.main' : 'text.primary'}>
                            {item.componentType === 'Deduction' ? `-${formatCurrency(item.amount)}` : formatCurrency(item.amount)}
                          </Typography>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </CardContent>
        </Card>
      )}

      <Dialog open={editDialog.open} onClose={() => setEditDialog({ open: false, items: [] })} maxWidth="sm" fullWidth>
        <DialogTitle>Edit Salary Structure</DialogTitle>
        <DialogContent>
          {editDialog.items.map((item, index) => (
            <Box key={index} sx={{ display: 'flex', gap: 1, alignItems: 'center', mt: 2 }}>
              <TextField
                select
                size="small"
                label="Component"
                value={item.componentId}
                onChange={(e) => handleItemChange(index, 'componentId', e.target.value)}
                sx={{ minWidth: 200 }}
              >
                {components.map(c => (
                  <MenuItem key={c.id} value={c.id}>{c.name} ({c.type})</MenuItem>
                ))}
              </TextField>
              <TextField
                size="small"
                label="Amount"
                type="number"
                value={item.amount}
                onChange={(e) => handleItemChange(index, 'amount', e.target.value)}
                sx={{ minWidth: 120 }}
              />
              <Button size="small" color="error" onClick={() => handleRemoveRow(index)}>
                <Close />
              </Button>
            </Box>
          ))}
          <Button startIcon={<Add />} onClick={handleAddRow} sx={{ mt: 1 }}>
            Add Component
          </Button>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditDialog({ open: false, items: [] })}>Cancel</Button>
          <Button variant="contained" startIcon={<Save />} onClick={handleSaveStructure} disabled={saving}>
            {saving ? 'Saving...' : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};
