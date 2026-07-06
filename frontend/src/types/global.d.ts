// Image declarations
declare module '*.png' {
  const src: string;
  export default src;
}
declare module '*.jpg' {
  const src: string;
  export default src;
}
declare module '*.jpeg' {
  const src: string;
  export default src;
}
declare module '*.gif' {
  const src: string;
  export default src;
}
declare module '*.svg' {
  import * as React from 'react';
  export const ReactComponent: React.FC<React.SVGProps<SVGSVGElement>>;
  const src: string;
  export default src;
}
declare module '*.css' {
  const classes: { readonly [key: string]: string };
  export default classes;
}

// Store types
interface RootState {
  auth: AuthState;
  ui: UIState;
  employee: EmployeeState;
  payroll: PayrollState;
}

interface AuthState {
  user: User | null;
  accessToken: string | null;
  isAuthenticated: boolean;
  loading: boolean;
  error: string | null;
}

interface UIState {
  sidebarOpen: boolean;
  theme: 'light' | 'dark';
  selectedMonth: number;
  selectedYear: number;
}

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: 'Admin' | 'HRManager' | 'Employee';
}
