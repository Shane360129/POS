import axios from 'axios';
import { useAuthStore } from '../auth/authStore';

// 統一 API client。OpenAPI 產生的 client 之後可共用此實例的攔截器設定。
export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '/api',
});

// 注入 Keycloak token、目前公司、（寫入時）樂觀鎖 If-Match。
api.interceptors.request.use((config) => {
  const { token, companyId } = useAuthStore.getState();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  if (companyId != null) config.headers['X-Company-Id'] = String(companyId);
  return config;
});
