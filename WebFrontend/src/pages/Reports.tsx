import { useEffect, useMemo, useState } from 'react';
import { formatISO, subDays } from 'date-fns';
import { fetchAttendance, fetchSessions } from '../api/reports';
import TinyTable from '../components/TinyTable';

function downloadCSV(filename: string, rows: any[]) {
  if (!rows.length) return;
  const keys = Object.keys(rows[0]);
  const csv = [keys.join(','), ...rows.map(r => keys.map(k => JSON.stringify(r[k] ?? '')).join(','))].join('\n');
  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a'); a.href = url; a.download = filename; a.click();
  URL.revokeObjectURL(url);
}

export default function Reports() {
  const [from, setFrom] = useState(formatISO(subDays(new Date(), 7)));
  const [to, setTo] = useState(formatISO(new Date()));
  const [groupBy, setGroupBy] = useState<'day'|'week'|'month'>('day');
  const [attendance, setAttendance] = useState<any[]>([]);
  const [sessions, setSessions] = useState<any[]>([]);
  const [busy, setBusy] = useState(false);

  async function load(){
    setBusy(true);
    try {
      const a = await fetchAttendance({ fromUtc: from, toUtc: to, groupBy });
      setAttendance(a.Rows || []);
      const s = await fetchSessions({ fromUtc: from, toUtc: to });
      setSessions(s.Rows || []);
    } finally { setBusy(false); }
  }
  useEffect(()=>{ load(); /* eslint-disable-line */ },[]);

  const attendanceView = useMemo(() =>
    (attendance||[]).map((x:any)=>({ Worker: x.WorkerName, Period: x.Period, Hours: x.TotalHours })), [attendance]);

  return (
    <div className="p-6 space-y-6">
      <h1 className="text-2xl font-semibold">Reports</h1>
      <div className="border rounded-xl p-4 grid md:grid-cols-4 gap-3">
        <div>
          <div className="text-xs text-gray-500 mb-1">From (UTC)</div>
          <input className="border rounded px-2 py-1 w-full" value={from} onChange={e=>setFrom(e.target.value)} />
        </div>
        <div>
          <div className="text-xs text-gray-500 mb-1">To (UTC)</div>
          <input className="border rounded px-2 py-1 w-full" value={to} onChange={e=>setTo(e.target.value)} />
        </div>
        <div>
          <div className="text-xs text-gray-500 mb-1">Group by</div>
          <select className="border rounded px-2 py-1 w-full" value={groupBy} onChange={e=>setGroupBy(e.target.value as any)}>
            <option value="day">Day</option><option value="week">Week</option><option value="month">Month</option>
          </select>
        </div>
        <div className="flex items-end gap-2">
          <button onClick={load} className="border rounded px-3 py-1">{busy?'Loading…':'Refresh'}</button>
          <button onClick={()=>downloadCSV('attendance.csv', attendanceView)} className="border rounded px-3 py-1">Export CSV</button>
          <button onClick={()=>window.print()} className="border rounded px-3 py-1">Print</button>
        </div>
      </div>

      <div className="border rounded-xl p-4">
        <div className="font-medium mb-2">Attendance (Aggregated)</div>
        <TinyTable rows={attendanceView} />
      </div>

      <div className="border rounded-xl p-4">
        <div className="font-medium mb-2">Sessions (Detailed)</div>
        <TinyTable rows={sessions} />
      </div>
    </div>
  );
}
