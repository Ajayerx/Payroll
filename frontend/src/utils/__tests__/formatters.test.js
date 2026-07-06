import { formatCurrency, formatDate, formatDateTime, formatPhone, truncate, capitalize } from '../formatters';

describe('formatCurrency', () => {
  it('formats number to INR currency', () => {
    expect(formatCurrency(50000)).toContain('50,000');
  });

  it('handles zero', () => {
    expect(formatCurrency(0)).toContain('0');
  });

  it('handles decimal values', () => {
    const result = formatCurrency(12345.67);
    expect(result).toContain('12');
    expect(result).toContain('345');
  });
});

describe('formatDate', () => {
  it('formats valid date string', () => {
    const result = formatDate('2025-06-01');
    expect(result).toBeTruthy();
    expect(typeof result).toBe('string');
  });

  it('returns empty string for null', () => {
    expect(formatDate(null)).toBe('');
  });

  it('returns empty string for undefined', () => {
    expect(formatDate(undefined)).toBe('');
  });
});

describe('formatDateTime', () => {
  it('formats valid datetime', () => {
    const result = formatDateTime('2025-06-01T10:30:00');
    expect(result).toBeTruthy();
    expect(typeof result).toBe('string');
  });
});

describe('formatPhone', () => {
  it('formats 10-digit phone', () => {
    expect(formatPhone('9876543210')).toBe('9876543210');
  });

  it('returns empty for null', () => {
    expect(formatPhone(null)).toBe('');
  });
});

describe('truncate', () => {
  it('truncates long strings', () => {
    expect(truncate('Hello World This Is Long', 10)).toBe('Hello Worl...');
  });

  it('does not truncate short strings', () => {
    expect(truncate('Hello', 10)).toBe('Hello');
  });
});

describe('capitalize', () => {
  it('capitalizes first letter', () => {
    expect(capitalize('hello')).toBe('Hello');
  });

  it('handles empty string', () => {
    expect(capitalize('')).toBe('');
  });

  it('handles single character', () => {
    expect(capitalize('a')).toBe('A');
  });
});
