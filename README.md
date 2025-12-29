# HỆ THỐNG QUẢN LÝ PHÒNG TẬP GYM (GYM MANAGEMENT PLATFORM)

## 1. TỔNG QUAN VỀ ĐỀ TÀI
Hệ thống Quản Lý Phòng Tập Gym được xây dựng nhằm hỗ trợ các phòng tập trong việc quản lý hội viên, gói tập, lịch tập với trainer, theo dõi check-in bằng mã QR, quản lý doanh thu và hỗ trợ giao tiếp giữa trainer và hội viên qua chat real-time.

### Vai trò người dùng (Roles)
Hệ thống bao gồm 3 vai trò chính:
* **Admin:** Quản lý toàn bộ hệ thống.
* **Trainer:** Quản lý học viên thuộc trách nhiệm.
* **Member:** Hội viên sử dụng dịch vụ của phòng gym.

### Mục tiêu
Tự động hóa, tối ưu quy trình làm việc, tăng trải nghiệm người dùng, và hỗ trợ nhà quản lý đưa ra quyết định dựa trên dữ liệu trực quan từ dashboard.

---

## 2. CÔNG NGHỆ SỬ DỤNG
* **Frontend:** Next.js
* **Backend:** .NET Core 9, Laravel (Lưu ý: Cần xác định rõ dùng 1 trong 2 hoặc cả 2 cho các service khác nhau)
* **Database:** PostgreSQL
* **DevOps:** Docker, CI/CD

---

## 3. MỤC TIÊU CHI TIẾT & KỸ THUẬT

### Frontend: Next.js
* Tìm hiểu về Next.js và cấu trúc thư mục.
* Tìm hiểu về **ShadCN UI** để xây dựng Dashboard.
* Tìm hiểu về **NextAuth** để xác thực người dùng.

### Backend: ASP.NET Core 9
* **Infrastructure:** Tìm hiểu về kiến trúc hạ tầng.
* **Entity Framework Core:** Sử dụng làm ORM và dùng LINQ để truy xuất dữ liệu.
* **AutoMapper:**
    * Tạo thư mục `Mapping`.
    * Quy tắc: Mỗi Entity 1 Profile, mỗi Service 2 Mapping (1 Request, 1 Response).
    * Đăng ký DI trong `Program.cs`:
        ```csharp
        builder.Services.AddAutoMapper(typeof(Program));
        ```
* **FluentValidation:**
    * Tạo thư mục `Validators`.
    * Đăng ký DI trong `Program.cs`:
        ```csharp
        builder.Services.AddValidatorsFromAssemblyContaining<CreateSupplierValidator>();
        ```
    * Ví dụ `CreateSupplierValidator.cs`:
        ```csharp
        public class CreateSupplierValidator : AbstractValidator<CreateSupplierRequest>
        {
            public CreateSupplierValidator()
            {
                RuleFor(x => x.SupplierName)
                    .NotEmpty().WithMessage("Tên nhà cung cấp không được để trống.")
                    .MaximumLength(100);

                RuleFor(x => x.ContactEmail)
                    .NotEmpty()
                    .EmailAddress().WithMessage("Email không hợp lệ.");

                RuleFor(x => x.ContactPhone)
                    .NotEmpty()
                    .Matches(@"^(0|\+84)[0-9]{9}$").WithMessage("Số điện thoại không hợp lệ.");

                RuleFor(x => x.Address)
                    .NotEmpty();
            }
        }
        ```
* **SignalR:** Xử lý Real-time (Chat, thông báo).
* **Hangfire:** Xử lý tác vụ nền (Nhắc gia hạn gói tập).
    * Đăng ký DI trong `Program.cs`:
        ```csharp
        builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddHangfireServer();
        ```
    * Middleware:
        ```csharp
        app.UseHangfireDashboard();
        ```
    * Cách sử dụng:
        ```csharp
        BackgroundJob.Enqueue(() => Console.WriteLine("Hello from Hangfire!"));
        ```

---

## 4. THIẾT KẾ DATABASE (POSTGRESQL)

### Logic hoạt động của các bảng

#### Nhóm 1: Ngữ cảnh "Cài đặt hệ thống" (Dành cho Admin tổng)
*Dùng khi triển khai phần mềm cho khách hàng mới.*
* `core.tenants`: Định danh khách hàng (ví dụ: Chuỗi Gym A).
* `core.branches`: Danh sách chi nhánh (Quận 1, Quận 3...).
* `iam.permissions`: Danh sách quyền tĩnh (vd: `member.create`, `payment.void`).

#### Nhóm 2: Ngữ cảnh "Quản lý nhân sự" (Dành cho Manager)
*Dùng khi tuyển dụng và phân quyền.*
* `iam.users`: Tài khoản đăng nhập (Lễ tân, PT, Quản lý).
* `iam.roles`: Định nghĩa chức vụ.
* `iam.role_permissions`: Phân quyền cho từng chức vụ.
* `iam.user_roles`: Gán nhân viên vào chức vụ.
* `iam.user_branch_access`: Phân quyền chi nhánh làm việc.

#### Nhóm 3: Ngữ cảnh "Tiếp khách & Bán hàng" (Dành cho Lễ tân)
*Luồng nghiệp vụ hằng ngày.*
* `customers.members`: Thông tin hội viên, trạng thái.
* `customers.member_profiles`: Thông tin cá nhân (Tên, SĐT, Ngày sinh - **Cần mã hóa**).
* `products.membership_plans`: Danh sách gói tập (1 tháng, 1 năm...).
* `sales.invoices`: Hóa đơn thanh toán.
* `sales.invoice_items`: Chi tiết hóa đơn.
* `sales.payments`: Lịch sử giao dịch (Tiền mặt/Chuyển khoản).

#### Nhóm 4: Ngữ cảnh "Tập luyện với PT" (Dành cho PT & Hội viên)
* `fitness.trainers`: Danh sách HLV và chuyên môn.
* `fitness.pt_contracts`: Theo dõi gói tập PT (Số buổi đã tập/còn lại).
* `fitness.bookings`: Đặt lịch tập để tránh trùng giờ.

#### Nhóm 5: Ngữ cảnh "Vận hành hằng ngày" (Automated System)
* `operations.checkins`: Ghi nhận giờ vào/ra (QR Code/Thẻ từ).
* `comms.message_templates`: Mẫu tin nhắn thông báo.
* `comms.notification_logs`: Lịch sử gửi tin nhắn.

#### Nhóm 6: Ngữ cảnh "Hậu kiểm & Bảo mật" (Audit)
* `audit.audit_logs`: Ghi lại toàn bộ thao tác nhạy cảm (Ai làm? Lúc nào? Dữ liệu cũ/mới?).
* `iam.user_sessions`: Theo dõi thiết bị đăng nhập (Session management).

### 💡 Gợi ý bảng đặc biệt

#### 1. Bảng `customers.member_profiles` (Bảo mật PII)
* **Vấn đề:** Dữ liệu mã hóa (Encrypted) không thể tìm kiếm (`WHERE phone = ...`).
* **Giải pháp:** Dùng cột `phone_hash` (SHA-256) để tìm kiếm, và cột `phone_enc` để lưu dữ liệu hiển thị (giải mã ở tầng App).

#### 2. Cột `version` (Concurrency Control)
* **Vấn đề:** Hai lễ tân cùng sửa một hồ sơ, người lưu sau ghi đè người trước.
* **Giải pháp:** Optimistic Locking.
    ```sql
    UPDATE ... WHERE id = ... AND version = [version_luc_doc];
    ```

#### 3. Bảng `audit.audit_logs` (Nhật ký vĩnh viễn)
* **Lưu ý:** Không bao giờ `DELETE`. Cần cơ chế Archive sang Cold Storage (S3/DB khác) sau 1-2 năm.

---

## 5. CÁC CHỨC NĂNG CỦA HỆ THỐNG

### Admin
* Quản lý Hội Viên, Gói Tập, Trainer.
* **Dashboard thống kê:**
    * Số hội viên mới.
    * Doanh thu theo tháng.
    * Biểu đồ check-in.
    * Tình trạng gói tập.

### Trainer
* Quản lý học viên cá nhân.
* Tạo lịch tập.
* Chat với Member (SignalR).
* Theo dõi tiến trình & Lịch sử check-in của học viên.

### Member
* Đăng ký gói tập & Xem thông tin gói.
* Xem lịch sử tập & Check-in (QR Code).
* Đặt lịch tập & Chat với Trainer.
* **AI Feature:** Gợi ý gói tập phù hợp.

### Chức năng chung
* Authentication (Xác thực).
* Thanh toán (Payment Gateway).
* Thông báo gia hạn (Hangfire + Email).
* Realtime Chat (SignalR).

---

## 6. KIẾN TRÚC & API

### Sơ đồ kiến trúc (Sơ lược)

```mermaid
graph TD
    User[Client / Browser] -->|Next.js| FE[Frontend Next.js]
    FE -->|REST API| BE[ASP.NET Core 9 Web API]
    BE -->|Query/Command| DB[(PostgreSQL Database)]
    BE <-->|Real-time| Hub[SignalR Hub]
    BE -->|Schedule| HF[Hangfire Scheduler]
    HF -->|Job| Email[Email Service]
    HF -->|Update Status| DB