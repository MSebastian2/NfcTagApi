import { useEffect, useState } from 'react';
import Papa from 'papaparse';
import FileDrop from '../components/FileDrop';
import { listWorkers, createWorker, deleteWorker, bulkUpsertWorkers, bulkDeleteWorkers } from '../api/workers';
import type { WorkerRow } from '../api/workers';

type CsvRow = { FullName: string; TagUid?: string; IsActive?: string | boolean; };
function bool(v:any){ if(typeof v==='boolean') return v; const s=String(v||'').trim().toLowerCase(); return ['1','true','yes','y'].includes(s); }

export default function WorkersBulk() {
  const [rows, setRows] = useState<WorkerRow[]>([]);
  const [csvPreview, setCsvPreview] = useState<CsvRow[]>([]);
  const [selected, setSelected] = useState<Set<number>>(new Set());
  const [busy, setBusy] = useState(false);
  const [msg, setMsg] = useState<string | null>(null);

  async function load(){ setRows(await listWorkers()); }
  useEffect(()=>{ load(); },[]);

  function onFile(f: File){
    Papa.parse<CsvRow>(f,{ header:true, skipEmptyLines:true, complete:(res)=>{
      const data=(res.data||[]).map(x=>({
        FullName: (x as any).FullName ?? (x as any).fullName ?? (x as any).full_name ?? '',
        TagUid: (x as any).TagUid ?? (x as any).tagUid ?? (x as any).uid ?? '',
        IsActive: bool((x as any).IsActive ?? (x as any).isActive),
      })).filter(x=>String(x.FullName).trim().length>0);
      setCsvPreview(data);
    }});
  }

  async function doImport(){
    if(!csvPreview.length) return;
    setBusy(true); setMsg(null);
    try{
      try{
        const r = await bulkUpsertWorkers(csvPreview.map(x=>({ fullName:String(x.FullName).trim(), tagUid:x.TagUid||undefined, isActive:Boolean(x.IsActive) })));
        setMsg(`Imported: ${r.created} created, ${r.updated} updated, ${r.failed.length} failed`);
      }catch{
        let created=0, failed=0;
        for (const x of csvPreview) {
          try { await createWorker({ fullName:String(x.FullName).trim(), tagUid:x.TagUid||undefined, isActive:Boolean(x.IsActive) }); created++; }
          catch { failed++; }
        }
        setMsg(`Imported (fallback): ${created} created, ${failed} failed`);
      }
      await load();
    } finally { setBusy(false); }
  }

  async function doDelete(){
    if (!selected.size) return;
    setBusy(true); setMsg(null);
    const ids = Array.from(selected);
    try{
      try{
        const r = await bulkDeleteWorkers(ids);
        setMsg(`Deleted: ${r.deletedCount}`);
      }catch{
        let d=0,f=0; for (const id of ids){ try{ await deleteWorker(id); d++; } catch { f++; } }
        setMsg(`Deleted (fallback): ${d} deleted, ${f} failed`);
      }
      setSelected(new Set()); await load();
    } finally { setBusy(false); }
  }

  function toggle(id:number){ const s=new Set(selected); s.has(id)?s.delete(id):s.add(id); setSelected(s); }

  return (
    <div className="p-6 space-y-6">
      <h1 className="text-2xl font-semibold">Workers · Bulk</h1>
      <div className="grid md:grid-cols-2 gap-6">
        <div className="border rounded-xl p-4">
          <div className="font-medium mb-2">Import CSV</div>
          <FileDrop onFile={onFile} />
          {csvPreview.length>0 && (
            <>
              <div className="text-sm text-gray-600 mt-3 mb-1">Preview ({csvPreview.length} rows)</div>
              <div className="max-h-56 overflow-auto text-xs border rounded">
                {csvPreview.map((r,i)=>(
                  <div key={i} className="grid grid-cols-3 gap-2 px-2 py-1 border-b">
                    <div className="truncate">{r.FullName}</div>
                    <div className="truncate">{r.TagUid||'—'}</div>
                    <div>{r.IsActive?'Active':'Inactive'}</div>
                  </div>
                ))}
              </div>
              <button disabled={busy} onClick={doImport} className="mt-3 border rounded px-3 py-1 text-sm">{busy?'Importing…':'Import'}</button>
            </>
          )}
        </div>

        <div className="border rounded-xl p-4">
          <div className="font-medium mb-2">Existing Workers</div>
          <div className="max-h-72 overflow-auto space-y-1">
            {rows.map(w=>(
              <label key={w.id} className="flex items-center gap-2 border rounded p-2">
                <input type="checkbox" checked={selected.has(w.id)} onChange={()=>toggle(w.id)} />
                <div className="flex-1">
                  <div className="text-sm font-medium">{w.fullName}</div>
                  <div className="text-xs text-gray-500">{w.tagUid || 'No Tag'} · {w.isActive?'Active':'Inactive'}{w.openSession?' · OPEN':''}</div>
                </div>
              </label>
            ))}
          </div>
          <button disabled={busy || !selected.size} onClick={doDelete} className="mt-3 border rounded px-3 py-1 text-sm">
            {busy?'Deleting…':`Delete Selected (${selected.size})`}
          </button>
        </div>
      </div>
      {msg && <div className="text-sm text-green-700">{msg}</div>}
    </div>
  );
}
