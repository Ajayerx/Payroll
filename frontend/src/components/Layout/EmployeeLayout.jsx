import { Box, Toolbar, Container } from '@mui/material';
import { Outlet } from 'react-router-dom';
import { Sidebar } from './Sidebar';
import { Header } from './Header';

const menuItems = [
  { label: 'Dashboard', icon: 'Dashboard', path: '/dashboard' },
  { label: 'My Salary', icon: 'Receipt', path: '/my-salary' },
  { label: 'Leave Management', icon: 'EventNote', path: '/leaves' },
];

export const EmployeeLayout = () => (
  <Box sx={{ display: 'flex', minHeight: '100vh' }}>
    <Header />
    <Sidebar menuItems={menuItems} />
    <Box
      component="main"
      sx={{
        flexGrow: 1,
        p: { xs: 2, sm: 3, md: 4 },
        transition: '0.2s',
        bgcolor: 'background.default',
        minHeight: '100vh',
      }}
    >
      <Toolbar />
      <Container maxWidth="xl" sx={{ px: { xs: 1, sm: 2, md: 3 } }}>
        <Outlet />
      </Container>
    </Box>
  </Box>
);
