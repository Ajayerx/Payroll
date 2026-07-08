import { useState, useMemo } from 'react';
import {
  Box, Card, CardContent, Typography, IconButton, Grid, Chip, Tooltip
} from '@mui/material';
import { ChevronLeft, ChevronRight } from '@mui/icons-material';
import dayjs from 'dayjs';

const WEEKDAYS = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
const STATUS_COLORS = { Approved: 'success', Pending: 'warning', Rejected: 'error', Cancelled: 'default' };

export const LeaveCalendar = ({ leaves = [] }) => {
  const [currentMonth, setCurrentMonth] = useState(dayjs().startOf('month'));

  const daysInMonth = currentMonth.daysInMonth();
  const startDayOfWeek = currentMonth.day();
  const monthLabel = currentMonth.format('MMMM YYYY');

  const calendarDays = useMemo(() => {
    const days = [];
    for (let i = 0; i < startDayOfWeek; i++) {
      days.push(null);
    }
    for (let d = 1; d <= daysInMonth; d++) {
      const date = currentMonth.date(d);
      const dayLeaves = leaves.filter(l => {
        const from = dayjs(l.fromDate).startOf('day');
        const to = dayjs(l.toDate).startOf('day');
        return date.isSame(from, 'day') || date.isSame(to, 'day') ||
          (date.isAfter(from) && date.isBefore(to));
      });
      days.push({ date, day: d, leaves: dayLeaves, isToday: date.isSame(dayjs(), 'day') });
    }
    return days;
  }, [currentMonth, leaves]);

  const prevMonth = () => setCurrentMonth(prev => prev.subtract(1, 'month'));
  const nextMonth = () => setCurrentMonth(prev => prev.add(1, 'month'));

  return (
    <Card>
      <CardContent>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <IconButton onClick={prevMonth}><ChevronLeft /></IconButton>
          <Typography variant="h6" fontWeight={600}>{monthLabel}</Typography>
          <IconButton onClick={nextMonth}><ChevronRight /></IconButton>
        </Box>

        <Grid container spacing={0.5}>
          {WEEKDAYS.map(d => (
            <Grid key={d} size={{ xs: 12 / 7 }}>
              <Typography variant="caption" fontWeight={600} sx={{ display: 'block', textAlign: 'center', py: 0.5 }}>
                {d}
              </Typography>
            </Grid>
          ))}

          {calendarDays.map((day, idx) => (
            <Grid key={idx} size={{ xs: 12 / 7 }}>
              {day ? (
                <Box sx={{
                  minHeight: 70, p: 0.5, borderRadius: 1,
                  bgcolor: day.isToday ? 'action.selected' : 'background.paper',
                  border: '1px solid', borderColor: 'divider',
                  position: 'relative', overflow: 'hidden',
                }}>
                  <Typography variant="caption" fontWeight={day.isToday ? 700 : 400}
                    sx={{ display: 'block', mb: 0.5 }}>
                    {day.day}
                  </Typography>
                  {day.leaves.slice(0, 2).map((l, i) => (
                    <Tooltip key={l.id || i} title={`${l.employeeName}: ${l.leaveType}`}>
                      <Chip
                        label={l.employeeName?.split(' ')[0] || l.leaveType}
                        size="small"
                        color={STATUS_COLORS[l.status] || 'default'}
                        variant="outlined"
                        sx={{ height: 18, fontSize: 10, mb: 0.25, maxWidth: '100%' }}
                      />
                    </Tooltip>
                  ))}
                  {day.leaves.length > 2 && (
                    <Typography variant="caption" color="text.secondary" sx={{ fontSize: 9 }}>
                      +{day.leaves.length - 2} more
                    </Typography>
                  )}
                </Box>
              ) : (
                <Box sx={{ minHeight: 70, bgcolor: 'grey.50', borderRadius: 1 }} />
              )}
            </Grid>
          ))}
        </Grid>
      </CardContent>
    </Card>
  );
};
