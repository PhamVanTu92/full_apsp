# Kế hoạch Sprint — i18n Extraction

> **Trạng thái:** Phase 1 ✅ DONE | Phase 2 ✅ DONE | Phase 3–5 🔲 TODO  
> **Mục tiêu:** Xóa toàn bộ hardcoded Vietnamese text khỏi sourcecode

---

## Nguyên tắc sử dụng

### Pattern đúng
```vue
<!-- Template -->
<Button :label="t('common.btn_confirm')" />
<Column :header="t('common.action')" />
{{ t('common.no_data') }}

<!-- Script -->
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
toast.add({ summary: t('common.error'), detail: t('common.msg_error_occurred') });
```

### Tra cứu keys có sẵn trong `common`
| Dùng cho | Key |
|---|---|
| Nút Xác nhận | `common.btn_confirm` |
| Nút Hủy | `common.btn_cancel` |
| Nút Lưu | `common.btn_save` |
| Nút Xóa | `common.btn_delete` |
| Nút Chỉnh sửa | `common.btn_edit` |
| Nút Thêm mới | `common.btn_add` |
| Nút Đóng | `common.btn_close` / `common.close` |
| Nút Gửi | `common.btn_send` |
| Nút Quay lại | `common.btn_back` |
| Nút Xuất Excel | `common.btn_export_excel` |
| Nút Nhập Excel | `common.btn_import_excel` |
| Nút Xóa bộ lọc | `common.btn_clear_filter` |
| Nút Hiển thị thêm | `common.btn_show_more` |
| Nút Ẩn bớt | `common.btn_show_less` |
| Nút Tạo | `common.btn_create` |
| Nút Tạo và gửi | `common.btn_create_and_send` |
| Nút Lưu nháp | `common.btn_draft` |
| Nút Duyệt | `common.btn_approve` |
| Nút Từ chối | `common.btn_reject` |
| Nút Thêm người duyệt | `common.btn_add_approver` |
| Column "Hành động" | `common.action` |
| Column "Người duyệt" | `common.approver` |
| Column "Tên tài liệu" | `common.document_name` |
| Toast summary "Lỗi" | `common.error` |
| Toast summary "Thành công" | `common.success` |
| Toast summary "Cảnh báo" | `common.warning` |
| Toast detail lỗi chung | `common.msg_error_occurred` |
| Toast detail lưu OK | `common.msg_save_success` |
| Toast detail xóa OK | `common.msg_delete_success` |
| Toast detail gửi OK | `common.msg_send_success` |
| Không có dữ liệu | `common.no_data` |
| Dialog xác nhận xóa header | `common.dialog_confirm_delete` |
| Confirm message xóa tài liệu | `common.msg_confirm_delete_document` |
| Placeholder tìm kiếm | `common.placeholder_search` |
| Placeholder nhập từ khóa | `common.placeholder_enter_keyword` |
| Số lượng khách hàng (count) | `common.customers_unit` |

---

## Phase 1 ✅ — Common namespace + Shared Components
**Hoàn thành:** `vi.json`, `en.json`, 8 components

Files đã fix:
- `src/components/AddApprovalersComp.vue`
- `src/components/AddApprovalLevelComp.vue`
- `src/components/DocumentAttach/DocumentAttach.vue`
- `src/components/CustomerSelector.vue`
- `src/components/CustomerSelectorId.vue`
- `src/components/DetailWarehouseStorageFee.vue`
- `src/components/FileSelect/FileSelect.vue`
- `src/views/client/pages/categories/components/Brand.vue`

---

## Phase 2 ✅ — Remaining Components (13 files)

### Ưu tiên cao (dùng nhiều nơi)
| File | Strings cần fix | Keys gợi ý |
|---|---|---|
| `src/components/FilesAttachment.vue` | "Thêm tài liệu", "Tên tài liệu" | `common.btn_add` + `common.document_name` |
| `src/components/FiltersProducts.vue` | "Thương hiệu", "Ngành hàng", "Quy cách bao bì", "Nhập từ khóa" | `body.home.brand_column`, `body.home.category_column`, `body.home.packaging_column`, `common.placeholder_enter_keyword` |
| `src/components/NodeGItem.vue` | "Chọn hàng mua", "Tìm kiếm nhóm hàng hoá" | Thêm key mới |
| `src/components/SearchAddress.vue` | "Thêm địa chỉ", "Nhập khu vực", "Nhập Phường/Xã" | Thêm key mới vào `common` |
| `src/components/SearchAddressOld.vue` | "Thay đổi", "Thêm địa chỉ khác", "Huỷ", "Xác nhận", "Cập nhật địa chỉ..." | `common.btn_*` |
| `src/components/CustomerSelection.vue` | "Hủy", "Chọn" | `common.btn_cancel`, `common.btn_select` |
| `src/components/DebtCheck.vue` | "Check công nợ", "Đóng" | `common.btn_close` |
| `src/components/OutputCheck.vue` | "Xem thêm", "Đóng" | `common.btn_show_more`, `common.btn_close` |
| `src/components/BillDetail/BillDetail.vue` | "Mô tả", "Giá trị (VND)" | `common.description` |
| `src/components/FilePicker/index.vue` | "Chỉnh sửa" | `common.btn_edit` |
| `src/components/order/AddProducts.vue` | "Chọn phương thức thanh toán" | `body.OrderList.payment_method_label` |

### Quy trình fix
```bash
# Với mỗi file:
1. Thêm `import { useI18n } from 'vue-i18n'` và `const { t } = useI18n()`
2. Thay string literal bằng t('key')
3. Nếu key chưa có → thêm vào vi.json + en.json trước
```

---

## Phase 3 🔲 — Admin Views (ưu tiên theo traffic)

### Sprint 3a — User Management
| File | Strings chính |
|---|---|
| `src/views/admin/Decentralization/components/NPP.vue` | 8+ strings (buttons, column headers, dialog) |
| `src/views/admin/User/UserList.vue` | 10+ strings |
| `src/views/admin/Decentralization/edit.vue` | "Bỏ qua", "Lưu", "Thêm mới vai trò" |

Keys cần thêm vào `body.admin.user.*`:
```json
{
  "add_user": "Thêm mới người dùng",
  "edit_user": "Chỉnh sửa người dùng",
  "user_name_col": "Tên người dùng",
  "full_name_col": "Họ và tên",
  "role_col": "Vai trò",
  "confirm_delete_user": "Bạn có chắc chắn xoá người dùng {name} không?"
}
```

### Sprint 3b — CommittedOutput / Discount
| File | Strings chính |
|---|---|
| `src/views/admin/CommittedOutput/index2.vue` | 15+ strings |
| `src/views/admin/Discount/Discount.vue` | 8+ strings |

### Sprint 3c — MasterData
| File | Strings chính |
|---|---|
| `src/views/admin/MasterData/SupplyItems/GroupProduct.vue` | 8+ strings |
| `src/views/admin/MasterData/SupplyItems/ItemType.vue` | 2 strings |

### Sprint 3d — Promotion
| File | Strings chính |
|---|---|
| `src/views/admin/Promotion/composables/usePromotionEdit.js` | Validation messages |
| `src/views/admin/ProductRetail/ProductDataRetailEdit.vue` | "Tải mẫu in", "Nhập Excel" |

### Sprint 3e — Remaining Admin (135+ files)
Áp dụng script tự động hóa (xem phần Tooling bên dưới)

---

## Phase 4 🔲 — Client Views (60+ files)

| Nhóm | Files |
|---|---|
| Purchase orders | `client/pages/PurchaseOrder/**` |
| User menu | `client/pages/user_menu/**` |
| Product pages | `client/pages/categories/**`, `client/pages/detail/**` |
| Checkout | `client/pages/checkout/**` |
| Reports | `client/pages/report/**` |

---

## Phase 5 🔲 — Auth + Common Views

| File | Strings chính |
|---|---|
| `src/views/auth/Login.vue` | "Đăng ký" |
| `src/views/auth/Register.vue` | "Đăng ký tài khoản" |
| `src/views/auth/OTP.vue` | "Gửi", "Quay trở lại đăng nhập" |
| `src/views/common/Order/dialogs/YCBS.vue` | 3 placeholders |

---

## Tooling — Script quét tự động

Dùng script sau để tìm tất cả hardcoded Vietnamese text còn sót:

```bash
# Tìm label với tiếng Việt
grep -rn 'label="[^"]*[àáâãèéêìíòóôõùúýăđơư]' src/

# Tìm placeholder với tiếng Việt
grep -rn 'placeholder="[^"]*[àáâãèéêìíòóôõùúýăđơư]' src/

# Tìm header với tiếng Việt
grep -rn 'header="[^"]*[àáâãèéêìíòóôõùúýăđơư]' src/

# Tìm toast summary/detail hardcode
grep -rn "summary: ['\"][^'\"]*[àáâãèéêìíòóôõùúýăđơư]" src/
grep -rn "detail: ['\"][^'\"]*[àáâãèéêìíòóôõùúýăđơư]" src/
```

---

## Checklist trước khi đóng sprint

- [ ] `vi.json` và `en.json` đồng bộ (mọi key trong vi đều có trong en)
- [ ] Không còn `label="[tiếng Việt]"` trong template
- [ ] Không còn `summary: "Lỗi"` hay `summary: "Thành công"` hardcode
- [ ] Không còn `header="[tiếng Việt]"` trong Column/Dialog
- [ ] Test chuyển ngôn ngữ vi ↔ en hoạt động đúng
- [ ] PR description ghi rõ keys đã thêm
