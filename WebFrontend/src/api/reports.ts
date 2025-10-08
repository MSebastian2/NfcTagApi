import api from './client';

export async function fetchAttendance(input: {
  fromUtc?: string; toUtc?: string; workerId?: number; locationId?: number; readerCode?: string;
  groupBy?: 'day'|'week'|'month';
}) {
  const { data } = await api.post('/reports/attendance', input);
  return data as { Rows: Array<{ WorkerId: number; WorkerName: string; Period: string; TotalHours: number; }> };
}

export async function fetchSessions(input: {
  fromUtc?: string; toUtc?: string; workerId?: number; locationId?: number; readerCode?: string;
}) {
  const { data } = await api.post('/reports/sessions', input);
  return data as { Rows: Array<any> };
}
