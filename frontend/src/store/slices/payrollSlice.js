import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { payrollService } from '../../services/payrollService';

export const fetchPayrolls = createAsyncThunk(
  'payroll/fetchAll',
  async (params, { rejectWithValue }) => {
    try {
      const response = await payrollService.getAll(params);
      return response.data;
    } catch (error) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch payrolls');
    }
  }
);

export const processPayroll = createAsyncThunk(
  'payroll/process',
  async (data, { rejectWithValue }) => {
    try {
      const response = await payrollService.process(data);
      return response.data;
    } catch (error) {
      return rejectWithValue(error.response?.data?.message || 'Payroll processing failed');
    }
  }
);

const payrollSlice = createSlice({
  name: 'payroll',
  initialState: {
    items: [],
    selected: null,
    total: 0,
    loading: false,
    processing: false,
    error: null,
  },
  reducers: {
    clearSelected: (state) => {
      state.selected = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchPayrolls.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchPayrolls.fulfilled, (state, action) => {
        state.loading = false;
        state.items = action.payload.items || action.payload;
        state.total = action.payload.total || 0;
      })
      .addCase(fetchPayrolls.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(processPayroll.pending, (state) => {
        state.processing = true;
      })
      .addCase(processPayroll.fulfilled, (state, action) => {
        state.processing = false;
        state.items.unshift(action.payload);
        state.total += 1;
      })
      .addCase(processPayroll.rejected, (state, action) => {
        state.processing = false;
        state.error = action.payload;
      });
  },
});

export const { clearSelected } = payrollSlice.actions;
export default payrollSlice.reducer;
