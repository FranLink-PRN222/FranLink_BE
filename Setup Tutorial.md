# Hướng dẫn Dự án FranLink (PostgreSQL)

## 1. Lệnh chạy Migration

Để quản lý database bằng Entity Framework Core, bạn sử dụng các lệnh sau trong thư mục gốc của giải pháp (`e:\FranLink`):

### Tạo mới Migration
Khi bạn thay đổi Model, hãy chạy lệnh này để tạo file migration mới:
```powershell
dotnet ef migrations add <TenMigration> --project DataAccessLayer_FranLink/DataAccessLayer_FranLink.csproj --startup-project PresentationLayer_FranLink/PresentationLayer_FranLink.csproj
```

### Cập nhật Database
Để áp dụng các thay đổi vào database trên Supabase:
```powershell
dotnet ef database update --project DataAccessLayer_FranLink/DataAccessLayer_FranLink.csproj --startup-project PresentationLayer_FranLink/PresentationLayer_FranLink.csproj
```

---

## 2. Hướng dẫn kết nối với pgAdmin

Để quản lý database trực quan bằng pgAdmin, hãy làm theo các bước sau:

1. Mở **pgAdmin 4**.
2. Chuột phải vào **Servers** -> **Register** -> **Server...**
3. Trong tab **General**:
   - **Name**: `FranLink-Supabase` (hoặc tên bất kỳ bạn muốn).
4. Trong tab **Connection**:
   - **Host name/address**: `db.cfvmttojdmusmnfbzxzi.supabase.co`
   - **Port**: `5432`
   - **Maintenance database**: `postgres`
   - **Username**: `postgres`
   - **Password**: `thaithinh2412` (Chọn "Save password" nếu muốn).
5. Nhấn **Save**.

> [!NOTE]
> Database này đang được host trên Supabase. Đảm bảo máy tính của bạn có kết nối internet khi thao tác.
