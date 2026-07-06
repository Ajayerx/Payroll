export const required = (message = 'Required') => value => (value ? undefined : message);

export const email = (message = 'Invalid email') => value =>
  value && !/^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i.test(value) ? message : undefined;

export const minLength = (min, message) => value =>
  value && value.length < min ? (message || `Must be at least ${min} characters`) : undefined;

export const maxLength = (max, message) => value =>
  value && value.length > max ? (message || `Must be at most ${max} characters`) : undefined;

export const numeric = (message = 'Must be a number') => value =>
  value && isNaN(Number(value)) ? message : undefined;

export const positiveNumber = (message = 'Must be a positive number') => value =>
  value && (isNaN(Number(value)) || Number(value) <= 0) ? message : undefined;

export const phoneNumber = (message = 'Invalid phone number') => value =>
  value && !/^\+?[\d\s-]{10,15}$/.test(value) ? message : undefined;

export const composeValidators = (...validators) => value =>
  validators.reduce((error, validator) => error || validator(value), undefined);
