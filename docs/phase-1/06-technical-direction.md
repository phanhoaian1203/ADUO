# 06 – Technical Direction (mức định hướng, chưa thiết kế chi tiết)

Thiết kế chi tiết (ERD, API, cấu trúc thư mục) để ở Phase 3. File này chỉ chốt hướng đi để Phase 2 có cơ sở.

## 1. Kiến trúc: Modular Monolith

- **Vấn đề:** một người phải phát triển và vận hành nhiều phân hệ nhưng vẫn cần mở rộng được.
- **Đề xuất:**
  - Một ASP.NET Core Web API, chia module theo bounded context: Identity, Catalog, Lexicon, Assessment, Progress, Content Ops, Info Hub.
  - Mỗi module có thư mục riêng (Domain, Application, Infrastructure, Endpoints).
  - Các module giao tiếp qua interface và service, không truy cập bảng của nhau tùy tiện.
  - Một database, mỗi module một **schema** riêng trong PostgreSQL.
- **Ưu điểm:** triển khai một container. Debug dễ. Không có độ phức tạp của distributed system. Sau này tách được một module thành service nếu thật sự cần.
- **Nhược điểm:** phải tự giữ kỷ luật ranh giới module. Có thể dùng test kiến trúc (NetArchTest) để kiểm tra.
- **Phương án thay thế:**
  - Microservices: bị loại vì overengineering.
  - Clean Architecture 4 project cho toàn bộ hệ thống: nhiều nghi thức (ceremony). Đề xuất dạng nhẹ hơn ở Phase 3.

## 2. Stack đề xuất

| Lớp | Đề xuất | Lý do | Phương án thay thế |
|---|---|---|---|
| Backend | **ASP.NET Core (.NET 10 LTS)** Web API, C# | Đúng mục tiêu học của bạn. Hiệu năng tốt. LTS dài | .NET 8 (LTS cũ hơn) |
| ORM | **EF Core** + migrations | Chuẩn của .NET, có migration | Dapper cho các query thống kê nặng |
| Database | **PostgreSQL** | Miễn phí, nhiều gói host rẻ hoặc free. Có `jsonb` cho metadata riêng của từng chương trình. Có full-text search và `pg_trgm` cho tìm từ tiếng Đức (cả dạng có và không có Umlaut) và nghĩa tiếng Việt không dấu | SQL Server: license và hosting đắt hơn. Bản Express giới hạn 10 GB |
| Auth | **ASP.NET Core Identity** (quản lý người dùng) + **JWT access token ngắn hạn + refresh token xoay vòng** (AD-09). Web nhận token qua cookie HttpOnly, app mobile sau này dùng bearer | Có sẵn hash mật khẩu, khóa tạm, xác minh email, Google login; mô hình token dùng được lâu dài | Cookie session thuần (đơn giản hơn nhưng khó mở rộng sang mobile/bên thứ ba), Auth0/Clerk (phụ thuộc bên ngoài, tốn phí khi lớn) |
| Frontend | **Next.js (App Router) + TypeScript + Tailwind + shadcn/ui** | SSR/SSG tốt cho SEO ở landing page và Info Hub. Hệ sinh thái lớn | React + Vite (SPA): đơn giản hơn nhưng SEO kém hơn |
| State / Data fetching | **TanStack Query** + Zustand (nếu cần) | Cache dữ liệu server, ít boilerplate | Redux Toolkit: nặng cho quy mô này |
| Form | **React Hook Form + Zod** | Chuẩn phổ biến | |
| Storage file / audio | **Cloudflare R2** (S3-compatible, không tính phí egress) | Audio là lưu lượng chính. Không phí egress giúp tiết kiệm | Azure Blob / AWS S3 |
| Logging | **Serilog** → console + file / Seq (tự host) hoặc Grafana Cloud free tier | | |
| Error tracking | **Sentry** free tier | | |
| CI/CD | **GitHub Actions** | Miễn phí với repo public, hoặc có quota cho repo private | |
| Container | **Docker** + docker-compose | Môi trường dev, staging, prod giống nhau | |

## 3. Hosting chi phí thấp (định hướng, chốt ở Phase 3)

| Phương án | Thành phần | Chi phí ước tính | Phù hợp |
|---|---|---|---|
| **A – 1 VPS** | API + Postgres + Next.js trên Docker Compose. Caddy làm reverse proxy (HTTPS tự động). R2 cho file | ~5–10 USD/tháng (VPS 2 vCPU / 4 GB, ví dụ Hetzner hoặc nhà cung cấp VN) | **Đề xuất**: rẻ, học được DevOps thật. Phải tự backup |
| B – Managed | Next.js trên Vercel (free/hobby). API trên Azure App Service / Render / Fly.io. Postgres trên Neon/Supabase | 0–25 USD/tháng | Ít việc vận hành. Free tier có giới hạn (sleep, quota). Gói hobby của Vercel **không cho dùng thương mại** 🟡 |

*Giá hosting chưa được xác minh trong đợt này (🟡). Sẽ kiểm tra ở Phase 3.*

## 4. Nguyên tắc kỹ thuật cố định từ bây giờ
1. Cấu trúc bài kiểm tra (Exam Blueprint) là **dữ liệu**, không viết cứng trong code.
2. Mọi bản ghi nội dung đều có provenance và license (file 05 §3).
3. Không gọi API ngoài trong luồng học của người dùng ở MVP. Audio và dịch được tạo trước trong CMS. Lý do: chi phí dự đoán được, không phụ thuộc rate limit, chạy nhanh trên 4G.
4. Secrets (connection string, API key) chỉ nằm trong biến môi trường hoặc secret store. Không commit vào git.
5. Có ba môi trường Development, Staging, Production ngay từ khi deploy lần đầu.
6. **(CR-001)** Mô hình dữ liệu vẫn có `Language` và `ProgramType` để sau này thêm ngôn ngữ khác, nhưng **chỉ triển khai tiếng Đức**. Không xây trước tính năng cho ngôn ngữ khác (AD-05).
