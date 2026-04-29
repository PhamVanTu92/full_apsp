# /project:review

Review code changes trong branch hiện tại theo tiêu chí của APSP project.

## Checklist khi review

### Code Quality
- [ ] Không dùng `any` TypeScript không có lý do
- [ ] Không tạo axios instance mới (phải dùng `api-main.js`)
- [ ] Không hardcode URL hoặc credentials
- [ ] Import order đúng theo `.claude/rules/code-style.md`

### State Management
- [ ] State mới dùng Pinia, không dùng Vuex
- [ ] Store naming đúng convention (`use<Name>Store`)

### Vue Best Practices
- [ ] Dùng `<script setup>` cho component mới
- [ ] Key dùng ID thực, không dùng index trong `v-for`
- [ ] Không có `console.log` hay debug code

### Security
- [ ] Không log token hay sensitive data
- [ ] Input từ user được validate trước khi gửi API
- [ ] Route mới có khai báo auth middleware

### i18n
- [ ] Text hiển thị dùng `$t('key')`, không hardcode string tiếng Việt/Anh

## Output
Tổng hợp findings theo 3 mức: **Critical** / **Warning** / **Suggestion**
