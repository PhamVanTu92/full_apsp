# Deploy Skill

## Trigger
Kích hoạt khi user nói: "deploy", "build production", "release", "đẩy lên server".

## Workflow

1. **Pre-flight check**
   - Chạy `npm run lint` — dừng nếu có lỗi
   - Kiểm tra không có `console.log` debug trong staged files
   - Xác nhận môi trường target (dev/uat/production)

2. **Build**
   - dev: `npm run build:dev`
   - UAT: `npm run build:uat`
   - Production: `npm run product`

3. **Verify output**
   - Kiểm tra `dist/` tồn tại
   - Kiểm tra `dist/index.html` có
   - Báo cáo bundle size

4. **Deploy guidance**
   - Vercel: hướng dẫn upload `dist/`
   - IIS: nhắc nhở copy `web.config` cùng `dist/`

## Safety Rules
- KHÔNG tự push lên production mà không có xác nhận từ user
- Luôn hỏi môi trường target trước khi build
