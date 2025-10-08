export default function TinyTable({ rows }: { rows: any[] }) {
  if (!rows.length) return <div className="text-sm text-gray-500">No data</div>;
  const cols = Object.keys(rows[0]);
  return (
    <div className="text-sm max-h-[420px] overflow-auto border rounded-lg">
      <div className="grid" style={{ gridTemplateColumns: `repeat(${cols.length}, minmax(0,1fr))` }}>
        {cols.map(c => <div key={c} className="px-2 py-1 font-medium border-b bg-gray-50">{c}</div>)}
        {rows.map((r,i) => cols.map(c =>
          <div key={c+'_'+i} className="px-2 py-1 border-b truncate">{String(r[c] ?? '')}</div>
        ))}
      </div>
    </div>
  );
}
