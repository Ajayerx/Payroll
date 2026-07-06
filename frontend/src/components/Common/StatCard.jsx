import { Paper, Typography, Box } from '@mui/material';

export const StatCard = ({ title, value, icon, color = 'primary.main', subtitle, onClick }) => (
  <Paper
    elevation={0}
    sx={{
      cursor: onClick ? 'pointer' : 'default',
      transition: '0.25s',
      border: 1,
      borderColor: 'divider',
      borderRadius: 2,
      height: '100%',
      '&:hover': onClick ? { transform: 'translateY(-3px)', boxShadow: 2, borderColor: color } : {},
    }}
    onClick={onClick}
  >
    <Box sx={{ p: { xs: 2, sm: 3 } }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <Box>
          <Typography variant="body2" color="text.secondary" gutterBottom sx={{ fontWeight: 500, letterSpacing: 0.3 }}>{title}</Typography>
          <Typography variant="h4" fontWeight={700} sx={{ fontSize: { xs: '1.5rem', sm: '2rem' } }}>{value}</Typography>
          {subtitle && <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5, display: 'block' }}>{subtitle}</Typography>}
        </Box>
        {icon && (
          <Box sx={{ color, opacity: 0.85 }}>{icon}</Box>
        )}
      </Box>
    </Box>
  </Paper>
);
