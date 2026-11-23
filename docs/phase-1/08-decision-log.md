# 08 – Decision Log (sổ quyết định của toàn dự án)

Cập nhật: 2026-11-22. Trạng thái dùng trong sổ:
- **Proposed**: đề xuất, chưa thống nhất.
- **Confirmed**: đã thống nhất.
- **Superseded**: đã bị quyết định khác thay thế (giữ lại để truy vết).

## 0. Lịch sử thay đổi phạm vi
| Mã | Ngày | Nội dung | Tài liệu |
|---|---|---|---|
| CR-001 | 2026-11-22 | **Tập trung 100% vào tiếng Đức** | [CR-001](../change-requests/CR-001-german-first.md) |
| CR-003 | 2026-11-22 | **[ĐÃ CHỐT – phương án 2]** Kế thừa và cải tiến cá nhân hóa học từ vựng từ phân tích sâu Brottin (chế độ học từ, thẻ giàu thông tin, cài đặt cá nhân hóa, Từ của tôi). Kèm quyết định D1: **không dùng dữ liệu Brottin** | [CR-003](../change-requests/CR-003-personalization-from-brottin.md), [deep dive](../research/02-brottin-deep-dive.md) |
| CR-002 | 2026-11-22 | **[ĐÃ CHỐT – phương án 2]** Tích hợp bài học từ khảo sát Brottin, TPT Glanz, Deutsch Tiger: thêm module Engagement, "Bàn học", SRS, streak, khách vào học ngay | [CR-002](../change-requests/CR-002-competitive-insights.md), [phân tích](../research/01-competitor-analysis.md) |

## 1. Câu trả lời của chủ dự án (Phase 1, 2026-11-22)

| # | Câu hỏi | Trả lời | Hệ quả thiết kế |
|---|---|---|---|
| Q1 | Phạm vi nội dung MVP | Theo đề xuất, sau đó điều chỉnh bởi CR-001 | **German A1 + Info Hub** (PD-08) |
| Q2 | Thu phí | Có thể thu phí nếu chạy thử tốt | Coi dự án là thương mại ngay từ đầu. Loại toàn bộ dữ liệu CC BY-NC. Có cờ `AccessTier` trên Course. Không dùng gói hosting cấm thương mại cho production |
| Q3 | Thời gian | Không cố định. Hiện đang rảnh nhiều | Roadmap đi theo mốc (milestone) |
| Q4 | Người duyệt nội dung | Không có, làm một mình | Quy trình duyệt cho người làm một mình (file 05 §6) |
| Q5 | Ngân sách | Tối ưu chi phí nhất có thể | 1 VPS + R2. Khoản nào trên khoảng 10 USD/tháng thì đề xuất riêng |
| Q6 | Ngôn ngữ giao diện | Tiếng Việt, gần gũi, dễ dùng | Chỉ tiếng Việt. Không làm i18n đa ngôn ngữ |

Quyết định kỹ thuật **Confirmed**: PostgreSQL, Next.js, Modular Monolith, ASP.NET Core Identity.

## 2. Product Decisions
| ID | Quyết định | Trạng thái |
|---|---|---|
| PD-02 | Vai trò MVP: Guest, Student, Admin (kiêm Content Manager) | Confirmed |
| PD-03 | Web responsive, mobile-first. Không có app native ở MVP | Proposed |
| PD-04 | Không cam kết đỗ, tuyển hay visa. Bài kiểm tra là "do nền tảng biên soạn", không phải đề chính thức của Goethe hay telc | Proposed |
| PD-05 | MVP miễn phí. Có khả năng thu phí sau beta, nên chỉ dùng dữ liệu cho phép thương mại | Confirmed |
| PD-06 | Giao diện chỉ tiếng Việt, văn phong gần gũi | Confirmed |
| PD-07 | Nội dung gắn nhãn "Beta" kèm nút "Báo lỗi" | Confirmed |
| **PD-08** | **Chỉ tiếng Đức.** MVP = A1 đầy đủ + Info Hub (Ausbildung, Recognition, Lộ trình học). Release 1.1 = A2. V2 = B1, SRS, tiếng Đức cho Ausbildung. | **Confirmed** (CR-001) |
| PD-09 | Mục tiêu học trong hồ sơ = **mục đích** (Ausbildung / Điều dưỡng / Công nhận nghề / Du học / Khác) + **cấp độ mục tiêu** (A1, A2, B1) | Proposed |

## 3. Business Rules chính (có căn cứ)
| ID | Quy tắc | Nguồn |
|---|---|---|
| BR-04 | Mọi nội dung đều phải có Source và License trước khi publish | File 04, 05 |
| BR-05 | Nội dung AI bắt đầu ở trạng thái AIDraft và phải được duyệt mới publish | File 05 |
| BR-06 | Bài Info Hub có nguồn chính thức, ngày truy cập, ngày rà soát | File 05 |
| BR-07 | Bài kiểm tra cuối A1 lấy **khung thời lượng** tham chiếu từ phần thi viết Goethe A1 (Hören khoảng 20 phút, Lesen 25 phút), nhưng đề do nền tảng tự soạn | File 02 §1.10 (🟡) |

Rule chi tiết: [phase-2/03-business-rules.md](../phase-2/03-business-rules.md).

## 4. Architecture Decisions
| ID | Quyết định | Trạng thái |
|---|---|---|
| AD-01 | Modular Monolith, mỗi module một schema PostgreSQL | Confirmed |
| AD-02 | Exam Blueprint và cách tính điểm cấu hình theo chương trình | Confirmed |
| AD-03 | ETL dữ liệu lớn bằng công cụ CLI .NET, không chạy trong web request | Confirmed |
| AD-04 | Không gọi API ngoài trong luồng học ở MVP. Audio và bản dịch tạo trước | Confirmed |
| AD-05 | Giữ thực thể `Language` và `ProgramType` để mở rộng sang ngôn ngữ khác, nhưng **không** xây trước tính năng cho ngôn ngữ khác (YAGNI) | Confirmed |
| AD-06 | Thêm module **Engagement** (schema `engagement`, schema thứ 8). Cần **migration mới** thêm schema khi làm module này (migration `Init` chỉ tạo 7 schema) và thêm hằng `Schemas.Engagement` trong code | Confirmed (CR-002) |
| AD-07 | Luyện tập công khai chấm ở client chỉ với bộ câu hỏi `is_public_practice`, tách khỏi ngân hàng đề kiểm tra. Bài kiểm tra cuối cấp luôn chấm ở server | Confirmed (CR-002) |
| AD-08 | "Ngày học" và streak tính theo múi giờ `Asia/Ho_Chi_Minh`. XP chỉ từ sự kiện server xác nhận | Confirmed (CR-002) |
| AD-09 | **Xác thực JWT hybrid**: access token 10 phút + refresh token xoay vòng, thu hồi được; web nhận qua cookie HttpOnly, native dùng bearer. **Thay thế** cookie session của AR-05 cũ. Chi tiết ở §9d | Confirmed |

## 5. Technology Stack (Confirmed)
.NET 10 LTS · ASP.NET Core Web API · EF Core · PostgreSQL · ASP.NET Core Identity (quản lý người dùng, Google login) + **JWT access/refresh token (AD-09)** · Next.js + TypeScript + Tailwind + shadcn/ui · TanStack Query · React Hook Form + Zod · Cloudflare R2 · Azure TTS (de-DE) · Docker · GitHub Actions · Serilog · Sentry.

## 6. Database / API / Naming Conventions
Đã chốt ở Phase 3 (Confirmed 2026-11-22):
- **Database:** snake_case, schema theo module, PK UUID v7, enum lưu text + CHECK, `timestamptz` UTC, ContentMeta nhúng vào từng bảng nội dung. Xem [phase-3/02](../phase-3/02-database-design.md).
- **API:** `/api/v1`, kebab-case, danh từ số nhiều, camelCase JSON, ProblemDetails + `code`, phân trang `page`/`pageSize`, sắp xếp `sort=-field`. Xem [phase-3/03](../phase-3/03-api-design.md).
- **Code:** vertical slice `{Verb}{Noun}Handler`, Conventional Commits, URL frontend tiếng Việt không dấu. Xem [phase-3/04](../phase-3/04-folder-structure.md).

## 7. Development Status
- Phase 1: Confirmed. **Cập nhật v2 theo CR-001** ngày 2026-11-22.
- Phase 2: Confirmed. **Cập nhật v2 theo CR-001** ngày 2026-11-22.
- Phase 3 – Architecture & UI/UX: **Confirmed** 2026-11-22. Tiếp theo: Phase 4 – MVP Development ([docs/phase-3](../phase-3/00-README.md)).
- Phase 4 – Bước 1 **Foundation: xong và đã xác minh đầy đủ, chưa commit** (2026-11-22). 51 test pass (39 unit, 3 architecture, 9 integration). Migration `Init` đã áp dụng được lên PostgreSQL 17 thật (7 schema + `pg_trgm`, `unaccent`), rollback về 0 rồi áp dụng lại đều chạy đúng. Trang web hiển thị "API: OK" và "Cơ sở dữ liệu: OK" qua proxy cùng origin.
- Phase 4 – Bước 2 **Identity: xong và đã xác minh, đã commit** (2026-11-23). **100 test pass** (39 unit, 3 architecture, 58 integration; Identity chạy trên PostgreSQL thật qua Testcontainers). Migration `Identity_Init` (11 bảng schema `identity`, seed 2 vai trò). Đã thử tay trên trình duyệt: đăng ký, chọn mục tiêu, xác minh email, đăng nhập, tự làm mới token, **đăng nhập Google thật** (chủ dự án thử). `npm run lint`, `typecheck`, `build` sạch.
  - **Hoãn có chủ đích (không chặn module kế tiếp):** (1) onboarding "phút học mỗi ngày" (ID-T23) và khách chơi thử gộp tiến độ (ID-T21, T22) sang module **Engagement**, vì dữ liệu lưu ở `engagement.user_preferences`; (2) luồng "nhập mật khẩu để liên kết Google" (BR-AUTH-09 đầy đủ) sang sau MVP, hiện không tự liên kết và báo người dùng đăng nhập bằng mật khẩu; (3) ID-T17 kiểm tra đầy đủ khi có endpoint `assessment.take`; (4) thử trên điện thoại thật và grep log kiểm tra không lộ bí mật, làm cùng đợt thử thủ công trước staging. ID-T40 (xoay khóa ký) đã có test và pass.

### Điều chỉnh so với thiết kế khi code Identity
| Thay đổi | Lý do |
|---|---|
| **CSRF dùng double-submit cookie tự viết** (`XSRF-TOKEN` = header `X-XSRF-TOKEN`), không dùng `IAntiforgery` | Token antiforgery gắn với danh tính; đăng nhập xong token cũ (cấp lúc ẩn danh) hết hợp lệ, buộc frontend lấy lại ở mọi lần đổi trạng thái. Double-submit + `SameSite=Lax` + danh sách Origin tùy chọn (`Auth:AllowedOrigins`) đủ cho cookie xác thực đã `HttpOnly`. Hợp đồng với frontend (cookie + header) giữ nguyên |
| **Token email (xác minh, đặt lại mật khẩu) tự viết trên Data Protection**, không dùng token provider của Identity | Provider của Identity đọc đồng hồ hệ thống nên không kiểm thử được hết hạn bằng `TimeProvider` (ID-T07). Token đặt lại mật khẩu gắn với SecurityStamp nên chỉ dùng được một lần |
| Làm mới trong ân hạn 10 giây **chỉ cấp access token mới**, không xoay tiếp | Đúng §14.5 bước 4a: tab song song nhận cookie refresh từ request thắng cuộc (ID-T28) |
| Ngưỡng `auth_time` ≤ 12 giờ của Admin trùng hạn tuyệt đối phiên Admin (12 giờ) | Lớp bảo vệ thứ hai, chỉ có tác dụng riêng khi đổi cấu hình `Auth:AdminReauthHours` nhỏ hơn hạn phiên. Test ID-T38 giả lập `auth_time` cũ trực tiếp trong DB |
| Frontend chỉ gọi `/auth/refresh` khi có "gợi ý đã đăng nhập" trong `localStorage` | Cookie `HttpOnly` không đọc được từ JS; không có gợi ý thì khách mỗi trang gọi refresh vô ích và chạm giới hạn tốc độ |
| Trang đọc `searchParams` phải bọc trong `<Suspense>` | Next.js 16 bật Cache Components: `next dev` không báo lỗi nhưng `next build` sẽ lỗi. **Luôn chạy `npm run build` trước khi tuyên bố xong frontend** |
| Mật khẩu: thông báo lỗi tiếng Việt qua `AduoPasswordValidator`, tắt quy tắc mặc định của Identity | Quy tắc mặc định trả thông báo tiếng Anh |

### Điều chỉnh so với thiết kế khi code Foundation
| Thay đổi | Lý do |
|---|---|
| Health endpoint viết bằng minimal API thay vì `MapHealthChecks` | Để `/api/health` xuất hiện trong OpenAPI và frontend sinh được kiểu TypeScript |
| Sink Serilog cấu hình bằng code (không để trong appsettings) | Dev dùng log chữ dễ đọc, môi trường khác dùng JSON; tránh phải sửa cấu hình nhiều chỗ |
| `Microsoft.OpenApi` ghim 2.13.1 | Bản 2.0.0 đi kèm có lỗ hổng GHSA-v5pm-xwqc-g5wc (build bị chặn vì `TreatWarningsAsErrors`) |
| Dùng EF Core / Npgsql bản 10.x (không phải 11.x mới nhất) | Bản 11.x chỉ hỗ trợ net11.0, trong khi dự án dùng .NET 10 LTS |
| Next.js 16 + shadcn dùng `@base-ui/react` (không phải Radix) | Mặc định của shadcn hiện tại. Component API tương tự |
| Integration test chia hai nhóm: có DB (Testcontainers) và không DB | Phần lớn test HTTP không cần DB, nên chạy nhanh và không cần Docker |

## 8. Phase 2 – Quyết định (Confirmed 2026-11-22)
| # | Câu hỏi | Kết quả | Ghi chú sau CR-001 |
|---|---|---|---|
| Q-P2-01 | Đăng nhập Google ở MVP | Confirmed – Có | Không đổi |
| Q-P2-02 | Đã chuyển phần thì không quay lại phần trước | Confirmed | **Áp dụng cho bài kiểm tra A1** (Hören rồi Lesen) |
| Q-P2-03 | Giới hạn số lần phát audio ở chế độ thi | Confirmed – mặc định 2 lần, cấu hình được | **Áp dụng cho phần Hören**, cấu hình được theo từng phần |
| Q-P2-04 | Tuổi tối thiểu | Confirmed – không giới hạn | Không đổi. Rủi ro pháp lý ghi ở BR-AUTH-04 |

## 9. Quyết định sau CR-001 (Confirmed 2026-11-22: chủ dự án yêu cầu bắt đầu Phase 3)

| # | Câu hỏi | Kết quả | Vì sao |
|---|---|---|---|
| Q-CR1-01 | MVP chỉ gồm **A1**, còn A2 ra ngay sau ở Release 1.1? Hay đưa luôn A2 vào MVP? | ✅ **Confirmed – chỉ A1 trong MVP.** Làm A1 thật chất lượng để có người dùng thật sớm. A2 chủ yếu là nhập nội dung, hệ thống đã có sẵn | Đưa A2 vào MVP sẽ làm phần nội dung gần gấp đôi (khoảng 1.300 đơn vị từ A2 theo Goethe), lùi ngày ra beta |
| Q-CR1-02 | Info Hub MVP gồm: **Ausbildung, Công nhận văn bằng, Lộ trình học tiếng Đức và các kỳ thi chứng chỉ**. Chủ đề **du học đại học Đức** để V2? | ✅ **Confirmed** | Đúng persona chính (Ausbildung). Du học đại học là hồ sơ khác (Studienkolleg, APS…), cần nghiên cứu thêm |

## 9b. CR-002 – Quyết định (Confirmed 2026-11-22, phương án 2)

| # | Câu hỏi | Đề xuất | Trạng thái |
|---|---|---|---|
| Q-CR2-01 | Phạm vi đưa vào MVP | **Phương án 2**: toàn bộ tầng A (Bàn học, streak + nhiệm vụ, SRS, khách vào học ngay + ngữ pháp công khai, mini-game Nối từ, PWA), khoảng +20–27 ngày. Tầng B vào Release 1.1. Phương án 1 (tinh gọn: A1+A2+A3, +13–17 ngày) hoặc 3 (giữ MVP cũ) | ✅ **Confirmed** |
| Q-CR2-02 | Thêm module **Engagement** (schema `engagement`) | Đồng ý | ✅ **Confirmed** (nằm trong phương án 2) |
| Q-CR2-03 | Hướng hình ảnh "Bàn học" | Gradient + vector tự thiết kế. Không dùng ảnh nền phong cách hãng phim | ⏳ **Chưa trả lời riêng.** Tạm theo đề xuất, sẽ hỏi lại trước khi làm giao diện Bàn học |

**Quyết định đã rút ra từ khảo sát (không cần chờ):**
- Giữ nguyên **BR-INFO-05** (không in lương, chi phí khi chưa có nguồn chính thức), dù đối thủ làm.
- **Không** dùng đề thi, audio của nhà xuất bản, video/phụ đề của chương trình truyền hình, hay ảnh nền của hãng phim.
- **Không** làm phễu thu số điện thoại hay "bao đậu" (ngoài phạm vi, trái BR-PRIV-01).

**Đã chuyển từ V2 lên MVP:** Flashcards + Spaced Repetition, streak. Thêm vào MVP: Bàn học, nhiệm vụ ngày, khách vào học ngay + ngữ pháp công khai, mini-game Nối từ, PWA.

## 9c. CR-003 – Cá nhân hóa học từ Brottin (Confirmed 2026-11-22, phương án 2)

Tài liệu: [CR-003](../change-requests/CR-003-personalization-from-brottin.md), [phân tích sâu Brottin](../research/02-brottin-deep-dive.md).

**Quyết định đã rút ra (hiệu lực ngay, không cần chờ):**
| ID | Quyết định |
|---|---|
| D1 | **Không dùng dữ liệu từ vựng của Brottin** (file CSV/Excel xuất, hay nội dung khác) trong ADUO. Lý do: điều khoản của họ cấm sao chép hàng loạt/phát hành lại; file không có nguồn/giấy phép (vi phạm BR-CNT-01); chất lượng chưa đạt (55% danh từ thiếu số nhiều). Xếp Group D |
| D2 | Được học ý tưởng, cấu trúc, trải nghiệm và tự xây nội dung riêng |
| D3 | File CSV đã tải chỉ để tự học/tham khảo, **không đặt trong thư mục dự án** |
| D4 | Nội dung có hỗ trợ AI phải có **nhãn hiển thị** theo `Origin`; bản dịch AI luôn qua duyệt |

**Quyết định của chủ dự án:**
| # | Câu hỏi | Đề xuất | Trạng thái |
|---|---|---|---|
| Q-CR3-01 | Phạm vi vào MVP | **Phương án 2**: toàn bộ tầng A (chế độ học từ, thẻ giàu thông tin, cài đặt cá nhân hóa, Lưu từ + Từ của tôi, phím tắt, nhãn AI), +12–14 ngày. Giữ **điểm kiểm tra** sau Identity + ContentOps 2a + Lexicon | ✅ **Confirmed** |
| Q-CR3-02 | Thử xin phép Brottin bằng văn bản | Có, bạn tự gửi, không phụ thuộc câu trả lời | ⏳ Chưa trả lời. Việc của chủ dự án, không chặn tiến độ |
| Q-CR3-03 | V2: cho phép gửi **một từ + cấp độ** lên API AI (sửa BR-PRIV-03) | Đồng ý, chỉ ở V2 | ⏳ Quyết ở V2 |


## 9d. AD-09 – Xác thực: JWT hybrid (Confirmed 2026-11-22)

**Bối cảnh:** thiết kế ban đầu (AR-05) dùng cookie session. Chủ dự án muốn dùng JWT và ưu tiên một phương án **an toàn, dùng được lâu dài**, không chỉ nhanh cho MVP, và nhờ tôi chọn hướng hợp lý.

| ID | Quyết định | Trạng thái |
|---|---|---|
| **AD-09** | **JWT access token (10 phút) + refresh token xoay vòng có phát hiện dùng lại.** Web nhận token qua cookie `HttpOnly`; client native sau này dùng `Authorization: Bearer`. ASP.NET Core Identity chỉ dùng để quản lý người dùng (mật khẩu, khóa tạm, token email, Google, SecurityStamp), **không** dùng cookie đăng nhập của nó. **Thay thế** phần cookie session của AR-05 cũ | **Confirmed** |

**Thiết kế chi tiết:** [01-identity.md §14](../phase-4/modules/01-identity.md). Các điểm chính:
- Refresh token lưu **băm SHA-256**, xoay vòng mỗi lần dùng, dùng lại ngoài ân hạn 10 giây thì **thu hồi cả chuỗi**.
- Thu hồi ≤ 60 giây (học viên), **tức thì** (Admin và hành động nhạy cảm).
- Hạn tuyệt đối của phiên (90 ngày khi "Ghi nhớ", 7 ngày khi không). **Admin:** tối đa 12 giờ, không "Ghi nhớ".
- "Thiết bị đăng nhập", "Đăng xuất mọi thiết bị", nhật ký sự kiện bảo mật (IP rút gọn, giữ 180 ngày).
- Khóa ký trong biến môi trường, có `kid`, có quy trình xoay khóa.
- Vẫn chống CSRF vì token nằm trong cookie (đã cài bằng double-submit cookie, xem mục 7 "Điều chỉnh khi code Identity").

**Đánh đổi đã chấp nhận:** Identity tăng từ 6–8 lên **11–15 ngày công**, tổng MVP khoảng **93–126**. Phức tạp hơn cookie session, bù lại bằng bộ test dày (ID-T24..45). Khi chạy nhiều instance, bộ đệm thu hồi phải chuyển sang Redis (ghi quyết định lúc đó).

**Điều kiện xem lại AD-09:** có app mobile native hoặc API bên thứ ba (bật `/auth/token`), hoặc chạy nhiều instance.

## 10. Known Issues / Open Verification
Xem [04-license-assessment.md §5](04-license-assessment.md). Các trang chưa đọc được trực tiếp:
- anerkennung-in-deutschland.de.
- make-it-in-germany.com (điều kiện visa §16a).
- Trang download của Leipzig.
- goethe.de/prf: điểm từng phần, ngưỡng đỗ và số lần phát audio của bài thi A1.
- Giá Google TTS.
- Danh sách giọng Azure de-DE.

## 11. Phase 3 – Quyết định (Confirmed 2026-11-22)

| # | Câu hỏi | Kết quả | Vì sao |
|---|---|---|---|
| Q-P3-01 | **Repo GitHub để public hay private?** | ✅ **Confirmed – Public** (portfolio, GitHub Actions miễn phí không giới hạn phút cho repo public). Secrets không bao giờ nằm trong repo. File dữ liệu thô (Wiktionary dump) không commit | Ảnh hưởng tới CI/CD và tới việc lưu image ở GHCR. Private vẫn được, nhưng có quota phút CI |
| Q-P3-02 | **Soạn nội dung ở admin bằng Markdown** (ô soạn + xem trước), chưa có trình soạn WYSIWYG? | ✅ **Confirmed – Markdown.** Bạn là người soạn duy nhất. Markdown nhẹ, an toàn (sanitize), dễ import và xem lịch sử phiên bản | WYSIWYG (Tiptap…) tốn thêm 1–2 tuần và phức tạp hơn khi lưu trữ |
| Q-P3-03 | **Tên thương hiệu và domain**: giữ tên "ADUO"? Dùng `.com` hay `.vn`? | ✅ **Confirmed – hoãn**, tạm giữ tên "ADUO". Chốt tên và domain **trước Phase 5 (deploy)** | `.vn` gần gũi với người dùng Việt nhưng thủ tục đăng ký khác. 🟡 Giá sẽ được xác minh ở Phase 5 |
