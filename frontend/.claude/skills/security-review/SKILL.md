# Security Review Skill

## Trigger
Tự động kích hoạt khi user đề cập: "security review", "audit bảo mật", "kiểm tra bảo mật", hoặc thêm tính năng auth/payment/permission mới.

## Workflow

1. **Scan auth flow** — `src/middlewares/`, `src/store/auth.module.js`, `src/helpers/auth-header.helper.js`
2. **Scan v-html usage** — tìm `v-html` trong toàn bộ `src/`
3. **Scan localStorage** — kiểm tra những gì được lưu và đọc
4. **Scan API calls** — kiểm tra endpoint nào không có auth header
5. **Scan console.log** — tìm log có thể expose sensitive data

## Output
Báo cáo dùng format của `security-auditor` agent, nhóm theo severity.
Kết thúc bằng danh sách action items theo thứ tự ưu tiên.
