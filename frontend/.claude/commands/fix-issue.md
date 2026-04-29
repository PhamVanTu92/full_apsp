# /project:fix-issue

Fix một issue cụ thể trong APSP project.

## Quy trình

1. **Đọc** mô tả issue (cung cấp issue number hoặc mô tả)
2. **Tìm** file liên quan trong `src/views/`, `src/components/`, `src/Pinia/`
3. **Reproduce** — xác định bước tái hiện bug
4. **Fix** — áp dụng fix nhỏ nhất, không refactor ngoài phạm vi
5. **Verify** — kiểm tra logic và không gây regression

## Lưu ý
- Chỉ sửa những gì cần thiết cho issue — không cleanup xung quanh
- Nếu fix ảnh hưởng Pinia store, kiểm tra tất cả component dùng store đó
- Nếu fix API call, kiểm tra error handling và loading state
- Sau fix, chạy `npm run lint` để đảm bảo code style
