import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5253',
  timeout: 15000,
});

api.interceptors.response.use(
  r => r,
  err => {
    const msg = err?.response?.data?.message || err?.message || 'Request failed';
    console.error('[api] error', msg, err?.response);
    return Promise.reject(err);
  }
);

export default api;
