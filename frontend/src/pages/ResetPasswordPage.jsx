import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import {
  Box, Card, CardContent, Typography, TextField, Button, Alert,
  InputAdornment, IconButton, CircularProgress, CheckCircleOutlineOutlined
} from '@mui/material';
import { Visibility, VisibilityOff, LockResetOutlined } from '@mui/icons-material';
import { toast } from 'react-toastify';
import { authService } from '../services/authService';

const ResetPasswordPage = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');

  const [form, setForm] = useState({ newPassword: '', confirmPassword: '' });
  const [showPassword, setShowPassword] = useState({ pwd: false, confirm: false });
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!token) {
      setError('Invalid or missing reset token. Please request a new password reset link.');
    }
  }, [token]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (form.newPassword.length < 6) {
      setError('Password must be at least 6 characters');
      return;
    }
    if (form.newPassword !== form.confirmPassword) {
      setError('Passwords do not match');
      return;
    }

    setLoading(true);
    try {
      await authService.resetPassword({ token, newPassword: form.newPassword });
      setSuccess(true);
      toast.success('Password has been reset successfully');
    } catch (err) {
      const msg = err.response?.data?.message || err.response?.data?.title || 'Failed to reset password';
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
        background: 'linear-gradient(135deg, #1a237e 0%, #283593 40%, #1565c0 100%)',
        position: 'relative', overflow: 'hidden'
      }}>
        <Box sx={{ position: 'absolute', top: -200, right: -200, width: 400, height: 400,
          borderRadius: '50%', background: 'radial-gradient(circle, rgba(255,255,255,0.06) 0%, transparent 70%)' }} />
        <Box sx={{ position: 'absolute', bottom: -150, left: -150, width: 300, height: 300,
          borderRadius: '50%', background: 'radial-gradient(circle, rgba(255,255,255,0.04) 0%, transparent 70%)' }} />
        <Card sx={{
          maxWidth: 440, width: '100%', mx: 2, borderRadius: 4,
          boxShadow: '0 24px 80px rgba(0,0,0,0.35)',
          position: 'relative', zIndex: 1,
          backdropFilter: 'blur(4px)',
          border: '1px solid rgba(255,255,255,0.1)'
        }}>
          <CardContent sx={{ p: { xs: 3, sm: 5 }, textAlign: 'center' }}>
            <CheckCircleOutlineOutlined sx={{ fontSize: 72, color: 'success.main', mb: 2 }} />
            <Typography variant="h5" fontWeight={700} gutterBottom color="primary.main">
              Password Reset Successful
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 4, maxWidth: 320, mx: 'auto' }}>
              Your password has been reset successfully. You can now sign in with your new password.
            </Typography>
            <Button
              variant="contained" size="large"
              onClick={() => navigate('/login')}
              sx={{ py: 1.5, px: 4, borderRadius: 2, textTransform: 'none', fontSize: 16 }}
            >
              Sign In Now
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
      background: 'linear-gradient(135deg, #1a237e 0%, #283593 40%, #1565c0 100%)',
      position: 'relative', overflow: 'hidden'
    }}>
      {/* Decorative gradients */}
      <Box sx={{ position: 'absolute', top: -200, right: -200, width: 400, height: 400,
        borderRadius: '50%', background: 'radial-gradient(circle, rgba(255,255,255,0.06) 0%, transparent 70%)' }} />
      <Box sx={{ position: 'absolute', bottom: -150, left: -150, width: 300, height: 300,
        borderRadius: '50%', background: 'radial-gradient(circle, rgba(255,255,255,0.04) 0%, transparent 70%)' }} />

      <Card sx={{
        maxWidth: 440, width: '100%', mx: 2, borderRadius: 4,
        boxShadow: '0 24px 80px rgba(0,0,0,0.35)',
        position: 'relative', zIndex: 1,
        backdropFilter: 'blur(4px)',
        border: '1px solid rgba(255,255,255,0.1)'
      }}>
        <CardContent sx={{ p: { xs: 3, sm: 5 } }}>
          <Box sx={{ textAlign: 'center' }} mb={4}>
            <Box sx={{
              width: 64, height: 64, borderRadius: '50%',
              background: 'linear-gradient(135deg, #1565c0, #1a237e)',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              mx: 'auto', mb: 2,
              boxShadow: '0 8px 24px rgba(26,35,126,0.3)'
            }}>
              <LockResetOutlined sx={{ fontSize: 32, color: '#fff' }} />
            </Box>
            <Typography variant="h4" fontWeight={700} color="primary.main" sx={{ letterSpacing: '-0.02em' }}>
              Set New Password
            </Typography>
            <Typography variant="body2" color="text.secondary" mt={1}>
              Enter your new password below
            </Typography>
          </Box>

          {error && (
            <Alert severity="error" sx={{ mb: 3, borderRadius: 2 }} onClose={() => setError('')}>
              {error}
            </Alert>
          )}

          <form onSubmit={handleSubmit}>
            <TextField
              fullWidth label="New Password" type={showPassword.pwd ? 'text' : 'password'}
              value={form.newPassword}
              onChange={(e) => setForm({ ...form, newPassword: e.target.value })}
              margin="normal" required autoFocus
              sx={{ '& .MuiOutlinedInput-root': { borderRadius: 2 } }}
              slotProps={{
                input: {
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton onClick={() => setShowPassword({ ...showPassword, pwd: !showPassword.pwd })} edge="end">
                        {showPassword.pwd ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                },
              }}
            />
            <TextField
              fullWidth label="Confirm Password" type={showPassword.confirm ? 'text' : 'password'}
              value={form.confirmPassword}
              onChange={(e) => setForm({ ...form, confirmPassword: e.target.value })}
              margin="normal" required
              sx={{ '& .MuiOutlinedInput-root': { borderRadius: 2 } }}
              slotProps={{
                input: {
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton onClick={() => setShowPassword({ ...showPassword, confirm: !showPassword.confirm })} edge="end">
                        {showPassword.confirm ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                },
              }}
            />
            <Button
              type="submit" fullWidth variant="contained" size="large"
              disabled={loading || !token}
              sx={{ mt: 3, mb: 2, py: 1.5, borderRadius: 2, textTransform: 'none', fontSize: 16,
                background: 'linear-gradient(135deg, #1a237e, #1565c0)',
                '&:hover': { background: 'linear-gradient(135deg, #283593, #1976d2)' }
              }}
            >
              {loading ? <CircularProgress size={22} sx={{ color: 'white' }} /> : 'Reset Password'}
            </Button>
          </form>

          <Box sx={{ textAlign: 'center', mt: 1 }}>
            <Button variant="text" onClick={() => navigate('/login')} sx={{ textTransform: 'none' }}>
              Back to Sign In
            </Button>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default ResetPasswordPage;
