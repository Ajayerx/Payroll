import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, Card, CardContent, Typography, TextField, Button, Alert, Divider
} from '@mui/material';
import { Save, ArrowBack } from '@mui/icons-material';
import { toast } from 'react-toastify';
import { PageHeader } from '../components/Common/PageHeader';
import { authService } from '../services/authService';

const ChangePasswordPage = () => {
  const navigate = useNavigate();
  const [form, setForm] = useState({ currentPassword: '', newPassword: '', confirmPassword: '' });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (form.newPassword !== form.confirmPassword) {
      setError('Passwords do not match');
      return;
    }
    if (form.newPassword.length < 8) {
      setError('Password must be at least 8 characters');
      return;
    }

    setLoading(true);
    try {
      await authService.changePassword({
        currentPassword: form.currentPassword,
        newPassword: form.newPassword
      });
      toast.success('Password changed successfully');
      navigate('/dashboard');
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to change password');
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageHeader
        title="Change Password"
        subtitle="Update your account password"
        breadcrumbs={[{ label: 'Dashboard', onClick: () => navigate('/dashboard') }, { label: 'Change Password' }]}
        action={
          <Button variant="outlined" startIcon={<ArrowBack />} onClick={() => navigate(-1)}>
            Back
          </Button>
        }
      />

      <Box sx={{ maxWidth: 500, mx: 'auto' }}>
        <Card>
          <CardContent sx={{ p: 3 }}>
            <Typography variant="h6" fontWeight={600} gutterBottom>Change Password</Typography>
            <Divider sx={{ mb: 3 }} />

            {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

            <form onSubmit={handleSubmit}>
              <TextField
                fullWidth label="Current Password" type="password" required
                value={form.currentPassword}
                onChange={(e) => setForm({ ...form, currentPassword: e.target.value })}
                sx={{ mb: 2 }}
              />
              <TextField
                fullWidth label="New Password" type="password" required
                value={form.newPassword}
                onChange={(e) => setForm({ ...form, newPassword: e.target.value })}
                helperText="Min 8 chars, at least 1 uppercase, 1 lowercase, 1 digit, 1 special char"
                sx={{ mb: 2 }}
              />
              <TextField
                fullWidth label="Confirm New Password" type="password" required
                value={form.confirmPassword}
                onChange={(e) => setForm({ ...form, confirmPassword: e.target.value })}
                sx={{ mb: 3 }}
              />
              <Button
                type="submit" variant="contained" fullWidth size="large"
                startIcon={<Save />} disabled={loading}
              >
                {loading ? 'Changing...' : 'Change Password'}
              </Button>
            </form>
          </CardContent>
        </Card>
      </Box>
    </>
  );
};

export default ChangePasswordPage;
