import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, Card, CardContent, Typography, TextField, Button, Alert,
  InputAdornment, IconButton, CircularProgress, Select, MenuItem,
  FormControl, InputLabel, PersonAddOutlined
} from '@mui/material';
import { Visibility, VisibilityOff } from '@mui/icons-material';
import { toast } from 'react-toastify';
import { authService } from '../services/authService';

const RegisterPage = () => {
  const navigate = useNavigate();
  const [form, setForm] = useState({
    firstName: '', lastName: '', email: '', password: '', role: 'Employee'
  });
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);

  const handleChange = (field) => (e) => {
    setForm({ ...form, [field]: e.target.value });
    if (error) setError('');
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!form.firstName || !form.lastName || !form.email || !form.password) {
      setError('Please fill in all fields');
      return;
    }
    if (form.password.length < 6) {
      setError('Password must be at least 6 characters');
      return;
    }

    setLoading(true);
    try {
      await authService.register(form);
      setSuccess(true);
      toast.success('Account created successfully');
    } catch (err) {
      const msg = err.response?.data?.message || err.response?.data?.title || 'Registration failed';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <Box sx={{
        display: 'flex', alignItems: 'center', justifyContent: 'center',
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #004d40 0%, #00695c 40%, #00897b 100%)',
        position: 'relative', overflow: 'hidden'
      }}>
        <Box sx={{ position: 'absolute', top: -200, left: -200, width: 500, height: 500,
          borderRadius: '50%', background: 'radial-gradient(circle, rgba(255,255,255,0.06) 0%, transparent 70%)' }} />
        <Card sx={{
          maxWidth: 440, width: '100%', mx: 2, borderRadius: 4,
          boxShadow: '0 24px 80px rgba(0,0,0,0.35)', position: 'relative', zIndex: 1
        }}>
          <CardContent sx={{ p: { xs: 3, sm: 5 }, textAlign: 'center' }}>
            <Box sx={{
              width: 72, height: 72, borderRadius: '50%',
              background: 'linear-gradient(135deg, #00897b, #004d40)',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              mx: 'auto', mb: 2, boxShadow: '0 8px 24px rgba(0,137,123,0.3)'
            }}>
              <PersonAddOutlined sx={{ fontSize: 36, color: '#fff' }} />
            </Box>
            <Typography variant="h5" fontWeight={700} gutterBottom color="primary.main">
              Registration Successful
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 4 }}>
              The account has been created. You can now sign in.
            </Typography>
            <Button variant="contained" size="large"
              onClick={() => navigate('/login')}
              sx={{ py: 1.5, px: 4, borderRadius: 2, textTransform: 'none', fontSize: 16 }}>
              Sign In
            </Button>
          </CardContent>
        </Card>
      </Box>
    );
  }

  return (
    <Box sx={{
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      minHeight: '100vh',
      background: 'linear-gradient(135deg, #004d40 0%, #00695c 40%, #00897b 100%)',
      position: 'relative', overflow: 'hidden'
    }}>
      <Box sx={{ position: 'absolute', top: -200, left: -200, width: 500, height: 500,
        borderRadius: '50%', background: 'radial-gradient(circle, rgba(255,255,255,0.06) 0%, transparent 70%)' }} />
      <Box sx={{ position: 'absolute', bottom: -100, right: -100, width: 250, height: 250,
        borderRadius: '50%', background: 'radial-gradient(circle, rgba(255,255,255,0.04) 0%, transparent 70%)' }} />

      <Card sx={{
        maxWidth: 480, width: '100%', mx: 2, borderRadius: 4,
        boxShadow: '0 24px 80px rgba(0,0,0,0.35)', position: 'relative', zIndex: 1
      }}>
        <CardContent sx={{ p: { xs: 3, sm: 5 } }}>
          <Box sx={{ textAlign: 'center' }} mb={3}>
            <Box sx={{
              width: 64, height: 64, borderRadius: '50%',
              background: 'linear-gradient(135deg, #00897b, #004d40)',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              mx: 'auto', mb: 2, boxShadow: '0 8px 24px rgba(0,137,123,0.3)'
            }}>
              <PersonAddOutlined sx={{ fontSize: 32, color: '#fff' }} />
            </Box>
            <Typography variant="h4" fontWeight={700} color="primary.main" sx={{ letterSpacing: '-0.02em' }}>
              Create Account
            </Typography>
            <Typography variant="body2" color="text.secondary" mt={0.5}>
              Register a new user account
            </Typography>
          </Box>

          {error && (
            <Alert severity="error" sx={{ mb: 3, borderRadius: 2 }} onClose={() => setError('')}>
              {error}
            </Alert>
          )}

          <form onSubmit={handleSubmit}>
            <Box sx={{ display: 'flex', gap: 2 }}>
              <TextField fullWidth label="First Name" value={form.firstName}
                onChange={handleChange('firstName')} required autoFocus
                sx={{ '& .MuiOutlinedInput-root': { borderRadius: 2 } }} />
              <TextField fullWidth label="Last Name" value={form.lastName}
                onChange={handleChange('lastName')} required
                sx={{ '& .MuiOutlinedInput-root': { borderRadius: 2 } }} />
            </Box>
            <TextField fullWidth label="Email" type="email" value={form.email}
              onChange={handleChange('email')} required margin="normal"
              sx={{ '& .MuiOutlinedInput-root': { borderRadius: 2 } }} />
            <TextField fullWidth label="Password" type={showPassword ? 'text' : 'password'}
              value={form.password} onChange={handleChange('password')} required margin="normal"
              sx={{ '& .MuiOutlinedInput-root': { borderRadius: 2 } }}
              slotProps={{
                input: {
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton onClick={() => setShowPassword(!showPassword)} edge="end">
                        {showPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                },
              }} />
            <FormControl fullWidth margin="normal" sx={{ '& .MuiOutlinedInput-root': { borderRadius: 2 } }}>
              <InputLabel id="role-label">Role</InputLabel>
              <Select labelId="role-label" label="Role" value={form.role}
                onChange={handleChange('role')}>
                <MenuItem value="Employee">Employee</MenuItem>
                <MenuItem value="HRManager">HR Manager</MenuItem>
                <MenuItem value="Admin">Admin</MenuItem>
              </Select>
            </FormControl>
            <Button type="submit" fullWidth variant="contained" size="large"
              disabled={loading}
              sx={{ mt: 3, mb: 2, py: 1.5, borderRadius: 2, textTransform: 'none', fontSize: 16,
                background: 'linear-gradient(135deg, #004d40, #00897b)',
                '&:hover': { background: 'linear-gradient(135deg, #00695c, #26a69a)' }
              }}>
              {loading ? <CircularProgress size={22} sx={{ color: 'white' }} /> : 'Create Account'}
            </Button>
          </form>

          <Box sx={{ textAlign: 'center', mt: 1 }}>
            <Button variant="text" onClick={() => navigate('/login')} sx={{ textTransform: 'none' }}>
              Already have an account? Sign In
            </Button>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default RegisterPage;
