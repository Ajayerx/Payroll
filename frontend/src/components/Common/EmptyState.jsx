import { Box, Typography, Button } from '@mui/material';
import InboxIcon from '@mui/icons-material/Inbox';

export const EmptyState = ({ title = 'No data found', description, actionLabel, onAction, icon }) => (
  <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: 300, p: 3 }}>
    {icon || <InboxIcon sx={{ fontSize: 64, color: 'text.disabled', mb: 2 }} />}
    <Typography variant="h6" color="text.secondary" gutterBottom>{title}</Typography>
    {description && <Typography variant="body2" color="text.disabled" align="center" sx={{ mb: 2 }}>{description}</Typography>}
    {actionLabel && onAction && (
      <Button variant="contained" onClick={onAction}>{actionLabel}</Button>
    )}
  </Box>
);
