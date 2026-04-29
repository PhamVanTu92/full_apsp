-- Unique index cho CRD4 — DB-level enforcement cho composite natural key.
--
-- Bối cảnh: BPSyncService dedup theo (BPId, InvoiceNumber) ở application layer,
-- nhưng DB không enforce. Nguy hiểm khi 2 instance race UPSERT hoặc code bug.
--
-- Audit ngày 2026-04-28:
--   • 1690 rows, (BPId, InvoiceNumber) unique → an toàn add unique index
--   • Có 1 cặp trùng InvoiceNumber giữa BPId khác nhau → composite vẫn unique
--   • InvoiceNumber hiện là nvarchar(MAX) — KHÔNG indexable. Phải ALTER xuống
--     nvarchar(50) trước (max length thực tế = 10 char, set 50 cho an toàn)
--
-- Apply:
--   sqlcmd -S "<server>" -d APSP -U <user> -P <pass> -i CRD4_unique_index.sql
--
-- Rollback:
--   DROP INDEX UX_CRD4_BPId_InvoiceNumber ON CRD4;
--   -- KHÔNG ALTER ngược lại MAX vì có thể đang dùng đúng

-- Step 1: ALTER InvoiceNumber từ nvarchar(MAX) → nvarchar(50)
-- Cần drop columnstore index CRD4_inter (đang reference InvoiceNumber) trước.
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'CRD4'
      AND COLUMN_NAME = 'InvoiceNumber'
      AND CHARACTER_MAXIMUM_LENGTH = -1   -- -1 = MAX
)
BEGIN
    IF EXISTS (SELECT 1 FROM CRD4 WHERE LEN(InvoiceNumber) > 50)
    BEGIN
        RAISERROR('Có InvoiceNumber > 50 ký tự — không thể shorten an toàn', 16, 1);
        RETURN;
    END

    -- Drop columnstore index nếu có
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='CRD4_inter' AND object_id=OBJECT_ID('CRD4'))
    BEGIN
        DROP INDEX CRD4_inter ON CRD4;
        PRINT 'Dropped CRD4_inter (columnstore)';
    END

    ALTER TABLE CRD4 ALTER COLUMN InvoiceNumber NVARCHAR(50) NOT NULL;
    PRINT 'Altered CRD4.InvoiceNumber: nvarchar(MAX) -> nvarchar(50)';

    -- Recreate columnstore index — covering tất cả cột nghiệp vụ thường query
    CREATE NONCLUSTERED COLUMNSTORE INDEX CRD4_inter ON CRD4 (
        InvoiceNumber, InvoiceDate, DueDate,
        InvoiceTotal, PaidAmount, AmountOverdue,
        PaymentMethodCode, PaymentMethodID, PaymentMethodName,
        BPId
    );
    PRINT 'Recreated CRD4_inter';
END
ELSE
BEGIN
    PRINT 'CRD4.InvoiceNumber already correct length';
END

-- Step 2: Unique composite index
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_CRD4_BPId_InvoiceNumber' AND object_id = OBJECT_ID('CRD4')
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM CRD4 GROUP BY BPId, InvoiceNumber HAVING COUNT(*) > 1
    )
    BEGIN
        RAISERROR(
            'Có duplicate (BPId, InvoiceNumber). Dọn data trước khi tạo unique index.',
            16, 1
        );
        RETURN;
    END

    CREATE UNIQUE INDEX UX_CRD4_BPId_InvoiceNumber ON CRD4 (BPId, InvoiceNumber);
    PRINT 'Created UX_CRD4_BPId_InvoiceNumber';
END
ELSE
BEGIN
    PRINT 'UX_CRD4_BPId_InvoiceNumber already exists';
END
