import { Box, Typography, Breadcrumbs, Link } from '@mui/material';

export const PageHeader = ({ title, subtitle, breadcrumbs, action }) => (
  <Box mb={4}>
    {breadcrumbs && (
      <Breadcrumbs sx={{ mb: 1 }}>
        {breadcrumbs.map((item, index) =>
          index < breadcrumbs.length - 1 ? (
            <Link key={index} component="button" variant="body2" onClick={() => item.onClick?.() || {}} color="text.secondary" underline="hover">
              {item.label}
            </Link>
          ) : (
            <Typography key={index} variant="body2" color="text.primary">{item.label}</Typography>
          )
        )}
      </Breadcrumbs>
    )}
    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}>
      <Box>
        <Typography variant="h5" fontWeight={700}>{title}</Typography>
        {subtitle && <Typography variant="body2" color="text.secondary" mt={0.5}>{subtitle}</Typography>}
      </Box>
      {action}
    </Box>
  </Box>
);
