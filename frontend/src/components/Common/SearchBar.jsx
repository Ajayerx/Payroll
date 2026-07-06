import { useState, useEffect, useRef } from 'react';
import { TextField, InputAdornment } from '@mui/material';
import { Search } from '@mui/icons-material';

export const SearchBar = ({ value, onChange, placeholder = 'Search...', debounceMs = 300, sx, ...props }) => {
  const [localValue, setLocalValue] = useState(value || '');
  const timeoutRef = useRef(null);

  useEffect(() => {
    setLocalValue(value || '');
  }, [value]);

  const handleChange = (e) => {
    const newValue = e.target.value;
    setLocalValue(newValue);
    if (timeoutRef.current) clearTimeout(timeoutRef.current);
    timeoutRef.current = setTimeout(() => {
      onChange?.(newValue);
    }, debounceMs);
  };

  useEffect(() => {
    return () => {
      if (timeoutRef.current) clearTimeout(timeoutRef.current);
    };
  }, []);

  return (
    <TextField
      size="small"
      placeholder={placeholder}
      value={localValue}
      onChange={handleChange}
      sx={{ minWidth: 300, ...sx }}
      slotProps={{
        input: {
          startAdornment: (
            <InputAdornment position="start"><Search color="action" /></InputAdornment>
          ),
        },
      }}
      {...props}
    />
  );
};
