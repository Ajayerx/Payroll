import {
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  TablePagination, TableSortLabel, Paper, Checkbox, Typography
} from '@mui/material';

export const DataTable = ({
  columns,
  rows,
  page,
  rowsPerPage,
  total,
  onPageChange,
  onRowsPerPageChange,
  order,
  orderBy,
  onSort,
  loading,
  selectable,
  selected,
  onSelectAllClick,
  onSelectClick,
  emptyMessage = 'No records found',
}) => {
  const handleSort = (column) => {
    if (!column.sortable) return;
    const isAsc = orderBy === column.id && order === 'asc';
    onSort?.(column.id, isAsc ? 'desc' : 'asc');
  };

  return (
    <Paper elevation={0} variant="outlined">
      <TableContainer>
        <Table size="small">
          <TableHead>
            <TableRow>
              {selectable && (
                <TableCell padding="checkbox">
                  <Checkbox
                    indeterminate={selected?.length > 0 && selected?.length < rows?.length}
                    checked={rows?.length > 0 && selected?.length === rows?.length}
                    onChange={onSelectAllClick}
                  />
                </TableCell>
              )}
              {columns.map((col) => (
                <TableCell key={col.id} align={col.align || 'left'} sx={{ fontWeight: 600, whiteSpace: 'nowrap' }}>
                  {col.sortable ? (
                    <TableSortLabel active={orderBy === col.id} direction={orderBy === col.id ? order : 'asc'} onClick={() => handleSort(col)}>
                      {col.label}
                    </TableSortLabel>
                  ) : (
                    col.label
                  )}
                </TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {rows?.length === 0 ? (
              <TableRow>
                <TableCell colSpan={columns.length + (selectable ? 1 : 0)} align="center" sx={{ py: 6 }}>
                  <Typography variant="body2" color="text.secondary">{emptyMessage}</Typography>
                </TableCell>
              </TableRow>
            ) : (
              rows?.map((row, index) => (
                <TableRow key={row.id || index} hover selected={selected?.includes(row.id)}>
                  {selectable && (
                    <TableCell padding="checkbox">
                      <Checkbox checked={selected?.includes(row.id)} onChange={() => onSelectClick?.(row.id)} />
                    </TableCell>
                  )}
                  {columns.map((col) => (
                    <TableCell key={col.id} align={col.align || 'left'}>
                      {col.render ? col.render(row) : row[col.id]}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
      <TablePagination
        component="div"
        count={total || 0}
        page={page || 0}
        rowsPerPage={rowsPerPage || 20}
        onPageChange={(e, p) => onPageChange?.(p)}
        onRowsPerPageChange={(e) => onRowsPerPageChange?.(parseInt(e.target.value, 10))}
        rowsPerPageOptions={[10, 20, 50, 100]}
      />
    </Paper>
  );
};
