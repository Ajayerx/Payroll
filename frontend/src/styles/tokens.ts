export const colors = {
  primary: {
    50: '#e8eaf6',
    100: '#c5cae9',
    200: '#9fa8da',
    300: '#7986cb',
    400: '#5c6bc0',
    500: '#3f51b5',
    600: '#1a237e',
    700: '#000051',
    800: '#000033',
    900: '#00001a',
    DEFAULT: '#1a237e',
  },
  secondary: {
    50: '#e0f2f1',
    100: '#b2dfdb',
    200: '#80cbc4',
    300: '#4db6ac',
    400: '#26a69a',
    500: '#00897b',
    600: '#00695c',
    700: '#004d40',
    800: '#00332a',
    900: '#001a15',
    DEFAULT: '#00897b',
  },
  success: { main: '#2e7d32', light: '#4caf50', dark: '#1b5e20' },
  warning: { main: '#ed6c02', light: '#ff9800', dark: '#e65100' },
  error: { main: '#d32f2f', light: '#ef5350', dark: '#c62828' },
  surface: { DEFAULT: '#f4f6f8', dark: '#0a1929', paper: '#ffffff' },
  text: { primary: '#1a1a2e', secondary: '#546e7a', disabled: '#9e9e9e' },
  border: { DEFAULT: '#e0e0e0', light: '#f0f0f0', dark: '#bdbdbd' },
} as const;

export const spacing = {
  xs: 4,
  sm: 8,
  md: 16,
  lg: 24,
  xl: 32,
  '2xl': 48,
  '3xl': 64,
} as const;

export const typography = {
  fontFamily: "'Inter', 'Roboto', 'Helvetica', 'Arial', sans-serif",
  sizes: {
    xs: '0.75rem',
    sm: '0.875rem',
    base: '1rem',
    lg: '1.125rem',
    xl: '1.25rem',
    '2xl': '1.5rem',
    '3xl': '1.875rem',
    '4xl': '2.25rem',
  },
  weights: {
    normal: 400,
    medium: 500,
    semibold: 600,
    bold: 700,
  },
} as const;

export const shadows = {
  sm: '0 1px 2px rgba(0,0,0,0.05)',
  md: '0 1px 3px rgba(0,0,0,0.08)',
  lg: '0 4px 12px rgba(0,0,0,0.1)',
  xl: '0 8px 24px rgba(0,0,0,0.12)',
} as const;

export const borderRadius = {
  sm: 4,
  md: 8,
  lg: 12,
  xl: 16,
  full: 9999,
} as const;
