/**
 * SignalR singleton kết nối tới hub `/api/hubs/reference-data`.
 *
 * Backend broadcast `ReferenceDataChanged` với payload `{ entityType, version, timestamp }`
 * mỗi khi admin update master data → frontend invalidate cache local theo entityType.
 *
 * Graceful degradation: nếu hub không sẵn sàng (backend chưa dựng / down / network error),
 * cache ETag vẫn hoạt động bình thường — chỉ mất real-time push.
 *
 * Cấu hình qua env (.env.*):
 *   VITE_REFERENCE_DATA_HUB_ENABLED  — "false" để tắt hoàn toàn (default bật)
 *   VITE_REFERENCE_DATA_HUB_PATH     — override path mặc định "/api/hubs/reference-data"
 */

import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { invalidateByEntityType } from './referenceDataCache';

const HUB_PATH = (import.meta.env.VITE_REFERENCE_DATA_HUB_PATH as string) || '/api/hubs/reference-data';
const HUB_ENABLED = (import.meta.env.VITE_REFERENCE_DATA_HUB_ENABLED as string) !== 'false';

/** Cooldown sau khi connect fail — tránh spam log + retry liên tục khi backend down. */
const FAILURE_COOLDOWN_MS = 5 * 60 * 1000; // 5 phút

interface ReferenceDataChangedPayload {
    entityType: string;
    version?: string;
    timestamp?: string;
}

let _connection: HubConnection | null = null;
let _starting: Promise<void> | null = null;
let _lastFailureAt = 0;
let _failureLogged = false;

function getToken(): string | null {
    try {
        const stored = localStorage.getItem('user');
        return stored ? JSON.parse(stored)?.token ?? null : null;
    } catch {
        return null;
    }
}

function buildConnection(): HubConnection {
    return new HubConnectionBuilder()
        .withUrl(HUB_PATH, {
            accessTokenFactory: () => getToken() ?? ''
        })
        // Không dùng withAutomaticReconnect cho initial connect failure
        // (sẽ tự gọi lại startReferenceDataHub() từ login flow / app boot).
        // Nếu cần auto-reconnect sau khi đã connect thành công thì bật .withAutomaticReconnect()
        // — nhưng sẽ chỉ áp dụng khi connection bị drop, không cứu được ECONNREFUSED ban đầu.
        .withAutomaticReconnect([2000, 10000, 30000])
        .configureLogging(LogLevel.None) // Tự log có chọn lọc bên dưới, không để SignalR spam
        .build();
}

/**
 * Khởi động kết nối hub. Idempotent: gọi nhiều lần chỉ tạo 1 connection.
 * - Bỏ qua nếu chưa có token, hub bị disable, hoặc đang trong cooldown sau lần fail trước.
 * - Lỗi connect được nuốt — app vẫn chạy, chỉ thiếu real-time push.
 */
export async function startReferenceDataHub(): Promise<void> {
    if (!HUB_ENABLED) return;
    if (!getToken()) return;
    if (_connection && _connection.state === HubConnectionState.Connected) return;
    if (_starting) return _starting;

    // Cooldown: nếu vừa fail trong vòng 5 phút, bỏ qua không thử nữa
    if (_lastFailureAt && Date.now() - _lastFailureAt < FAILURE_COOLDOWN_MS) return;

    _connection = buildConnection();

    _connection.on('ReferenceDataChanged', async (payload: ReferenceDataChangedPayload) => {
        if (!payload?.entityType) return;
        const removed = await invalidateByEntityType(payload.entityType);
        if (removed > 0) {
            console.info(`[refHub] invalidated ${removed} entries for "${payload.entityType}"`);
        }
    });

    _starting = _connection
        .start()
        .then(() => {
            _lastFailureAt = 0;
            _failureLogged = false;
            console.info('[refHub] connected');
        })
        .catch(err => {
            _lastFailureAt = Date.now();
            // Chỉ log 1 lần để không spam console khi backend down
            if (!_failureLogged) {
                _failureLogged = true;
                console.info(
                    `[refHub] không kết nối được ${HUB_PATH} — chạy ở chế độ ETag-only ` +
                    `(cache vẫn hoạt động, chỉ thiếu real-time invalidation). ` +
                    `Sẽ thử lại sau ${FAILURE_COOLDOWN_MS / 60000} phút.`
                );
            }
            // Cleanup connection để không giữ resource
            _connection = null;
        })
        .finally(() => {
            _starting = null;
        });

    return _starting;
}

export async function stopReferenceDataHub(): Promise<void> {
    if (!_connection) return;
    try {
        await _connection.stop();
    } catch (err) {
        // Im lặng — stop có thể fail nếu connection chưa kịp connect xong
    } finally {
        _connection = null;
        _lastFailureAt = 0;
        _failureLogged = false;
    }
}
