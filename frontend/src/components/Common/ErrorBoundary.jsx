import React from 'react';
import { Box, Typography, Button } from '@mui/material';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutlineOutlined';

export const ErrorFallback = ({ error, resetErrorBoundary }) => (
  <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: 400, p: 3 }}>
    <ErrorOutlineIcon color="error" sx={{ fontSize: 64, mb: 2 }} />
    <Typography variant="h5" gutterBottom>Something went wrong</Typography>
    <Typography variant="body2" color="text.secondary" align="center" sx={{ mb: 2 }}>
      {error?.message || 'An unexpected error occurred'}
    </Typography>
    {resetErrorBoundary && (
      <Button variant="contained" onClick={resetErrorBoundary}>Try Again</Button>
    )}
  </Box>
);

export class ErrorBoundary extends React.Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error) {
    return { hasError: true, error };
  }

  resetErrorBoundary = () => {
    this.setState({ hasError: false, error: null });
  };

  render() {
    if (this.state.hasError) {
      return (
        <ErrorFallback
          error={this.state.error}
          resetErrorBoundary={this.resetErrorBoundary}
        />
      );
    }
    return this.props.children;
  }
}
