# Code Style Rules

## Formatting
- Indent: **4 spaces** (theo `.prettierrc.json`)
- Quotes: **single quotes**
- Semicolons: **bắt buộc**
- Max line width: 250 ký tự (không phải 80)
- Trailing commas: **không**

## Vue Component Conventions
- `<script setup>` — dùng Composition API, không dùng Options API
- `<script setup lang="ts">` — ưu tiên TypeScript cho file mới
- Tên component: PascalCase (`UserSelector.vue`, không phải `user-selector.vue`)
- Props: khai báo với `defineProps<{...}>()` có TypeScript types
- **KHÔNG đặt tên file trùng với component PrimeVue/global** (vd: `Dialog.vue`, `Button.vue`, `Dropdown.vue`, `DataTable.vue`, `Calendar.vue`, `Toast.vue`, `Card.vue`, `Sidebar.vue`, `Menu.vue`, `Tag.vue`...). Vue 3 SFC tự suy `name` từ filename → trong template chính file đó, `<Dialog>` sẽ resolve thành self-reference (đệ quy) thay vì PrimeVue Dialog → leak component instance, prop validation warning dồn dập. Đặt tên có scope: `AddressDialog.vue`, `ProductDialog.vue`, `ConfirmDeleteDialog.vue`.

## Naming Conventions
- **Files/Folders:** PascalCase cho component (`UserSelector/`), camelCase cho helper (`format.helper.js`)
- **Variables:** camelCase
- **Constants:** UPPER_SNAKE_CASE
- **Pinia stores:** file đặt trong `src/Pinia/`, tên store dùng `use<Name>Store()`
- **Composables:** `use<Name>.js` hoặc `use<Name>.ts`

## TypeScript
- File mới: dùng `.ts` / `.vue` với `lang="ts"`
- Không dùng `any` trừ khi bắt buộc (ghi chú lý do)
- Export interfaces từ file riêng (ví dụ `userInfo.interface.ts`)

## Comments
- Không comment những gì code đã tự giải thích
- Chỉ comment khi: workaround bug, constraint ẩn, invariant quan trọng
- Không comment TODO thông thường — dùng GitHub Issues thay thế

## Import Order
1. Vue core (`vue`, `vue-router`, `pinia`)
2. Third-party (`primevue/*`, `axios`, `lodash`)
3. Internal absolute (`@/components/...`, `@/api/...`)
4. Relative (`./`, `../`)
