# APSP Frontend — Saigon Petro Procurement System

## Project Overview
Hệ thống quản lý mua sắm nội bộ của Saigon Petro. Vue 3 + Vite + PrimeVue. Hỗ trợ tiếng Việt/Anh.

- **Backend API:** Xem `.env.*` để lấy URL theo môi trường
- **Cổng dev server:** `0.0.0.0:1072`
- **Node version:** 18+

## Tech Stack
| Layer | Technology |
|---|---|
| Framework | Vue 3 (Composition API) |
| Build | Vite 4 |
| UI | PrimeVue 3 + PrimeFlex + TailwindCSS |
| State | Pinia (primary), Vuex (legacy — đang loại bỏ) |
| HTTP | Axios với JWT token |
| Realtime | Microsoft SignalR |
| i18n | Vue I18n (vi, en) |
| Export | ExcelJS, jsPDF |
| Lang | TypeScript (một phần) + JavaScript |

## Build Commands
```bash
npm run dev          # dev server
npm run build:dev    # build môi trường dev
npm run build:uat    # build môi trường UAT
npm run product      # build production
npm run lint         # ESLint auto-fix
```

## Quy ước quan trọng
- Xem `.claude/rules/code-style.md` — code style và naming
- Xem `.claude/rules/api-conventions.md` — cách gọi API
- Xem `.claude/rules/testing.md` — hướng dẫn viết test
- Xem `.claude/rules/state-management.md` — dùng Pinia, không dùng Vuex mới
- Xem `.claude/rules/ci-cd.md` — pipeline rules, checklist trước PR

## Cấu trúc `src/`
```
src/
├── api/           # Axios wrappers (api-main.js)
├── components/    # 50+ component tái sử dụng
├── composables/   # Vue 3 composable functions
├── helpers/       # Utility functions
├── i18n/          # Bản dịch vi.json / en.json
├── layout/        # AppLayout, sidebar, topbar
├── middlewares/   # Auth / Authorization route guards
├── Pinia/         # Pinia stores (nguồn state chính)
├── router/        # Vue Router với lazy-loading
├── services/      # Business logic services
├── store/         # Vuex legacy (KHÔNG thêm mới)
├── utils/         # TypeScript utilities
└── views/         # Page components (36 admin modules)
```

## Lưu ý bảo mật
- JWT lưu trong `localStorage` — KHÔNG log token ra console
- Không hardcode URL hay credentials vào code
- Kiểm tra auth middleware trước khi thêm route mới
