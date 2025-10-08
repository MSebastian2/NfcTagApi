import { useEffect, useState } from 'react';
import { listReaders, getReaderConfig, setReaderActive, pushReaderScript, assignReaderLocation } from '../api/readers';
import type { ReaderRow } from '../api/readers';


export default function ReadersAutomation() {
  const [rows, setRows] = useState<ReaderRow[]>([]);
  const [selected, setSelected] = useState<ReaderRow | null>(null);
  const [config, setConfig] = useState<null | { readerCode: string; apiKey: string; apiBase: string; punchEndpoint: string }>(null);
  const [locCode, setLocCode] = useState('');
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  async function load(){ setRows(await listReaders()); }
  useEffect(()=>{ load(); },[]);

  async function pick(r: ReaderRow) {
    setSelected(r); setConfig(null); setErr(null);
    try { setLoading(true); setConfig(await getReaderConfig(r.id)); }
    catch (e:any) { setErr(e?.response?.data?.message || e.message); }
    finally { setLoading(false); }
  }

  async function toggleActive(r: ReaderRow){ await setReaderActive(r.code, !r.isActive); await load(); }
  async function push(name:'gateway'|'door-opener'){ if(!selected) return; setLoading(true);
    try{ await pushReaderScript(selected.id, name); alert('Script push requested (check device).'); }
    catch(e:any){ alert(e?.response?.data?.message || e.message); }
    finally{ setLoading(false); }
  }
  async function attach(){
    if (!selected) return;
    if (!locCode.trim()) { alert('Enter location code'); return; }
    await assignReaderLocation(selected.code, locCode.trim());
    setLocCode(''); await load();
  }

  return (
    <div className="p-6 space-y-6">
      <h1 className="text-2xl font-semibold">Readers · Automation</h1>
      <div className="grid lg:grid-cols-2 gap-6">
        <div className="border rounded-xl p-4">
          <div className="font-medium mb-2">Readers</div>
          <div className="space-y-2 max-h-[420px] overflow-auto">
            {rows.map(r=>(
              <div key={r.id} className={`flex items-center justify-between border rounded-lg p-3 ${selected?.id===r.id?'bg-gray-50':''}`}>
                <div>
                  <div className="font-medium">{r.code} <span className="text-gray-500">({r.name || '—'})</span></div>
                  <div className="text-xs text-gray-500">{r.location ? `${r.location.code} · ${r.location.name}` : 'Unassigned'}</div>
                </div>
                <div className="flex items-center gap-2">
                  <button onClick={()=>toggleActive(r)} className="text-sm border rounded px-3 py-1">{r.isActive?'Deactivate':'Activate'}</button>
                  <button onClick={()=>pick(r)} className="text-sm border rounded px-3 py-1">Pair / Config</button>
                </div>
              </div>
            ))}
          </div>
          {selected && (
            <div className="mt-4 flex gap-2">
              <input className="border rounded px-2 py-1 text-sm" placeholder="Location code (e.g. LAB)" value={locCode} onChange={e=>setLocCode(e.target.value)} />
              <button onClick={attach} className="text-sm border rounded px-3 py-1">Assign Location</button>
            </div>
          )}
        </div>

        <div className="border rounded-xl p-4">
          <div className="font-medium mb-2">Provisioning</div>
          {loading && <div className="text-sm text-gray-500">Loading…</div>}
          {err && <div className="text-sm text-red-600">{err}</div>}
          {!selected && <div className="text-sm text-gray-500">Select a reader to view config</div>}
          {selected && config && (
            <div className="space-y-4">
              <div className="text-sm">
                <div><b>Reader:</b> {selected.code}</div>
                <div><b>Endpoint:</b> {config.punchEndpoint}</div>
              </div>
              <div>
                <div className="text-sm font-medium mb-1">Gateway ENV</div>
                <pre className="bg-gray-100 rounded p-3 text-xs overflow-auto">
{`READER_CODE=${config.readerCode}
READER_API_KEY=${config.apiKey}
API_BASE=${config.apiBase}
PUNCH_URL=${config.punchEndpoint}`}
                </pre>
              </div>
              <div>
                <div className="text-sm font-medium mb-1">cURL test</div>
                <pre className="bg-gray-100 rounded p-3 text-xs overflow-auto">
{`curl -X POST "${config.punchEndpoint}" ^
  -H "Authorization: Bearer ${config.apiKey}" ^
  -H "Content-Type: application/json" ^
  -d "{""TagUid"":""04AABBCCDD22"",""ReaderCode"":""${config.readerCode}""}"`}
                </pre>
              </div>
              <div className="flex gap-2">
                <button onClick={()=>push('gateway')} className="border rounded px-3 py-1 text-sm">Push Gateway Script</button>
                <button onClick={()=>push('door-opener')} className="border rounded px-3 py-1 text-sm">Push Door-Opener</button>
              </div>
              <div className="text-xs text-gray-500">
                Protect `/admin/*` on the backend. Don’t leak ApiKeys to unauthenticated users.
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
