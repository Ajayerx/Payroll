import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, Grid, Card, CardContent, Typography, Avatar, Button, Divider, TextField, CircularProgress
} from '@mui/material';
import { ArrowBack, Save } from '@mui/icons-material';
import { toast } from 'react-toastify';
import { PageHeader } from '../components/Common/PageHeader';
import { useAuth } from '../hooks/useAuth';

const ProfilePage = () => {
  const navigate = useNavigate();
  const { user } = useAuth();

  return (
    <>
      <PageHeader
        title="My Profile"
        subtitle="View and manage your profile information"
        breadcrumbs={[{ label: 'Dashboard', onClick: () => navigate('/dashboard') }, { label: 'Profile' }]}
        action={
          <Button variant="outlined" startIcon={<ArrowBack />} onClick={() => navigate('/dashboard')}>
            Back to Dashboard
          </Button>
        }
      />

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 4 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center', py: 4 }}>
              <Avatar sx={{ width: 100, height: 100, mx: 'auto', mb: 2, bgcolor: 'primary.main', fontSize: 36 }}>
                {user?.firstName?.[0]}{user?.lastName?.[0]}
              </Avatar>
              <Typography variant="h5" fontWeight={600}>{user?.firstName} {user?.lastName}</Typography>
              <Typography variant="body2" color="text.secondary">{user?.role}</Typography>
              <Typography variant="body2" color="text.secondary">{user?.email}</Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 8 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" fontWeight={600} gutterBottom>Account Details</Typography>
              <Divider sx={{ mb: 2 }} />
              <Grid container spacing={2}>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <Typography variant="caption" color="text.secondary">First Name</Typography>
                  <Typography variant="body2">{user?.firstName || '-'}</Typography>
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <Typography variant="caption" color="text.secondary">Last Name</Typography>
                  <Typography variant="body2">{user?.lastName || '-'}</Typography>
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <Typography variant="caption" color="text.secondary">Email</Typography>
                  <Typography variant="body2">{user?.email || '-'}</Typography>
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <Typography variant="caption" color="text.secondary">Role</Typography>
                  <Typography variant="body2">{user?.role || '-'}</Typography>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </>
  );
};

export default ProfilePage;
