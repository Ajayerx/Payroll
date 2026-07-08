import { useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import { ThemeProvider, createTheme, CssBaseline } from '@mui/material';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

import { ProtectedRoute } from './components/Auth/ProtectedRoute';
import { AdminLayout } from './components/Layout/AdminLayout';
import { EmployeeLayout } from './components/Layout/EmployeeLayout';
import { ErrorBoundary } from './components/Common/ErrorBoundary';
import { ROLES } from './constants';
import { checkSessionTimeout } from './services/axios';

import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import EmployeeListPage from './pages/EmployeeListPage';
import EmployeeFormPage from './pages/EmployeeFormPage';
import EmployeeProfilePage from './pages/EmployeeProfilePage';
import PayrollListPage from './pages/PayrollListPage';
import PayrollProcessPage from './pages/PayrollProcessPage';
import ReportsPage from './pages/ReportsPage';
import SettingsPage from './pages/SettingsPage';
import MySalaryPage from './pages/MySalaryPage';
import LeaveListPage from './pages/LeaveListPage';
import LeaveFormPage from './pages/LeaveFormPage';
import ProfilePage from './pages/ProfilePage';
import ChangePasswordPage from './pages/ChangePasswordPage';
import ForgotPasswordPage from './pages/ForgotPasswordPage';
import ResetPasswordPage from './pages/ResetPasswordPage';
import RegisterPage from './pages/RegisterPage';

const getTheme = (mode: 'light' | 'dark') => createTheme({
  palette: {
    mode,
    primary: { main: '#1a237e', light: '#534bae', dark: '#000051' },
    secondary: { main: '#00897b', light: '#4ebaaa', dark: '#005b4f' },
    ...(mode === 'light'
      ? {
          background: { default: '#f4f6f8', paper: '#ffffff' },
          text: { primary: '#1a1a2e', secondary: '#546e7a' },
        }
      : {
          background: { default: '#0a1929', paper: '#132f4c' },
          text: { primary: '#e3f2fd', secondary: '#90caf9' },
        }),
  },
  typography: {
    fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
    h4: { fontWeight: 700, letterSpacing: '-0.02em' },
    h5: { fontWeight: 600, letterSpacing: '-0.01em' },
    h6: { fontWeight: 600 },
    subtitle1: { fontWeight: 500 },
    body2: { lineHeight: 1.6 },
    button: { fontWeight: 600 },
  },
  shape: { borderRadius: 10 },
  components: {
    MuiButton: {
      styleOverrides: {
        root: { textTransform: 'none', fontWeight: 600, borderRadius: 8, padding: '8px 20px' },
        contained: { boxShadow: 'none', '&:hover': { boxShadow: '0 4px 12px rgba(0,0,0,0.15)' } },
      },
    },
    MuiCard: { styleOverrides: { root: { borderRadius: 12, boxShadow: '0 1px 3px rgba(0,0,0,0.08)' } } },
    MuiPaper: { styleOverrides: { root: { borderRadius: 12 } } },
    MuiTableCell: {
      styleOverrides: {
        root: {
          '&.MuiTableCell-head': {
            fontWeight: 600,
            backgroundColor: mode === 'dark' ? '#1a3a5c' : '#1a237e',
            color: '#ffffff',
            borderColor: 'rgba(255,255,255,0.12)',
          },
          '&.MuiTableCell-body': {
            color: 'inherit',
          },
        },
      },
    },
    MuiChip: { styleOverrides: { root: { fontWeight: 500 } } },
    MuiAppBar: { styleOverrides: { root: { boxShadow: '0 1px 3px rgba(0,0,0,0.08)' } } },
  },
});

function App() {
  const themeMode = useSelector((state: AppState) => state.ui?.theme || 'light');

  useEffect(() => {
    checkSessionTimeout();
  }, []);

  return (
    <ThemeProvider theme={getTheme(themeMode)}>
      <CssBaseline />
      <BrowserRouter>
        <ErrorBoundary>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/forgot-password" element={<ForgotPasswordPage />} />
            <Route path="/reset-password" element={<ResetPasswordPage />} />
            <Route path="/register" element={<RegisterPage />} />

            {/* Admin & HR Routes */}
            <Route element={<ProtectedRoute roles={[ROLES.ADMIN, ROLES.HR_MANAGER]} />}>
              <Route element={<AdminLayout />}>
                <Route path="/dashboard" element={<DashboardPage />} />
                <Route path="/employees" element={<EmployeeListPage />} />
                <Route path="/employees/new" element={<EmployeeFormPage />} />
                <Route path="/employees/:id" element={<EmployeeProfilePage />} />
                <Route path="/employees/:id/edit" element={<EmployeeFormPage />} />
                <Route path="/payroll" element={<PayrollListPage />} />
                <Route path="/payroll/process" element={<PayrollProcessPage />} />
                <Route path="/reports" element={<ReportsPage />} />
                <Route path="/settings" element={<SettingsPage />} />
                <Route path="/leaves" element={<LeaveListPage />} />
                <Route path="/leaves/new" element={<LeaveFormPage />} />
                <Route path="/profile" element={<ProfilePage />} />
                <Route path="/change-password" element={<ChangePasswordPage />} />
              </Route>
            </Route>

            {/* Employee Routes */}
            <Route element={<ProtectedRoute roles={[ROLES.EMPLOYEE]} />}>
              <Route element={<EmployeeLayout />}>
                <Route path="/dashboard" element={<DashboardPage />} />
                <Route path="/my-salary" element={<MySalaryPage />} />
                <Route path="/leaves" element={<LeaveListPage />} />
                <Route path="/leaves/new" element={<LeaveFormPage />} />
                <Route path="/profile" element={<ProfilePage />} />
                <Route path="/change-password" element={<ChangePasswordPage />} />
              </Route>
            </Route>

            <Route path="/" element={<Navigate to="/dashboard" replace />} />
            <Route path="*" element={<Navigate to="/dashboard" replace />} />
          </Routes>
        </ErrorBoundary>
      </BrowserRouter>
      <ToastContainer
        position="top-right"
        autoClose={3000}
        hideProgressBar={false}
        newestOnTop
        closeOnClick
        pauseOnHover
        draggable
        theme={themeMode}
      />
    </ThemeProvider>
  );
}

export default App;
