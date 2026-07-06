import { Chip } from '@mui/material';
import { getStatusColor } from '../../utils/helpers';

export const StatusBadge = ({ status, size = 'small' }) => (
  <Chip
    label={status}
    color={getStatusColor(status)}
    size={size}
    variant="outlined"
  />
);
