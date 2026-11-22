# 08 – Decision Log (sổ quyết định của toàn dự án)

Cập nhật: 2026-11-22. Trạng thái dùng trong sổ:
- **Proposed**: đề xuất, chưa thống nhất.
- **Confirmed**: đã thống nhất.
- **Superseded**: đã bị quyết định khác thay thế (giữ lại để truy vết).

## 0. Lịch sử thay đổi phạm vi
| Mã | Ngày | Nội dung | Tài liệu |
|---|---|---|---|
| CR-001 | 2026-11-22 | **Tập trung 100% vào tiếng Đức** | [CR-001](../change-requests/CR-001-german-first.md) |

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

## 5. Technology Stack (Confirmed)
.NET 10 LTS · ASP.NET Core Web API · EF Core · PostgreSQL · ASP.NET Core Identity (+ Google login) · Next.js + TypeScript + Tailwind + shadcn/ui · TanStack Query · React Hook Form + Zod · Cloudflare R2 · Azure TTS (de-DE) · Docker · GitHub Actions · Serilog · Sentry.

## 6. Database / API / Naming Conventions
Đã chốt ở Phase 3 (Confirmed 2026-11-22):
- **Database:** snake_case, schema theo module, PK UUID v7, enum lưu text + CHECK, `timestamptz` UTC, ContentMeta nhúng vào từng bảng nội dung. Xem [phase-3/02](../phase-3/02-database-design.md).
- **API:** `/api/v1`, kebab-case, danh từ số nhiều, camelCase JSON, ProblemDetails + `code`, phân trang `page`/`pageSize`, sắp xếp `sort=-field`. Xem [phase-3/03](../phase-3/03-api-design.md).
- **Code:** vertical slice `{Verb}{Noun}Handler`, Conventional Commits, URL frontend tiếng Việt không dấu. Xem [phase-3/04](../phase-3/04-folder-structure.md).

## 7. Development Status
- Phase 1: Confirmed. **Cập nhật v2 theo CR-001** ngày 2026-11-22.
- Phase 2: Confirmed. **Cập nhật v2 theo CR-001** ngày 2026-11-22.
- Phase 3 – Architecture & UI/UX: **Confirmed** 2026-11-22. Tiếp theo: Phase 4 – MVP Development ([docs/phase-3](../phase-3/00-README.md)).
- Chưa có code.

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
