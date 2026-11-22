# AGENTS.md – Hướng dẫn cho AI làm việc trong dự án ADUO

> Đọc file này đầu tiên khi bắt đầu một phiên làm việc mới. File giữ phần **tổng quát và ổn định**. Chi tiết nằm trong `docs/`, đừng chép lại ở đây. Khi một quyết định lớn thay đổi, cập nhật file này **và** `docs/phase-1/08-decision-log.md`.

## 1. Dự án là gì

**ADUO** là nền tảng web học **tiếng Đức bằng tiếng Việt**, lộ trình **A1 → A2 → B1 (rồi B2)**, hướng tới người học muốn đi **Ausbildung** (du học nghề Đức). Đối tượng chính là học sinh vừa tốt nghiệp THPT ở các tỉnh như Quảng Trị, dùng điện thoại Android tầm trung và mạng 4G.

**Chủ dự án làm một mình (solo developer)**, có hai mục tiêu song song:
1. **Học:** hiểu toàn bộ dự án, luyện tư duy lập trình. Vì vậy luôn giải thích *vì sao*, không chỉ đưa code.
2. **Sản phẩm thật:** làm chỉn chu để có thể **go-live** và phục vụ người dùng thật.

**Phạm vi đã chốt (CR-001):** chỉ tiếng Đức. Tiếng Hàn (EPS-TOPIK, TOPIK) đã **bị loại bỏ hoàn toàn**, không nhắc tới và không xây trước.

## 2. Cách làm việc với chủ dự án (rất quan trọng)

- **Ngôn ngữ:** trả lời bằng **tiếng Việt**. Giữ nguyên thuật ngữ kỹ thuật tiếng Anh. Văn phong rõ, thẳng, không khoa trương.
- **Làm theo từng phase và từng module**, không làm tất cả một lúc. Xong một phần thì tổng kết, nêu quyết định cần xác nhận, rồi chờ chủ dự án.
- **Phân tích trước khi code.** Không code khi yêu cầu hay dữ liệu chưa rõ.
- **Phản biện**, không đồng ý máy móc. Nếu đề xuất của chủ dự án chưa hợp lý thì nói rõ và đưa phương án tốt hơn.
- **Không overengineering.** Ưu tiên giải pháp đơn giản, công nghệ phổ biến, chi phí thấp. Không thêm thư viện hay tầng trừu tượng khi chưa cần (YAGNI).
- **Không bịa.** Không bịa tên dataset, API, URL, giấy phép, số liệu pháp lý. Chưa xác minh được thì ghi rõ (🟡 cần kiểm tra thêm, ⚖️ NEEDS LEGAL VERIFICATION, ⚪ không tìm thấy).
- **Báo cáo trung thực.** Nói rõ điều gì **đã chạy và kiểm chứng**, điều gì **chưa**. Không nói "xong" khi chưa chạy thật.
- **Git:** chủ dự án **tự commit** dưới tài khoản của họ. Không tự commit hay push nếu chưa được yêu cầu. Nếu được nhờ commit thì dùng đúng `git config user.name/email` có sẵn và **không thêm** dòng `Co-Authored-By` của AI trừ khi chủ dự án yêu cầu.
- **An toàn máy dev:** không dừng/xóa hàng loạt tiến trình hay file (ví dụ kill mọi `node`). Chỉ dừng đúng tiến trình mình khởi chạy, xác minh bằng cổng và đường dẫn trước khi dừng.
- **Bí mật:** không commit `.env`, API key, mật khẩu thật. Mật khẩu trong `compose.dev.yml` và `appsettings.Development.json` chỉ dành cho máy local.

## 3. Tài liệu (đọc theo nhu cầu)

| Cần biết | Đọc |
|---|---|
| **Mọi quyết định đã chốt, trạng thái hiện tại, điều đang chờ xác nhận** | [docs/phase-1/08-decision-log.md](docs/phase-1/08-decision-log.md) |
| Vì sao phạm vi như vậy, thay đổi phạm vi | [CR-001](docs/change-requests/CR-001-german-first.md) (chỉ tiếng Đức, **đã chốt**), [CR-002](docs/change-requests/CR-002-competitive-insights.md) (học từ đối thủ, **đã chốt phương án 2**: thêm module Engagement, SRS, streak, "Bàn học", khách vào học ngay, PWA) |
| Khảo sát Brottin / TPT Glanz / Deutsch Tiger: tính năng, UX, điều nên và không nên bắt chước | [docs/research/01-competitor-analysis.md](docs/research/01-competitor-analysis.md) |
| Phân tích sâu Brottin sau đăng nhập, phân tích file CSV, **vì sao không dùng dữ liệu của họ** | [docs/research/02-brottin-deep-dive.md](docs/research/02-brottin-deep-dive.md) |
| Cá nhân hóa học từ vựng (**đã chốt phương án 2**: chế độ học từ, thẻ giàu thông tin, cài đặt cá nhân hóa, Lưu từ + Từ của tôi) | [CR-003](docs/change-requests/CR-003-personalization-from-brottin.md) |
| Sản phẩm, persona, MVP, tính năng tương lai | [docs/phase-1/01-product-discovery.md](docs/phase-1/01-product-discovery.md) |
| Nguồn dữ liệu, giấy phép, API ngoài | [docs/phase-1/02…04](docs/phase-1/00-README.md) |
| Chiến lược nội dung, quy trình duyệt khi làm một mình | [docs/phase-1/05-content-strategy.md](docs/phase-1/05-content-strategy.md) |
| Use case, yêu cầu, **business rules (BR-xxx)**, user story + tiêu chí chấp nhận, sơ đồ | [docs/phase-2/](docs/phase-2/00-README.md) |
| Kiến trúc, **thiết kế database**, **API**, cấu trúc thư mục, UX, design system | [docs/phase-3/](docs/phase-3/00-README.md) |
| **Kế hoạch từng module** (0–8), thứ tự triển khai, test case | [docs/phase-4/00-module-breakdown.md](docs/phase-4/00-module-breakdown.md) và `docs/phase-4/modules/` |
| Bổ sung yêu cầu, rule, user story theo CR-002 | [docs/phase-2/06-cr002-addendum.md](docs/phase-2/06-cr002-addendum.md) |

Mỗi tài liệu module (`docs/phase-4/modules/NN-*.md`) có: mục tiêu, kiến thức sẽ học, mô hình miền, luồng chi tiết, API, UI, **test case có mã (ví dụ ID-T05) kèm kết quả mong đợi**, các bước triển khai, Definition of Done. **Làm module nào thì đọc kỹ tài liệu module đó trước.**

## 4. Quyết định cốt lõi (tóm tắt)

| Chủ đề | Quyết định |
|---|---|
| Kiến trúc | **Modular Monolith**, một DbContext, **mỗi module một schema PostgreSQL**. Không microservices |
| Backend | **.NET 10 LTS**, ASP.NET Core **Minimal APIs**, EF Core 10, **vertical slice** (`{Verb}{Noun}Handler`), gọi handler qua DI, **không dùng MediatR**, Result pattern thay exception cho lỗi nghiệp vụ |
| Database | **PostgreSQL**, snake_case, PK **UUID v7**, enum lưu text + CHECK, `timestamptz` UTC, `jsonb` có kiểu cho dữ liệu đa hình |
| Frontend | **Next.js 16 (App Router)** + TypeScript + Tailwind + shadcn/ui, TanStack Query, React Hook Form + Zod. Giao diện **chỉ tiếng Việt**, URL tiếng Việt không dấu |
| Auth | **JWT access token (10 phút) + refresh token xoay vòng có phát hiện dùng lại (AD-09)**. Web nhận token qua **cookie HttpOnly** + antiforgery, native dùng bearer sau này. ASP.NET Core Identity chỉ quản lý người dùng (mật khẩu, khóa tạm, token email, Google, SecurityStamp), **không** dùng cookie đăng nhập của nó. Thu hồi ≤ 60 giây (học viên), tức thì (Admin). Phân quyền theo **permission**, không theo tên vai trò |
| Một domain | `/api/*` → ASP.NET Core, còn lại → Next.js (không CORS). Dev dùng rewrites của Next.js |
| Trải nghiệm | **"Bàn học"** một cửa, streak + 3 nhiệm vụ ngày, **SRS** (SM-2 giản lược) với 6 chế độ học từ chung một lịch, thẻ từ giàu thông tin, cài đặt cá nhân hóa (độ nghiêm khi chấm chỉ ở luyện tập), Lưu từ + "Từ của tôi" (tự nhập, không AI ở MVP), khách vào học ngay (tiến độ lưu trên máy rồi gộp khi đăng ký), PWA, nhãn "Có hỗ trợ AI" (CR-002, CR-003) |
| Nội dung | Mọi nội dung có **Source + License** (BR-CNT-01). Chặn license không cho phép thương mại (có thể thu phí về sau). Vòng đời Draft → InReview → Published / NeedsUpdate / Archived |
| AI | Chỉ dùng **trong CMS** để viết nháp và kiểm định, **luôn qua duyệt**. Không gọi API ngoài trong luồng học của người học (AD-04). Không gửi dữ liệu người học lên API AI |
| Bài kiểm tra | Giờ do **server** tính, tự lưu, snapshot câu hỏi, đáp án đúng **không bao giờ** gửi xuống trước khi nộp |
| Hạ tầng | 1 VPS + Docker Compose + Caddy + Cloudflare R2, mục tiêu ≤ ~10 USD/tháng. Staging dùng chung VPS |
| Repo | GitHub **public**. Không commit bí mật |

**Tuổi người dùng:** không giới hạn tuổi; người dưới 16 xác nhận có cha mẹ đồng ý trong ô đồng ý (rủi ro pháp lý còn lại, cần rà lại NĐ 13/2023 ở Phase 5).

## 5. Cấu trúc repo

```
ADUO/
├── AGENTS.md / CLAUDE.md      # file này
├── backend/
│   ├── Aduo.slnx
│   ├── Directory.Build.props  # net10.0, Nullable, TreatWarningsAsErrors
│   ├── src/Aduo.Api/          # host mỏng: Program.cs, middleware, Setup/
│   ├── src/Aduo.Core/         # Common/, Infrastructure/, (Modules/ sẽ thêm)
│   ├── src/Aduo.Cli/          # lệnh quản trị (seed, create-admin, import…)
│   └── tests/                 # Core.Tests (unit), ArchitectureTests, Api.IntegrationTests
├── frontend/                  # Next.js. Xem thêm frontend/AGENTS.md
├── deploy/compose.dev.yml     # PostgreSQL cho máy dev
├── docs/                      # toàn bộ tài liệu
└── .github/workflows/ci.yml
```

Quy ước chi tiết (đặt tên, thư mục module, slice): [docs/phase-3/04-folder-structure.md](docs/phase-3/04-folder-structure.md).

## 6. Lệnh thường dùng

```powershell
docker compose -f deploy/compose.dev.yml up -d                       # PostgreSQL (cần Docker Desktop)
cd backend
dotnet tool restore
dotnet ef database update --project src/Aduo.Core --startup-project src/Aduo.Api
dotnet run --project src/Aduo.Api --launch-profile http            # API http://localhost:5000
dotnet test                                                         # toàn bộ (integration cần Docker)
dotnet test tests/Aduo.Core.Tests                                   # unit, không cần Docker

cd ../frontend
npm install
npm run dev                                                         # web http://localhost:3000
npm run gen:api       # sinh lại kiểu TS từ OpenAPI (cần API đang chạy)
npm run lint; npm run typecheck; npm run build

# Thêm migration (khi đã có module):
dotnet ef migrations add <Ten> --project src/Aduo.Core --startup-project src/Aduo.Api --output-dir Infrastructure/Persistence/Migrations
```

Cổng: API **5000**, web **3000**, PostgreSQL **5432**. Bash trong môi trường này là Git Bash trên Windows.

## 7. Trạng thái hiện tại

> Cập nhật mục này khi hoàn thành mỗi bước. Nguồn chính xác nhất là `docs/phase-1/08-decision-log.md` §7.

- **Phase 1–3 (khám phá, phân tích, thiết kế): xong và đã xác nhận.**
- **Phase 4 (code MVP):**
  - ✅ **Bước 1 – Foundation:** xong, đã xác minh (51 test pass, migration chạy trên Postgres thật, web ↔ API ↔ DB thông suốt).
  - ✅ **CR-002 đã chốt (phương án 2)** ngày 2026-11-22. Thêm module **8 – Engagement** vào MVP. Còn một câu chưa trả lời riêng: **Q-CR2-03** (hướng hình ảnh "Bàn học": tạm theo đề xuất gradient + vector tự thiết kế). Hỏi lại trước khi làm giao diện Bàn học.
  - ⏭️ **Bước 2 – Identity:** chưa bắt đầu. Cần chủ dự án tạo Google OAuth Client ID/Secret.
- **Hướng trải nghiệm:** chủ dự án muốn giao diện và cảm giác học gần với **Brottin** ("Bàn học" một cửa, streak, nhiệm vụ ngày, vào học ngay không cần đăng nhập), rồi mở rộng. Điểm khác biệt của ADUO: **thông tin Ausbildung/Recognition có nguồn chính thức và ngày rà soát, minh bạch nguồn/giấy phép nội dung, không môi giới**.
- **Thứ tự triển khai đã chốt (sau CR-002):** Foundation → Identity → ContentOps 2a (lõi) → Lexicon → Catalog → Assessment 5a → Progress → **Engagement** → Assessment 5b → InfoHub → ContentOps 2b. Tổng ước tính code MVP khoảng 93–126 ngày công (sau CR-002, CR-003 và AD-09). Có điểm kiểm tra sau Identity + ContentOps 2a + Lexicon để đo tốc độ thực. (Khác Phase 2: ContentOps và Lexicon trước Catalog vì bài học tham chiếu từ vựng và mọi nội dung cần Source/License.)
- Quy trình mỗi module: rà yêu cầu → migration → domain + unit test → API + integration test → UI → test thủ công trên mobile → cập nhật tài liệu. Module **chỉ xong** khi CI xanh và mọi tiêu chí chấp nhận liên quan ở `docs/phase-2/04-user-stories.md` đạt.

## 8. Những điều dễ vấp (đã gặp thật)

- **Package phải là bản 10.x.** EF Core, Npgsql, `Microsoft.AspNetCore.Mvc.Testing`… bản 11.x chỉ hỗ trợ net11.0. Dùng `dotnet package search <tên> --exact-match` để chọn đúng bản 10.x.
- **`TreatWarningsAsErrors` bật.** Cảnh báo bảo mật NuGet (ví dụ `Microsoft.OpenApi` 2.0.0) làm build lỗi. Hiện ghim `Microsoft.OpenApi` 2.13.1.
- **Next.js 16 khác bản cũ.** Đọc `frontend/AGENTS.md` và docs trong `frontend/node_modules/next/dist/docs/` trước khi viết code Next.js. shadcn hiện dùng `@base-ui/react` (không phải Radix).
- **Schema EF không tự tạo** nếu chưa có bảng trong schema đó, nên migration `Init` tạo 7 schema bằng tay. **Module Engagement cần schema thứ 8** (`engagement`): thêm `Schemas.Engagement` + migration mới tạo schema đó (AD-06). Các module khác tự thêm bảng vào schema của mình.
- **OpenAPI:** endpoint chỉ xuất hiện trong OpenAPI nếu viết bằng minimal API (đó là lý do `/api/health` không dùng `MapHealthChecks`). Thêm API mới xong thì chạy `npm run gen:api`.
- **Integration test:** nhóm `NoDbApiFactory` không cần Docker (nhanh). Nhóm `ApiFactory` dùng Testcontainers và cần Docker Desktop đang chạy.
- **Serilog:** sink cấu hình bằng code trong `Program.cs`, mức log đọc từ appsettings. Không bao giờ log mật khẩu, token, cookie, nội dung báo lỗi.
- **Dữ liệu Wiktionary** (module Lexicon) có cấu trúc chưa được kiểm tra trên dữ liệu thật. Bước đầu tiên của Lexicon là đọc thử khoảng 50 entry và ghi lại cấu trúc, rồi mới viết mapper.

## 9. Pháp lý & nội dung (không được vi phạm)

- **Không dùng dữ liệu của các nền tảng học tiếng Đức khác** (Brottin, TPT Glanz, Deutsch Tiger…): không import file CSV/Excel xuất của họ, không đưa vào repo, không dùng làm mẫu cho AI. Điều khoản của Brottin cấm sao chép hàng loạt và phát hành lại (CR-003 D1). Chỉ học ý tưởng và cấu trúc.
- Không scraping/tải hàng loạt từ Goethe, telc, DW, DWDS… Chỉ **liên kết**. Danh sách từ Goethe chỉ dùng **đối chiếu nội bộ**, không xuất bản lại.
- Dữ liệu CC BY-SA (Wiktionary) → bản dịch/biến đổi vẫn là CC BY-SA (BR-CNT-04). Phải ghi công hiển thị trên UI và có trang `/nguon-du-lieu`.
- Bài kiểm tra do nền tảng tự soạn: **không** gọi là "đề Goethe/telc", **không** quy đổi "điểm Goethe", **không** dự đoán đỗ/trượt (BR-EXAM-10, 11).
- Info Hub (Ausbildung, visa, công nhận bằng): chỉ trích dẫn con số **kèm nguồn chính thức và ngày truy cập**; không tự viết quy định, mức lương, chi phí; không cam kết kết quả (BR-INFO-05).
- Các mục còn ⚖️ cần xác minh trước khi dùng dữ liệu thật: [docs/phase-1/04-license-assessment.md](docs/phase-1/04-license-assessment.md) §5.
