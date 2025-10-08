import { useRef } from 'react';

export default function FileDrop({ onFile }: { onFile: (f: File) => void }) {
  const ref = useRef<HTMLInputElement>(null);
  return (
    <div className="border-2 border-dashed rounded-lg p-6 text-center cursor-pointer hover:bg-gray-50"
         onClick={() => ref.current?.click()}>
      <div className="text-sm text-gray-600">Click to select CSV (FullName, TagUid, IsActive)</div>
      <input ref={ref} type="file" accept=".csv" hidden onChange={e => {
        const f = e.target.files?.[0]; if (f) onFile(f);
      }} />
    </div>
  );
}
