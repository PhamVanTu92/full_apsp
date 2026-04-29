import axios, { AxiosError, AxiosResponse } from 'axios';
import { authHeader_new } from '@/helpers/auth-header.helper';
import { getCached, saveCache, matchCacheable } from '@/services/referenceDataCache';

const API_URL = import.meta.env.VITE_APP_API as string;

type Headers = Record<string, string>;

// ─── ApiError — envelope error từ backend ────────────────────────────────────
export class ApiError extends Error {
    readonly statusCode: number;
    readonly code: string;
    readonly errors: string[] | null;
    readonly traceId: string | null;

    constructor(envelope: {
        statusCode: number;
        code: string;
        message: string;
        errors?: string[] | null;
        traceId?: string | null;
    }) {
        super(envelope.message);
        this.name = 'ApiError';
        this.statusCode = envelope.statusCode;
        this.code = envelope.code;
        this.errors = envelope.errors ?? null;
        this.traceId = envelope.traceId ?? null;
    }
}
// ─────────────────────────────────────────────────────────────────────────────

// ─── Refresh-token interceptor ────────────────────────────────────────────────
let _isRefreshing = false;
let _failedQueue: Array<{ resolve: (token: string) => void; reject: (err: unknown) => void }> = [];

function _flushQueue(error: unknown, token: string | null = null): void {
    _failedQueue.forEach(p => (error ? p.reject(error) : p.resolve(token!)));
    _failedQueue = [];
}

axios.interceptors.response.use(
    response => response,
    async (error: AxiosError) => {
        const config = error.config as any;

        // Bỏ qua nếu không phải 401, hoặc là chính request refresh/login
        if (
            error.response?.status !== 401 ||
            config?._retry ||
            config?.url?.includes('Account/refresh-token') ||
            config?.url?.includes('account/login')
        ) {
            return Promise.reject(error);
        }

        // Nếu đang refresh, đưa request vào hàng đợi
        if (_isRefreshing) {
            return new Promise<string>((resolve, reject) => {
                _failedQueue.push({ resolve, reject });
            }).then(token => {
                config.headers.Authorization = `Bearer ${token}`;
                return axios(config);
            });
        }

        config._retry = true;
        _isRefreshing = true;

        try {
            const stored = localStorage.getItem('user');
            const user = stored ? JSON.parse(stored) : null;
            if (!user?.refreshToken) throw new Error('no-refresh-token');

            const { data } = await axios.post<{ token: string; refreshToken: string }>(
                `${API_URL}Account/refresh-token`,
                { refreshToken: user.refreshToken }
            );

            // Cập nhật token mới vào localStorage
            user.token = data.token;
            user.refreshToken = data.refreshToken;
            localStorage.setItem('user', JSON.stringify(user));

            _flushQueue(null, data.token);
            config.headers.Authorization = `Bearer ${data.token}`;
            return axios(config);
        } catch (refreshError) {
            _flushQueue(refreshError);
            localStorage.removeItem('user');
            location.replace('/login');
            return Promise.reject(refreshError);
        } finally {
            _isRefreshing = false;
        }
    }
);
// ─────────────────────────────────────────────────────────────────────────────

// ─── Envelope unwrap interceptor ─────────────────────────────────────────────
// Đăng ký SAU 401 interceptor → chạy TRƯỚC theo LIFO của Axios.
// Success: bóc res.data.data → res.data (tương thích cả endpoint chưa wrap).
// Error  : 401/403 pass-through cho interceptor 401 & handleAxiosError xử lý;
//          các status khác có envelope → ném ApiError để component bắt trực tiếp.
axios.interceptors.response.use(
    response => {
        const env = response.data as any;
        if (env && typeof env === 'object' && 'success' in env) {
            response.data = env.data ?? null;
        }
        return response;
    },
    (error: AxiosError) => {
        const status = error.response?.status;
        // Nhường cho refresh-token interceptor (401) và global redirect (403)
        if (status === 401 || status === 403) {
            return Promise.reject(error);
        }
        const env = error.response?.data as any;
        if (env && typeof env === 'object' && 'success' in env && env.success === false) {
            return Promise.reject(new ApiError(env));
        }
        return Promise.reject(error);
    }
);
// ─────────────────────────────────────────────────────────────────────────────

function handleAxiosError(error: unknown): never {
    if (axios.isAxiosError(error)) {
        const axiosErr = error as AxiosError;
        if (axiosErr.response?.status === 403) location.replace('/access-denied');
        else if (axiosErr.code === 'ECONNABORTED') console.error('[API] Request timed out:', axiosErr.config?.url);
        else if (axiosErr.code === 'ERR_EMPTY_RESPONSE' || axiosErr.code === 'ERR_NETWORK') console.error('[API] Server closed connection (empty response):', axiosErr.config?.url);
        else console.error('[API] Error:', axiosErr.response?.status, axiosErr.config?.url, axiosErr.message);
    }
    throw error;
}

// ─── In-flight GET deduplication ─────────────────────────────────────────────
// Nhiều component mount cùng lúc gọi cùng endpoint (vd: Account/getall) → chỉ
// gửi 1 HTTP request, tất cả caller nhận cùng 1 Promise.
const _inflight = new Map<string, Promise<AxiosResponse<any>>>();
// ─────────────────────────────────────────────────────────────────────────────

class API {
    private TokenString: () => Headers;

    constructor() {
        this.TokenString = authHeader_new;
    }

    async get<T = unknown>(path: string): Promise<AxiosResponse<T>> {
        const endpoint = API_URL + path;
        if (_inflight.has(endpoint)) {
            return _inflight.get(endpoint) as Promise<AxiosResponse<T>>;
        }
        const cacheable = matchCacheable(path);
        const promise = (cacheable
            ? this._cachedGet<T>(path, endpoint, cacheable.entityType)
            : axios.get<T>(endpoint, { headers: this.TokenString(), timeout: 15000 }))
            .catch((error: unknown) => handleAxiosError(error))
            .finally(() => _inflight.delete(endpoint)) as Promise<AxiosResponse<T>>;
        _inflight.set(endpoint, promise);
        return promise;
    }

    /**
     * GET có ETag/304 cho master data. Lưu data + ETag vào IndexedDB,
     * lần sau gửi `If-None-Match`. Server trả 304 → restore từ cache.
     * SignalR `ReferenceDataChanged` sẽ invalidate entry tương ứng (xem referenceDataHub.ts).
     */
    private async _cachedGet<T>(path: string, endpoint: string, entityType: string): Promise<AxiosResponse<T>> {
        const cached = await getCached<T>(path);
        const headers: Headers = { ...this.TokenString() };
        if (cached?.etag) headers['If-None-Match'] = cached.etag;

        const res = await axios.get<T>(endpoint, {
            headers,
            timeout: 15000,
            // Chấp nhận 304 là response hợp lệ thay vì throw
            validateStatus: status => status === 304 || (status >= 200 && status < 300)
        });

        if (res.status === 304) {
            // Server xác nhận data chưa đổi — restore body từ cache
            if (cached) {
                res.data = cached.data;
                (res as AxiosResponse).status = 200;
            }
            return res;
        }

        // 200: lưu data + ETag mới (response đã unwrap envelope qua interceptor)
        const etag = (res.headers?.etag ?? res.headers?.ETag) as string | undefined;
        if (etag) {
            await saveCache({
                key: path,
                data: res.data,
                etag,
                entityType,
                savedAt: Date.now()
            });
        }
        return res;
    }

    async getNoToken<T = unknown>(path: string): Promise<AxiosResponse<T>> {
        const endpoint = API_URL + path;
        try {
            return await axios.get<T>(endpoint, {});
        } catch (error) {
            return handleAxiosError(error);
        }
    }

    async add<T = unknown>(path: string, data?: unknown): Promise<AxiosResponse<T>> {
        const endpoint = API_URL + path;
        const timeout = /purchase(Order|Request)/i.test(path) ? 120000 : 15000;
        const headers = this.TokenString();
        if (/^notifications\/send$/i.test(path) && typeof data === 'string') {
            headers['Content-Type'] = 'application/json';
        }
        try {
            return await axios.post<T>(endpoint, data, { headers, timeout });
        } catch (error) {
            return handleAxiosError(error);
        }
    }

    async update<T = unknown>(path: string, params?: unknown): Promise<AxiosResponse<T>> {
        const endpoint = API_URL + path;
        try {
            return await axios.put<T>(endpoint, params, { headers: this.TokenString(), timeout: 15000 });
        } catch (error) {
            return handleAxiosError(error);
        }
    }

    async delete<T = unknown>(path: string, params?: unknown): Promise<AxiosResponse<T>> {
        const endpoint = API_URL + path;
        try {
            return await axios.delete<T>(endpoint, {
                headers: this.TokenString(),
                data: params,
                timeout: 15000
            });
        } catch (error) {
            return handleAxiosError(error);
        }
    }

    async patch<T = unknown>(path: string, payload: unknown = null): Promise<AxiosResponse<T>> {
        const endpoint = API_URL + path;
        try {
            return await axios.patch<T>(endpoint, payload, { headers: this.TokenString(), timeout: 15000 });
        } catch (error) {
            return handleAxiosError(error);
        }
    }
}

export default new API();
