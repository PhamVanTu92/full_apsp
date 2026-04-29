/**
 * Reference data cache — lưu response của các GET endpoint master data
 * vào IndexedDB cùng ETag để gửi lên server qua header `If-None-Match`.
 *
 * Backend trả 304 Not Modified → restore data từ cache (không tốn DB query).
 * SignalR hub `ReferenceDataChanged` → invalidate theo entityType.
 */

const DB_NAME = 'apsp-reference-data';
const STORE_NAME = 'entries';
const DB_VERSION = 1;

export interface CacheEntry<T = unknown> {
    /** Key = path (đã chuẩn hoá, tính cả query string) */
    key: string;
    /** Body đã unwrap envelope (theo interceptor trong api-main.ts) */
    data: T;
    /** ETag từ response header (raw, gồm cả dấu nháy nếu server trả) */
    etag: string;
    /** Loại entity để invalidate khi nhận event SignalR */
    entityType: string;
    /** Thời điểm lưu — debug + có thể dùng để stale-while-revalidate */
    savedAt: number;
}

/**
 * Whitelist các endpoint dùng cache + map sang entityType backend phát qua SignalR.
 * Key match theo regex (case-insensitive) trên `path` (không có domain, có query).
 *
 * Khi backend phát event với `entityType`, mọi entry có cùng entityType bị xoá.
 */
const CACHEABLE_PATTERNS: Array<{ pattern: RegExp; entityType: string }> = [
    { pattern: /^brand\/getall$/i, entityType: 'brand' },
    { pattern: /^industry\/getall$/i, entityType: 'industry' },
    { pattern: /^Industry\/getallHiarchy$/i, entityType: 'industry' },
    { pattern: /^ItemGroup\b/i, entityType: 'itemGroup' },
    { pattern: /^BPSize\/getall$/i, entityType: 'bpSize' },
    { pattern: /^BPArea\b/i, entityType: 'bpArea' },
    { pattern: /^regions\?/i, entityType: 'regions' },
    { pattern: /^appsetting\b/i, entityType: 'appsetting' }
];

export function matchCacheable(path: string): { entityType: string } | null {
    for (const { pattern, entityType } of CACHEABLE_PATTERNS) {
        if (pattern.test(path)) return { entityType };
    }
    return null;
}

let _dbPromise: Promise<IDBDatabase> | null = null;

function openDb(): Promise<IDBDatabase> {
    if (_dbPromise) return _dbPromise;
    _dbPromise = new Promise((resolve, reject) => {
        const req = indexedDB.open(DB_NAME, DB_VERSION);
        req.onupgradeneeded = () => {
            const db = req.result;
            if (!db.objectStoreNames.contains(STORE_NAME)) {
                const store = db.createObjectStore(STORE_NAME, { keyPath: 'key' });
                store.createIndex('entityType', 'entityType', { unique: false });
            }
        };
        req.onsuccess = () => resolve(req.result);
        req.onerror = () => reject(req.error);
    });
    return _dbPromise;
}

async function tx<T>(mode: IDBTransactionMode, fn: (store: IDBObjectStore) => IDBRequest<T> | Promise<T>): Promise<T> {
    const db = await openDb();
    return new Promise<T>((resolve, reject) => {
        const transaction = db.transaction(STORE_NAME, mode);
        const store = transaction.objectStore(STORE_NAME);
        const result = fn(store);
        if (result instanceof IDBRequest) {
            result.onsuccess = () => resolve(result.result as T);
            result.onerror = () => reject(result.error);
        } else {
            // For async iterations (delete-by-index)
            Promise.resolve(result).then(resolve, reject);
        }
        transaction.onerror = () => reject(transaction.error);
    });
}

export async function getCached<T = unknown>(key: string): Promise<CacheEntry<T> | null> {
    try {
        const entry = await tx<CacheEntry<T> | undefined>('readonly', store => store.get(key) as IDBRequest<CacheEntry<T> | undefined>);
        return entry ?? null;
    } catch (err) {
        console.warn('[refCache] getCached failed', key, err);
        return null;
    }
}

export async function saveCache(entry: CacheEntry): Promise<void> {
    try {
        await tx<IDBValidKey>('readwrite', store => store.put(entry));
    } catch (err) {
        console.warn('[refCache] saveCache failed', entry.key, err);
    }
}

export async function invalidateByEntityType(entityType: string): Promise<number> {
    const db = await openDb();
    return new Promise<number>((resolve, reject) => {
        const transaction = db.transaction(STORE_NAME, 'readwrite');
        const store = transaction.objectStore(STORE_NAME);
        const index = store.index('entityType');
        const cursorReq = index.openCursor(IDBKeyRange.only(entityType));
        let count = 0;
        cursorReq.onsuccess = () => {
            const cursor = cursorReq.result;
            if (cursor) {
                cursor.delete();
                count++;
                cursor.continue();
            } else {
                resolve(count);
            }
        };
        cursorReq.onerror = () => reject(cursorReq.error);
    });
}

export async function clearAllCache(): Promise<void> {
    try {
        await tx<undefined>('readwrite', store => store.clear());
    } catch (err) {
        console.warn('[refCache] clearAllCache failed', err);
    }
}
