import dayjs from 'dayjs';

export const getCurrentMonthYear = () => ({
  month: dayjs().month() + 1,
  year: dayjs().year(),
});

export const months = [
  { value: 1, label: 'January' },
  { value: 2, label: 'February' },
  { value: 3, label: 'March' },
  { value: 4, label: 'April' },
  { value: 5, label: 'May' },
  { value: 6, label: 'June' },
  { value: 7, label: 'July' },
  { value: 8, label: 'August' },
  { value: 9, label: 'September' },
  { value: 10, label: 'October' },
  { value: 11, label: 'November' },
  { value: 12, label: 'December' },
];

export const fiscalYears = () => {
  const currentYear = dayjs().year();
  return Array.from({ length: 5 }, (_, i) => ({
    value: currentYear - 2 + i,
    label: `FY ${currentYear - 2 + i}-${(currentYear - 2 + i + 1).toString().slice(2)}`,
  }));
};

export const getStatusColor = (status) => {
  const colors = {
    Draft: 'warning',
    Processed: 'info',
    Paid: 'success',
    Active: 'success',
    Inactive: 'error',
    Pending: 'warning',
    Approved: 'success',
    Rejected: 'error',
  };
  return colors[status] || 'default';
};

export const generateEmployeeCode = (department, count) => {
  const prefix = department?.substring(0, 3).toUpperCase() || 'EMP';
  return `${prefix}${String(count + 1).padStart(4, '0')}`;
};

export const downloadFile = (blob, filename) => {
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.setAttribute('download', filename);
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  window.URL.revokeObjectURL(url);
};
