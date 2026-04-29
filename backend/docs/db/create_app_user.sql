-- Tạo SQL user `apsp_app` chỉ có quyền đủ để chạy app — least-privilege.
-- Thay thế cho việc dùng `sa` (DBA superuser) trong connection string.
--
-- Apply (chạy bằng sa hoặc SQL DBA):
--   sqlcmd -S "<server>" -U sa -P "<sa-pass>" -i create_app_user.sql
--
-- Sau đó update connection string app:
--   Server=...;Database=APSP;User Id=apsp_app;Password=<new-pass>;TrustServerCertificate=True
--
-- Encrypt connection string mới qua EncryptDecrypt utility, set vào User Secrets/env var.

USE [master];
GO

-- 1. Tạo SQL Login (server-level)
DECLARE @password NVARCHAR(128) = N'__REPLACE_WITH_STRONG_PASSWORD__';

IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = N'apsp_app')
BEGIN
    DECLARE @sql NVARCHAR(MAX) = N'CREATE LOGIN [apsp_app] WITH PASSWORD = ''' + @password
        + N''', DEFAULT_DATABASE = [APSP], CHECK_EXPIRATION = OFF, CHECK_POLICY = ON';
    EXEC sp_executesql @sql;
    PRINT 'Created login [apsp_app]';
END
ELSE
BEGIN
    PRINT 'Login [apsp_app] already exists — skip create';
END
GO

-- 2. Mapping vào database APSP
USE [APSP];
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'apsp_app')
BEGIN
    CREATE USER [apsp_app] FOR LOGIN [apsp_app];
    PRINT 'Created user [apsp_app] in APSP';
END
GO

-- 3. Cấp quyền — chọn 1 trong 2 mức:

-- Mức A (đơn giản, đủ cho hầu hết case): db_owner cho database APSP
-- App có quyền tạo/xoá table (cần cho EF migration), CRUD mọi data,
-- nhưng KHÔNG có quyền server-level (không tạo DB khác, không quản lý login).
ALTER ROLE db_owner ADD MEMBER [apsp_app];
PRINT 'Added [apsp_app] to db_owner role of APSP';

-- Mức B (chặt hơn, khuyến nghị cho production sau khi migration ổn định):
-- Comment Mức A, uncomment Mức B. App chỉ CRUD data + execute SP/function,
-- KHÔNG sửa schema (migration phải chạy bằng user khác).
--
--   ALTER ROLE db_datareader ADD MEMBER [apsp_app];
--   ALTER ROLE db_datawriter ADD MEMBER [apsp_app];
--   GRANT EXECUTE TO [apsp_app];
--
-- Khi cần migrate schema, dùng user DBA tạm thời.

GO

-- 4. Verify
SELECT
    dp.name AS UserName,
    dp.type_desc,
    rp.name AS RoleName
FROM sys.database_role_members drm
JOIN sys.database_principals dp ON drm.member_principal_id = dp.principal_id
JOIN sys.database_principals rp ON drm.role_principal_id = rp.principal_id
WHERE dp.name = N'apsp_app';
GO

-- Rollback (nếu cần):
--   USE [APSP]; DROP USER [apsp_app];
--   USE [master]; DROP LOGIN [apsp_app];
