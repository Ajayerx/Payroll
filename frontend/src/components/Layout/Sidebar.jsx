import { useNavigate, useLocation } from 'react-router-dom';
import {
  Drawer, List, ListItemButton, ListItemIcon, ListItemText, Box,
  Typography, Divider, useTheme, useMediaQuery
} from '@mui/material';
import {
  Dashboard, People, AttachMoney, Receipt, Assessment,
  Settings, Logout, EventNote
} from '@mui/icons-material';
import { useUI } from '../../hooks/useUI';
import { useAuth } from '../../hooks/useAuth';

const drawerWidth = 270;

const iconMap = {
  Dashboard: <Dashboard />,
  People: <People />,
  AttachMoney: <AttachMoney />,
  EventNote: <EventNote />,
  Assessment: <Assessment />,
  Settings: <Settings />,
  Receipt: <Receipt />,
};

export const Sidebar = ({ menuItems = [] }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const { sidebarOpen, toggleSidebar } = useUI();
  const { user, logout } = useAuth();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  const content = (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <Box sx={{ p: 3, display: 'flex', alignItems: 'center', gap: 1.5 }}>
        <Box
          sx={{
            width: 40, height: 40, borderRadius: 2,
            background: 'linear-gradient(135deg, #1a237e, #534bae)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
          }}
        >
          <AttachMoney sx={{ color: '#fff', fontSize: 24 }} />
        </Box>
        <Box>
          <Typography variant="h6" fontWeight={700} sx={{ lineHeight: 1.2 }}>PayrollApp</Typography>
          <Typography variant="caption" color="text.secondary">{user?.role === 'Employee' ? 'Employee Portal' : 'Enterprise Suite'}</Typography>
        </Box>
      </Box>
      <Divider />
      <List sx={{ flex: 1, px: 1.5, py: 1 }}>
        {menuItems.map((item) => (
          <ListItemButton
            key={item.path}
            selected={location.pathname === item.path || (item.path !== '/' && location.pathname.startsWith(item.path))}
            onClick={() => { navigate(item.path); if (isMobile) toggleSidebar(); }}
            sx={{
              borderRadius: 2, mb: 0.5, py: 1.2, px: 2,
              '&.Mui-selected': {
                backgroundColor: 'primary.main',
                color: '#fff',
                '&:hover': { backgroundColor: 'primary.dark' },
                '& .MuiListItemIcon-root': { color: '#fff' },
              },
            }}
          >
            <ListItemIcon sx={{ minWidth: 38, color: 'inherit' }}>
              {iconMap[item.icon] || <Dashboard />}
            </ListItemIcon>
            <ListItemText primary={item.label} slotProps={{ primary: { fontWeight: 500, fontSize: 14 } }} />
          </ListItemButton>
        ))}
      </List>
      <Divider />
      <List sx={{ px: 1.5, py: 1 }}>
        <ListItemButton onClick={handleLogout} sx={{ borderRadius: 2, py: 1.2, px: 2 }}>
          <ListItemIcon sx={{ minWidth: 38 }}><Logout /></ListItemIcon>
          <ListItemText primary="Logout" slotProps={{ primary: { fontWeight: 500, fontSize: 14 } }} />
        </ListItemButton>
      </List>
    </Box>
  );

  return (
    <Drawer
      variant={isMobile ? 'temporary' : 'persistent'}
      open={sidebarOpen}
      onClose={toggleSidebar}
      sx={{
        width: sidebarOpen ? drawerWidth : 0,
        flexShrink: 0,
        '& .MuiDrawer-paper': {
          width: drawerWidth,
          boxSizing: 'border-box',
          borderRight: '1px solid',
          borderColor: 'divider',
        },
      }}
    >
      {content}
    </Drawer>
  );
};
