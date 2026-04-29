# /project:deploy

Hướng dẫn build và deploy APSP Frontend.

## Build theo môi trường

```bash
# Development
npm run build:dev

# UAT
npm run build:uat

# Production
npm run product
```

## Checklist trước khi deploy

### Pre-build
- [ ] `npm run lint` — không có lỗi ESLint
- [ ] Kiểm tra `.env.*` đúng môi trường target
- [ ] Không có `console.log` debug trong commit
- [ ] Version trong `package.json` đã cập nhật (nếu cần)

### Post-build
- [ ] Thư mục `dist/` được tạo thành công
- [ ] Không có build error/warning nghiêm trọng
- [ ] File `index.html` có trong `dist/`

## Deploy targets
- **Vercel:** tự động qua `vercel.json` (SPA rewrite đã cấu hình)
- **IIS:** upload `dist/` lên server, `web.config` đã có sẵn

## Rollback
Giữ lại bản build trước trong `dist_backup/` trước khi deploy production.
