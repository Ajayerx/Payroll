import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  AppBar, Toolbar, IconButton, Typography, Box, Avatar, Menu, MenuItem,
  ListItemIcon, Divider, Tooltip, Badge, List, ListItem, ListItemText,
  ListItemButton, Button
} from '@mui/material';
import {
  Menu as MenuIcon, DarkMode, LightMode, Person, Settings,
  Logout, Notifications, MarkEmailRead
} from '@mui/icons-material';
import { useUI } from '../../hooks/useUI';
import { useAuth } from '../../hooks/useAuth';
import { useNotifications } from '../../hooks/useNotifications';
import { formatRelativeTime } from '../../utils/formatters';

export const Header = () => {
  const navigate = useNavigate();
  const { sidebarOpen, toggleSidebar, theme, setTheme } = useUI();
  const { user, logout } = useAuth();
  const [anchorEl, setAnchorEl] = useState(null);
  const [notifAnchor, setNotifAnchor] = useState(null);
  const {
    notifications, unreadCount, markAsRead, markAllAsRead,
    dropdownOpen: notifOpen, setDropdownOpen: setNotifOpen
  } = useNotifications();
  const open = Boolean(anchorEl);

  const handleLogout = async () => {
    setAnchorEl(null);
    await logout();
    navigate('/login');
  };

  const handleNotifClick = (notification) => {
    if (!notification.isRead) {
      markAsRead([notification.id]);
    }
    if (notification.link) {
      navigate(notification.link);
    }
    setNotifAnchor(null);
  };

  const handleNotifOpen = (e) => {
    setNotifOpen(true);
    setNotifAnchor(e.currentTarget);
  };

  const handleNotifClose = () => {
    setNotifOpen(false);
    setNotifAnchor(null);
  };

  return (
    <AppBar position="fixed" color="inherit" elevation={0}
      sx={{
        width: sidebarOpen ? `calc(100% - 260px)` : '100%',
        ml: sidebarOpen ? '260px' : 0,
        transition: '0.2s',
        borderBottom: 1,
        borderColor: 'divider',
      }}
    >
      <Toolbar>
        <IconButton edge="start" onClick={toggleSidebar} sx={{ mr: 1 }}>
          <MenuIcon />
        </IconButton>

        <Typography variant="h6" sx={{ flexGrow: 1 }} />

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Tooltip title={`Switch to ${theme === 'light' ? 'dark' : 'light'} mode`}>
            <IconButton onClick={() => setTheme(theme === 'light' ? 'dark' : 'light')}>
              {theme === 'light' ? <DarkMode /> : <LightMode />}
            </IconButton>
          </Tooltip>

          <Tooltip title="Notifications">
            <IconButton onClick={handleNotifOpen}>
              <Badge badgeContent={unreadCount} color="error">
                <Notifications />
              </Badge>
            </IconButton>
          </Tooltip>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, cursor: 'pointer' }}
               onClick={(e) => setAnchorEl(e.currentTarget)}>
            <Avatar sx={{ width: 32, height: 32, bgcolor: 'primary.main', fontSize: 14 }}>
              {user?.firstName?.[0]}{user?.lastName?.[0]}
            </Avatar>
            <Box sx={{ display: { xs: 'none', sm: 'block' } }}>
              <Typography variant="body2" fontWeight={600}>{user?.firstName} {user?.lastName}</Typography>
              <Typography variant="caption" color="text.secondary">{user?.role}</Typography>
            </Box>
          </Box>
        </Box>

        <Menu anchorEl={anchorEl} open={open} onClose={() => setAnchorEl(null)}
              transformOrigin={{ horizontal: 'right', vertical: 'top' }}
              anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}>
          <MenuItem onClick={() => { setAnchorEl(null); navigate('/profile'); }}>
            <ListItemIcon><Person fontSize="small" /></ListItemIcon>Profile
          </MenuItem>
          <MenuItem onClick={() => { setAnchorEl(null); navigate('/settings'); }}>
            <ListItemIcon><Settings fontSize="small" /></ListItemIcon>Settings
          </MenuItem>
          <MenuItem onClick={() => { setAnchorEl(null); navigate('/change-password'); }}>
            <ListItemIcon><Settings fontSize="small" /></ListItemIcon>Change Password
          </MenuItem>
          <Divider />
          <MenuItem onClick={handleLogout}>
            <ListItemIcon><Logout fontSize="small" /></ListItemIcon>Logout
          </MenuItem>
        </Menu>

        <Menu anchorEl={notifAnchor} open={Boolean(notifAnchor)}
              onClose={handleNotifClose}
              transformOrigin={{ horizontal: 'right', vertical: 'top' }}
              anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
              slotProps={{ paper: { sx: { width: 360, maxHeight: 400 } } }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', px: 2, py: 1 }}>
            <Typography variant="subtitle2" fontWeight={600}>Notifications</Typography>
            {unreadCount > 0 && (
              <Button size="small" startIcon={<MarkEmailRead />} onClick={() => { markAllAsRead(); }}>
                Mark all read
              </Button>
            )}
          </Box>
          <Divider />
          {notifications.length === 0 ? (
            <Box sx={{ textAlign: 'center', py: 4, px: 2 }}>
              <Typography variant="body2" color="text.secondary">No notifications</Typography>
            </Box>
          ) : (
            <List disablePadding>
              {notifications.slice(0, 10).map((n) => (
                <ListItem key={n.id} disablePadding
                  sx={{ bgcolor: n.isRead ? 'transparent' : 'action.hover' }}>
                  <ListItemButton onClick={() => handleNotifClick(n)} sx={{ py: 1.5, px: 2 }}>
                    <ListItemText
                      primary={n.title}
                      secondary={
                        <>
                          {n.message}
                          <Typography variant="caption" display="block" color="text.disabled">
                            {formatRelativeTime(n.createdDate)}
                          </Typography>
                        </>
                      }
                      primaryTypographyProps={{
                        variant: 'body2',
                        fontWeight: n.isRead ? 400 : 600,
                      }}
                    />
                  </ListItemButton>
                </ListItem>
              ))}
            </List>
          )}
        </Menu>
      </Toolbar>
    </AppBar>
  );
};
