import { Box, Toolbar, Container } from '@mui/material';
import { Sidebar } from './Sidebar';
import { Header } from './Header';

export const Layout = ({ children }) => (
  <Box sx={{ display: 'flex', minHeight: '100vh' }}>
    <Header />
    <Sidebar />
    <Box
      component="main"
      sx={{
        flexGrow: 1,
        p: { xs: 2, sm: 3, md: 4 },
        transition: '0.2s',
        bgcolor: 'grey.50',
        minHeight: '100vh',
      }}
    >
      <Toolbar />
      <Container maxWidth="xl" disableGutters sx={{ px: { xs: 0, md: 1 } }}>
        {children}
      </Container>
    </Box>
  </Box>
);
