# CI/CD Rules

## Pipeline (.github/workflows/ci.yml)

Mỗi push và pull request vào `main`, `master`, `develop` đều chạy:

| Job | Điều kiện | Nội dung |
|---|---|---|
| **Lint & Test** | Luôn chạy | `npm run lint` + `npm test` |
| **Build dev** | Sau khi lint/test pass | `npm run build:dev` |
| **Build uat** | Sau khi lint/test pass | `npm run build:uat` |

Build artifact được lưu 3 ngày trên GitHub Actions.

## Quy tắc trước khi tạo PR

1. **Lint phải pass:** `npm run lint -- --max-warnings=0`
2. **Tests phải pass:** `npm test` — 35/35 minimum
3. **Build phải thành công:** `npm run build:dev`
4. Không push trực tiếp lên `main`/`master` — dùng PR

## Khi thêm feature mới
- Nếu thêm helper/composable/store mới → viết test đi kèm
- Nếu thêm `app.component()` global → đảm bảo là custom component (PrimeVue auto-import qua PrimeVueResolver)
- Nếu thêm route mới → đặt trong `src/router/index.js` với lazy-load

## Môi trường
- **Node:** 18 (`.nvmrc`)
- **CI runner:** ubuntu-latest
- `npm ci` thay vì `npm install` — đảm bảo lock file được tôn trọng
