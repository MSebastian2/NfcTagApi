import api from './client';

export type WorkerRow = { id: number; fullName: string; tagUid?: string; isActive: boolean; openSession?: boolean; };

export async function listWorkers(): Promise<WorkerRow[]> {
  const { data } = await api.get('/workers');
  return data;
}

export async function createWorker(input: { fullName: string; tagUid?: string; isActive?: boolean; }) {
  const { data } = await api.post('/workers', input);
  return data;
}

export async function deleteWorker(id: number) {
  await api.delete(`/workers/${id}`);
}

// Preferred bulk endpoints if you add them; UI falls back if missing
export async function bulkUpsertWorkers(items: Array<{ fullName: string; tagUid?: string; isActive?: boolean; }>) {
  const { data } = await api.post('/admin/workers/bulk', { items });
  return data as { created: number; updated: number; failed: { row: number; error: string; }[] };
}

export async function bulkDeleteWorkers(ids: number[]) {
  const { data } = await api.post('/admin/workers/bulk-delete', { ids });
  return data as { deletedCount: number };
}
