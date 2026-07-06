export const ROLES = {
  ADMIN: 'Admin',
  HR_MANAGER: 'HRManager',
  EMPLOYEE: 'Employee',
};

export const ROLES_LABELS = {
  [ROLES.ADMIN]: 'Administrator',
  [ROLES.HR_MANAGER]: 'HR Manager',
  [ROLES.EMPLOYEE]: 'Employee',
};

export const PAYROLL_STATUS = {
  DRAFT: 'Draft',
  PROCESSED: 'Processed',
  PAID: 'Paid',
};

export const PAYROLL_STATUS_LABELS = {
  [PAYROLL_STATUS.DRAFT]: 'Draft',
  [PAYROLL_STATUS.PROCESSED]: 'Processed',
  [PAYROLL_STATUS.PAID]: 'Paid',
};

export const LEAVE_REQUEST_STATUS = {
  PENDING: 'Pending',
  APPROVED: 'Approved',
  REJECTED: 'Rejected',
};

export const SALARY_COMPONENT_TYPES = {
  EARNING: 'Earning',
  DEDUCTION: 'Deduction',
};

export const DEDUCTION_TYPES = {
  LOAN: 'Loan',
  ADVANCE: 'Advance',
};

export const ENTITY_NAMES = {
  EMPLOYEE: 'Employee',
  PAYROLL: 'Payroll',
  SALARY_COMPONENT: 'SalaryComponent',
  DEDUCTION: 'Deduction',
  USER: 'User',
  LEAVE_REQUEST: 'LeaveRequest',
  COMPANY_SETTING: 'CompanySetting',
};

export const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5175/api/v1';

export const PAGINATION = {
  DEFAULT_PAGE_SIZE: 20,
  PAGE_SIZES: [10, 20, 50, 100],
};

export const THEME_CONFIG = {
  light: {
    primary: '#1976d2',
    secondary: '#dc004e',
  },
  dark: {
    primary: '#90caf9',
    secondary: '#f48fb1',
  },
};

export const SESSION_TIMEOUT = 30 * 60 * 1000;

export const TOAST_CONFIG = {
  position: 'top-right',
  autoClose: 3000,
  hideProgressBar: false,
  closeOnClick: true,
  pauseOnHover: true,
  draggable: true,
};
