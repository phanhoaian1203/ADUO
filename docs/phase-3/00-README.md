# PHASE 3 – Architecture & UI/UX

**Trạng thái:** Confirmed 2026-11-22 ([decision log §11](../phase-1/08-decision-log.md)): repo public, admin soạn bằng Markdown, tên và domain chốt trước Phase 5.
**Phạm vi:** MVP German A1 + Info Hub (PD-08). Phase này chỉ thiết kế, chưa có code.

| File | Nội dung | Đáp ứng yêu cầu |
|---|---|---|
| [01-system-architecture.md](01-system-architecture.md) | Kiến trúc tổng thể, backend (modular monolith + vertical slice), frontend, auth (JWT access + refresh token, cookie HttpOnly cho web, Google), phân quyền theo permission, lưu file R2, background jobs, xử lý lỗi, logging, bảo mật, deployment + CI/CD + chi phí | Mục 9.3, 13, 14 |
| [02-database-design.md](02-database-design.md) | Quy ước, ERD, khoảng 50 bảng chia theo 7 schema, ràng buộc, index, chuẩn hóa và phi chuẩn hóa có chủ đích, chiến lược migration, seed | Mục 9.4 |
| [03-api-design.md](03-api-design.md) | Quy ước REST, phân trang, ProblemDetails, CSRF, endpoint của từng module kèm ví dụ request/response | Mục 9.5 |
| [04-folder-structure.md](04-folder-structure.md) | Monorepo, 3 project backend, cấu trúc module và slice, cấu trúc Next.js, quy tắc đặt tên, Git | Mục 9.6 |
| [05-ux-design.md](05-ux-design.md) | Nguyên tắc UX, sitemap, user flow theo màn hình, 9 wireframe, trạng thái màn hình, accessibility | Mục 10 |
| [06-design-system.md](06-design-system.md) | Màu (kèm màu theo giống danh từ), typography (Be Vietnam Pro), spacing, danh mục component, giọng văn | Mục 10 |

## Các quyết định kiến trúc chính (tóm tắt)
| ID | Quyết định |
|---|---|
| AR-01 | Một domain duy nhất: Caddy chuyển `/api/*` sang ASP.NET Core, phần còn lại sang Next.js. Không cần CORS |
| AR-02 | Trang công khai dùng SSR/ISR (SEO). Trang cần đăng nhập render ở client, nên Next.js không phải giữ phiên đăng nhập |
| AR-03 | Modular Monolith gồm 7 module, mỗi module một schema. Ranh giới kiểm tra bằng architecture test |
| AR-04 | Vertical slice + Minimal APIs. Handler được gọi trực tiếp qua DI, không dùng MediatR |
| AR-05 | **(Cập nhật theo AD-09)** Xác thực bằng JWT access token ngắn hạn + refresh token xoay vòng; web truyền bằng cookie `HttpOnly` + antiforgery, native dùng bearer. Phân quyền theo permission |
| AR-06 | Background job dùng `BackgroundService` + bảng job trong Postgres. Không thêm Hangfire hay Redis |
| AR-07 | `jsonb` có kiểu (typed) cho thân câu hỏi, khối bài học, bảng chia động từ, snapshot. Dữ liệu dùng chung thì dùng FK |
| AR-08 | Một VPS chạy Docker Compose. Staging dùng chung VPS. Migration bundle chạy trước khi deploy, không auto-migrate |

## Truy vết sang Phase 2
- BR-EXAM-04, 06, 07 được hiện thực ở `attempt_sections.deadline_at`, unique partial index, và `attempt_questions.snapshot`.
- BR-LEX-01, 02 được hiện thực ở CHECK constraint và `forms` jsonb kèm validator khi publish.
- BR-CNT-01, 03 được hiện thực ở các cột `source_id`, `license_id` NOT NULL, kiểm tra `allows_commercial` ở import và publish.
- BR-RPT-01 được hiện thực ở unique partial index trên `error_reports`.
