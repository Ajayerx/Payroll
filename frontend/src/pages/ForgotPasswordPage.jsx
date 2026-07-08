import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, Card, CardContent, Typography, TextField, Button, Alert
} from '@mui/material';
import { toast } from 'react-toastify';
import { authService } from '../services/authService';

const ForgotPasswordPage = () => {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [sent, setSent] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await authService.forgotPassword({ email });
      setSent(true);
      toast.success('If the email exists, a reset link has been sent');
    } catch {
      toast.error('Failed to process request');
    } finally {
      setLoading(false);
    }
  };

  if (sent) {
    return (
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', minHeight: '100vh',
        background: 'linear-gradient(135deg, #1976d2 0%, #1565c0 50%, #0d47a1 100%)' }}>
        <Card sx={{ maxWidth: 440, width: '100%', mx: 2, borderRadius: 3, boxShadow: '0 20px 60px rgba(0,0,0,0.3)' }}>
          <CardContent sx={{ p: { xs: 3, sm: 4 }, textAlign: 'center' }}>
            <Typography variant="h5" fontWeight={700} gutterBottom>Check Your Email</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
              If an account with {email} exists, we've sent password reset instructions.
            </Typography>
            <Button variant="contained" onClick={() => navigate('/login')}>Back to Login</Button>
          </CardContent>
        </Card>
      </Box>
    );
  }

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', minHeight: '100vh',
      background: 'linear-gradient(135deg, #1976d2 0%, #1565c0 50%, #0d47a1 100%)' }}>
      <Card sx={{ maxWidth: 440, width: '100%', mx: 2, borderRadius: 3, boxShadow: '0 20px 60px rgba(0,0,0,0.3)' }}>
        <CardContent sx={{ p: { xs: 3, sm: 4 } }}>
          <Box sx={{ textAlign: 'center' }} mb={3}>
            <Typography variant="h4" fontWeight={700} color="primary">PayrollApp</Typography>
            <Typography variant="body2" color="text.secondary" mt={0.5}>Reset your password</Typography>
          </Box>
          <form onSubmit={handleSubmit}>
            <TextField fullWidth label="Email" type="email" value={email}
              onChange={(e) => setEmail(e.target.value)} required autoFocus sx={{ mb: 3 }} />
            <Button type="submit" fullWidth variant="contained" size="large" disabled={loading} sx={{ mb: 2 }}>
              {loading ? 'Sending...' : 'Send Reset Link'}
            </Button>
            <Button fullWidth variant="text" onClick={() => navigate('/login')}>Back to Login</Button>
          </form>
        </CardContent>
      </Card>
    </Box>
  );
};

export default ForgotPasswordPage;
