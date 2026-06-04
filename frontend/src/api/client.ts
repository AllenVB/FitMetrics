import axios, { AxiosError } from 'axios';

const TOKEN_KEY = 'fm_token';

export const tokenStorage = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (token: string) => localStorage.setItem(TOKEN_KEY, token),
  clear: () => localStorage.removeItem(TOKEN_KEY),
};

const client = axios.create({ baseURL: '/api' });

// Her isteğe JWT ekle
client.interceptors.request.use((config) => {
  const token = tokenStorage.get();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// 401'de oturumu temizle ve giriş sayfasına yönlendir
client.interceptors.response.use(
  (res) => res,
  (error: AxiosError) => {
    if (error.response?.status === 401 && tokenStorage.get()) {
      tokenStorage.clear();
      if (!window.location.pathname.startsWith('/login')) {
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  },
);

export interface ApiError {
  status: number;
  message: string;
  errors: string[];
}

export function getErrorMessage(error: unknown): string {
  const err = error as AxiosError<ApiError>;
  const data = err.response?.data;
  if (data) {
    if (data.errors && data.errors.length > 0) return data.errors.join(' • ');
    if (data.message) return data.message;
  }
  return 'Bir hata oluştu. Lütfen tekrar deneyin.';
}

export default client;
