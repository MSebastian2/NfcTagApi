import api from './client';

export type ReaderRow = {
  id: number; code: string; name?: string; isActive: boolean;
  type: number; // enum in backend
  location?: { id: number; code: string; name: string } | null;
};

export async function listReaders(): Promise<ReaderRow[]> {
  const { data } = await api.get('/readers');
  return data;
}

export async function setReaderActive(code: string, isActive: boolean) {
  await api.patch(`/readers/${encodeURIComponent(code)}/status`, { isActive });
}

export async function getReaderConfig(id: number) {
  const { data } = await api.get(`/admin/readers/${id}/config`);
  return data as { readerCode: string; apiKey: string; apiBase: string; punchEndpoint: string };
}

export async function pushReaderScript(id: number, scriptName: 'gateway'|'door-opener') {
  const { data } = await api.post(`/admin/readers/${id}/push-script`, { scriptName });
  return data;
}

export async function assignReaderLocation(readerCode: string, locationCode?: string, locationId?: number) {
  const { data } = await api.post('/readers/assign-location', { readerCode, locationCode, locationId });
  return data;
}
