import { Box, CircularProgress, Typography, Skeleton } from '@mui/material';

export const LoadingSpinner = ({ message = 'Loading...' }) => (
  <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: 400 }}>
    <CircularProgress />
    <Typography variant="body2" color="text.secondary" mt={2}>{message}</Typography>
  </Box>
);

export const SkeletonLoader = ({ count = 6, height = 60 }) => (
  <Box sx={{ p: 2 }}>
    {Array.from({ length: count }).map((_, i) => (
      <Skeleton key={i} variant="rectangular" height={height} sx={{ mb: 1, borderRadius: 1 }} />
    ))}
  </Box>
);

export const TableSkeleton = ({ rows = 8, cols = 5 }) => (
  <Box sx={{ p: 2 }}>
    <Skeleton variant="rectangular" height={48} sx={{ mb: 1, borderRadius: 1 }} />
    {Array.from({ length: rows }).map((_, i) => (
      <Skeleton key={i} variant="rectangular" height={40} sx={{ mb: 0.5, borderRadius: 1 }} />
    ))}
  </Box>
);
