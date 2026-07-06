import { Box, Typography, Pagination as MuiPagination } from '@mui/material';

export const Pagination = ({ page, count, rowsPerPage, onPageChange, onRowsPerPageChange, rowsPerPageOptions = [10, 20, 50, 100] }) => {
  if (!count) return null;

  const totalPages = Math.ceil(count / rowsPerPage);
  const start = count === 0 ? 0 : page * rowsPerPage + 1;
  const end = Math.min((page + 1) * rowsPerPage, count);

  return (
    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', py: 2, px: 1 }}>
      <Typography variant="body2" color="text.secondary">
        Showing {start}-{end} of {count}
      </Typography>
      <MuiPagination
        count={totalPages}
        page={page + 1}
        onChange={(e, p) => onPageChange(p - 1)}
        color="primary"
        size="small"
        showFirstButton
        showLastButton
      />
    </Box>
  );
};
