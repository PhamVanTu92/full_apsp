# API Conventions

## API Layer Structure
- **`src/api/api-main.js`** — axios instance chính với auth headers
- **`src/api/apis/api.js`** — API helper functions
- Auth header được inject tự động qua `src/helpers/auth-header.helper.js`

## Gọi API đúng cách
```js
// ĐÚNG — dùng helper đã có
import { apiMain } from '@/api/api-main'

const result = await apiMain.get('/endpoint', { params })
```

```js
// SAI — không tạo axios instance mới hay hardcode URL
import axios from 'axios'
const result = await axios.get('https://...')
```

## Error Handling
- 403 Forbidden → tự động redirect `/access-denied` (đã xử lý ở interceptor)
- Các lỗi khác → hiển thị qua Notification service:
```js
import { useNotification } from '@/services/Notification'
const notify = useNotification()
notify.error('Có lỗi xảy ra')
```

## Environments
| Env | File |
|---|---|
| Development | `.env.development` |
| UAT | `.env.uat` |
| Production | `.env.production` |

- Biến env đọc qua `import.meta.env.VITE_API_URL`
- KHÔNG hardcode URL vào component hay service

## Authentication
- Token lưu localStorage key: `access-token`
- Token expiry: `access-exp` (ms)
- Refresh token tự động khi token hết hạn
- Dùng `auth-header.helper.js` để đính kèm Authorization header

## Reference Data Cache (master data)
- Một số GET endpoint master data được cache **tự động** trong IndexedDB qua `API.get()`:
  - `brand/getall`, `industry/getall`, `Industry/getallHiarchy`
  - `ItemGroup`, `BPSize/getall`, `BPArea`
  - `regions?skip=0&limit=10000`, `appsetting`
- Cơ chế:
  1. Client gửi `If-None-Match: <etag>` từ entry cũ
  2. Backend trả 304 (chưa đổi) → restore data từ cache, không tốn DB
  3. Khi admin update master data, backend broadcast SignalR `ReferenceDataChanged` → client invalidate entry tương ứng
- Whitelist URL → entityType nằm trong `src/services/referenceDataCache.ts` (`CACHEABLE_PATTERNS`)
- **Khi thêm endpoint master data mới**: thêm pattern vào `CACHEABLE_PATTERNS` + đảm bảo backend phát SignalR cùng `entityType`
- Component **không cần thay đổi gì** — chỉ gọi `API.get(path)` như cũ, cache tự hoạt động khi path khớp whitelist

## Response Conventions
- Success: `{ data: {...}, success: true }`
- Error: `{ message: '...', success: false }`
- Paginated: `{ data: [...], total: N, page: N, pageSize: N }`
