# Nguyên tắc chung khi sửa code

File này định nghĩa các quy tắc bắt buộc tuân thủ cho MỌI task sửa đổi code trong project. Đọc và áp dụng cho mọi prompt sửa code, không cần được nhắc lại trong từng prompt cụ thể.

## 1. Quy tắc về branch

Trước khi bắt đầu bất kỳ thay đổi nào:

- Tạo branch mới tên `refactor/[ten-task]` từ `main`
- Tên task viết bằng kebab-case, ngắn gọn, mô tả đúng nội dung (ví dụ: `refactor/fix-async-backend`, `refactor/split-large-components`)
- KHÔNG commit trực tiếp lên `main`
- KHÔNG làm nhiều task không liên quan trên cùng một branch

## 2. Quy tắc về commit

- Sau mỗi nhóm sửa logic-related, commit với message rõ ràng theo Conventional Commits:
  - `feat:` thêm tính năng mới
  - `fix:` sửa bug
  - `refactor:` refactor không đổi behavior
  - `perf:` cải thiện performance
  - `test:` thêm/sửa test
  - `docs:` sửa tài liệu
  - `chore:` việc lặt vặt (config, deps...)
  - `security:` hoặc `fix(security):` cho vấn đề bảo mật
- Message phải mô tả WHAT và WHY, không chỉ HOW
- Một commit = một chủ đề. KHÔNG gom nhiều thay đổi không liên quan vào 1 commit
- Commit thường xuyên — dễ revert khi cần

## 3. Quy tắc kết thúc mỗi prompt sửa code

Sau khi sửa xong, BẮT BUỘC làm 3 việc sau theo thứ tự:

1. **Liệt kê file đã thay đổi** — đường dẫn đầy đủ, kèm trạng thái (created / modified / deleted / renamed)
2. **Tóm tắt những gì đã làm** — giải thích ngắn gọn các thay đổi chính, lý do, và bất kỳ trade-off nào
3. **DỪNG LẠI, đợi review** — KHÔNG tự động chuyển sang task tiếp theo, KHÔNG tự đề xuất việc khác để làm tiếp

## 4. Quy tắc an toàn

- KHÔNG đổi public API (signature method, response shape, route...) mà không cảnh báo trước
- KHÔNG xoá code mà không chắc chắn — nếu nghi ngờ, comment và hỏi
- KHÔNG sửa file config (appsettings, .env, vite.config...) mà không báo trước
- KHÔNG chạy migration database, KHÔNG `git push --force`, KHÔNG xoá branch mà không hỏi
- KHÔNG commit secret (API key, password, connection string thật) — nếu phát hiện secret trong code, CẢNH BÁO ngay
- Khi không chắc về intent của code hiện có, HỎI trước khi kết luận đó là bug

## 5. Quy tắc về phạm vi

- Chỉ làm đúng những gì prompt yêu cầu
- Nếu phát hiện vấn đề khác ngoài phạm vi: ghi chú lại vào file `FOLLOW_UP.md` (append, không overwrite), KHÔNG tự sửa
- Nếu cần đọc nhiều file để hiểu context: đọc trước, hiểu rồi mới sửa — không sửa mò

## 6. Quy tắc về chất lượng

- Sau khi sửa, chạy được những lệnh sau (nếu áp dụng) trước khi tuyên bố xong:
  - Backend: `dotnet build` không lỗi, `dotnet test` PASS
  - Frontend: `npm run build` không lỗi, `npm run lint` PASS, type check PASS
- Nếu có test liên quan đến phần sửa: chạy test đó, đảm bảo PASS
- Nếu không có test: đề xuất test case cần thêm, KHÔNG tự cho qua

## 7. Quy tắc giao tiếp

- Khi đề xuất thay đổi lớn (refactor cấu trúc, đổi tên class công khai, tách module): MÔ TẢ kế hoạch trước, đợi confirm rồi mới làm
- Khi gặp ambiguity: hỏi, không tự đoán
- Khi sửa xong, viết tóm tắt bằng cùng ngôn ngữ với prompt (Việt nếu prompt Việt, Anh nếu prompt Anh)
- Nếu một task quá lớn cho 1 lượt: đề xuất tách nhỏ, KHÔNG cố làm hết một mạch

## 8. Lưu vết công việc

Sau mỗi session sửa code dài, append vào file `CHANGELOG_REFACTOR.md` ở root:

```
## YYYY-MM-DD - [tên branch]
- File thay đổi: ...
- Tóm tắt: ...
- Lý do: ...
- Lưu ý cho lần sau: ...
```

File này giúp các phiên Claude Code sau hiểu nhanh lịch sử thay đổi mà không cần đọc lại toàn bộ git log.
