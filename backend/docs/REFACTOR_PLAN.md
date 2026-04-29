# Plan tách God class — `DocumentService` (9000 dòng) và `BusinessPartnerService` (~2400 dòng sau cleanup)

⚠ Refactor này KHÔNG nên làm 1 commit lớn. Đề xuất 1 PR / module, mỗi PR có:
1. Extract method sang file mới
2. Forward call cũ → method mới (giữ tương thích)
3. Update tests + verify
4. Sau khi tất cả call site dùng method mới → xoá forward và clean up

## 1. `BusinessPartnerService` (~2400 dòng, 31 methods)

### Cấu trúc đề xuất

```
Service/BusinessPartners/
├── BusinessPartnerService.cs          # Giữ CRUD chính: ~600 dòng
│   • CreateBPAsync, UpdateBPAsync, DeleteBPAsync
│   • GetAllBPAsync (3 overloads), GetBPAsync, GetBPByIdAsync
│   • SyncBPAsync (2 overloads — bulk import)
│
├── BPCRD3Service.cs                   # Payment method preferences (CRD3)
│   • GetCRD3Async, UpdateCrd3, CUDCRD3, GetCurrentCommited
│   ~150 dòng
│
├── BPAddressService.cs                # Quản lý địa chỉ
│   • AddAddress, UpdateAddress, RemoveAddress
│   ~200 dòng
│
├── BPFileService.cs                   # Upload + manage attachment
│   • AddFiles, RemoveFile, UpdateFiles
│   ~200 dòng
│
├── BPClassifyService.cs               # Phân loại khách hàng
│   • AddClassify, UpdateClassify, RemoveClassify
│   ~200 dòng
│
├── BPAssignmentService.cs             # Sale staff assignment + hierarchy
│   • ChangeSaleStaff, GetAllStaffUnderManager
│   ~150 dòng
│
└── Legacy/
    └── BusinessPartnerSyncLegacy.cs   # Sync methods cũ (proxy + point-specific)
        # Vẫn được wrap qua InternalProxySyncService
        • SyncBPCRD4Async, SyncTTDHAsync, SyncTTDHHAsync,
          SyncTTDH1Async, SyncIssueCancelAsync, SyncCancelYCHGsync
        ~700 dòng
```

### Thứ tự PR đề xuất (4 tuần, 1 PR/tuần)

1. **Week 1**: Extract `BPCRD3Service` (smallest, isolated)
2. **Week 2**: Extract `BPAddressService` + `BPFileService`
3. **Week 3**: Extract `BPClassifyService` + `BPAssignmentService`
4. **Week 4**: Move sync legacy → `Legacy/BusinessPartnerSyncLegacy`

POC trong session này: xem [Service/BusinessPartners/Address](../BackEndAPI/Service/BusinessPartners/Address/) — đã extract `BPAddressService` làm template.

## 2. `DocumentService` (~9000 dòng — chưa biết chính xác sau khi sync legacy còn dùng)

### Cấu trúc đề xuất

```
Service/Document/
├── DocumentService.cs                 # Core query/CRUD: ~800 dòng
│   • GetDocumentList, GetById, basic Create/Update
│
├── Order/
│   ├── OrderCreationService.cs        # Tạo đơn DXN
│   │   • Logic validate, tính discount, áp promotion
│   ├── OrderConfirmationService.cs    # Workflow xác nhận đơn
│   │   • Chuyển status XN → DXN → DONG, trigger notifications
│   └── OrderQueryService.cs           # Báo cáo, lọc đơn theo status
│
├── Return/
│   └── ReturnDocumentService.cs       # ORFS, đơn trả hàng
│       • Khoảng 1000 dòng
│
├── Payment/
│   └── DocumentPaymentService.cs      # Áp dụng thanh toán vào đơn
│
├── VPKM/
│   └── VPKMService.cs                 # Văn phòng kiểm tra mặt hàng (ObjType=12)
│
├── Promotion/
│   └── DocumentPromotionService.cs    # Tính discount theo promotion
│       • Voucher redemption logic
│
└── Legacy/
    └── DocumentSapPushLegacy.cs       # Sync push to SAP cũ
        # Vẫn dùng qua DocumentPushSyncService
        • SyncToSapAsync, SyncToSapDraftAsync,
          SyncIssueToSapAsync, SyncVPKMToSapAsync
        ~5000 dòng (phần lớn của file gốc)
```

### Thứ tự PR đề xuất (8 tuần, 1 PR/tuần)

1. **Week 1**: Move sync legacy → `Legacy/DocumentSapPushLegacy` (mass move, không sửa logic) → giảm 50% kích thước file ngay
2. **Week 2**: Extract `OrderQueryService` (read-only, an toàn)
3. **Week 3**: Extract `OrderCreationService`
4. **Week 4**: Extract `OrderConfirmationService` (workflow phức tạp — cần nhiều test)
5. **Week 5**: Extract `ReturnDocumentService`
6. **Week 6**: Extract `DocumentPromotionService`
7. **Week 7**: Extract `DocumentPaymentService`
8. **Week 8**: Extract `VPKMService` + cleanup

## Quy tắc khi tách

### ✅ Nên

1. **Mỗi PR self-contained**: extract 1 service, update controllers gọi đúng nơi, test pass.
2. **Giữ namespace cũ** cho service mới: `BackEndAPI.Service.BusinessPartners.BPAddressService` — không đổi.
3. **Forward call** trong service cũ trước khi xoá method:
   ```csharp
   // Old in BusinessPartnerService — keep for 1-2 sprint, then remove
   public Task<(BP?, Mess?)> AddAddress(int bpId, CRD1 address)
       => _addressService.AddAddress(bpId, address);
   ```
4. **Inject service mới qua constructor** chứ không qua service locator.
5. **Test cover từng service mới**: xem `BackEndAPI.Tests/Unit/Services/` làm template.

### ❌ Không nên

1. Refactor logic trong cùng PR với extract — chỉ MOVE, không SỬA.
2. Tách quá nhỏ (1 method / file) — gom theo chức năng nghiệp vụ.
3. Đổi signature method khi extract — sẽ phải update mọi call site cùng lúc.
4. Quên xoá `Task<...>` declaration trong interface cũ sau khi đã xoá method.

## Đo lường tiến độ

| Mốc | Mục tiêu |
|---|---|
| Sau Week 4 (BPService) | `BusinessPartnerService.cs` < 800 dòng |
| Sau Week 8 (DocumentService) | `DocumentService.cs` < 1500 dòng |
| Test coverage | Mỗi service mới ≥ 60% line coverage |
| Build warnings | Tiếp tục giảm xuống < 200 |

## Tài nguyên hỗ trợ

- `[Service/Sync/](../BackEndAPI/Service/Sync/)` — đã làm sẵn pattern tách service theo domain (`BPSyncService`, `DocumentPushSyncService`, etc.)
- `[Models/Sync/](../BackEndAPI/Models/Sync/)` — pattern cho models phụ trợ
- `[BackEndAPI.Tests/Unit/Services/SampleServiceTests.cs](../BackEndAPI.Tests/Unit/Services/SampleServiceTests.cs)` — template Moq + xUnit
