# 02 – Functional & Non-functional Requirements

> **Phiên bản 2 (CR-001):** chỉ tiếng Đức.

Mức ưu tiên dùng MoSCoW. **M** = Must (bắt buộc cho MVP), **S** = Should (nên có nếu kịp), **C** = Could (V2).

## 1. Functional Requirements

### FR-ID – Identity & Profile
| ID | Yêu cầu | Ưu tiên | UC |
|---|---|---|---|
| FR-ID-01 | Đăng ký bằng email + mật khẩu, có xác nhận đồng ý điều khoản (không giới hạn tuổi, BR-AUTH-04) | M | UC-01 |
| FR-ID-02 | Gửi email xác minh. Link có hiệu lực 24 giờ, có thể gửi lại (tối đa 3 lần/giờ) | M | UC-02 |
| FR-ID-03 | Đăng nhập, đăng xuất, tùy chọn "Ghi nhớ đăng nhập" | M | UC-03 |
| FR-ID-04 | Quên mật khẩu qua email. Link có hiệu lực 1 giờ, dùng một lần | M | UC-04 |
| FR-ID-05 | Đăng nhập bằng Google | M | UC-01b |
| FR-ID-06 | Hồ sơ: tên hiển thị, ảnh đại diện (tùy chọn), **mục đích học** (Ausbildung / Điều dưỡng / Công nhận nghề / Du học / Khác), **cấp độ mục tiêu** (A1 / A2 / B1) | M | UC-05 |
| FR-ID-07 | Đổi mật khẩu khi đã đăng nhập | M | UC-05 |
| FR-ID-08 | Xóa tài khoản: xóa hoặc ẩn danh dữ liệu cá nhân trong 30 ngày | M | UC-06 |
| FR-ID-09 | Tải xuống dữ liệu cá nhân của mình (JSON) | C | — |

### FR-CAT – Catalog & Learning
| ID | Yêu cầu | Ưu tiên | UC |
|---|---|---|---|
| FR-CAT-01 | Hiển thị cây Language → Program → Level → Course → Unit → Lesson, chỉ gồm nội dung đã Published | M | UC-07 |
| FR-CAT-02 | Trang chi tiết khóa học: mô tả, số bài, thời lượng ước tính, danh sách unit/bài, tiến độ của người học | M | UC-07 |
| FR-CAT-03 | Tự động enroll khi mở bài đầu tiên. Có nút "Tham gia" rõ ràng | M | UC-08 |
| FR-CAT-04 | Bài học gồm các khối nội dung có thứ tự: RichText, VocabularyList, GrammarPoint, Dialogue, Audio, Image, Exercise | M | UC-09 |
| FR-CAT-05 | Guest học được bài đánh dấu `IsPreview` | M | UC-09 |
| FR-CAT-06 | Điều hướng "Bài trước / Bài tiếp", "Tiếp tục học" ở dashboard | M | UC-09 |
| FR-CAT-07 | Dashboard người học hiển thị theo mục tiêu đã chọn | M | UC-05 |

### FR-LEX – Từ vựng & Ngữ pháp
| ID | Yêu cầu | Ưu tiên | UC |
|---|---|---|---|
| FR-LEX-01 | Mục từ gồm: từ gốc, phiên âm hoặc IPA, loại từ, nghĩa tiếng Việt, cấp độ, chủ đề, audio, câu ví dụ, ghi công nguồn | M | UC-10 |
| FR-LEX-02 | Tiếng Đức: danh từ có giống (der/die/das) và số nhiều; động từ có bảng chia ở thì hiện tại (Präsens) | M | UC-10 |
| FR-LEX-03 | Động từ hiển thị cờ "động từ tách" và "bất quy tắc". Danh từ hiển thị mạo từ có màu kèm chữ (BR-LEX-03). Cấp A2 trở lên hiển thị thêm Perfekt | M | UC-10 |
| FR-LEX-04 | Tìm từ theo: từ tiếng Đức (chấp nhận ae/oe/ue/ss, BR-LEX-04), nghĩa tiếng Việt, tiếng Việt không dấu. Có lọc theo ngôn ngữ, cấp độ, chủ đề | M | UC-10 |
| FR-LEX-05 | Điểm ngữ pháp: tiêu đề, cấu trúc, giải thích tiếng Việt, ví dụ, lỗi thường gặp, liên kết tới bài học | M | UC-11 |
| FR-LEX-06 | Hiển thị ghi công nguồn và giấy phép trên từng mục từ / câu ví dụ đã import (BR-CNT-02) | M | — |
| FR-LEX-07 | Lưu từ vào "Sổ từ của tôi" | S | — |

### FR-ASM – Bài tập, câu hỏi, thi thử
| ID | Yêu cầu | Ưu tiên | UC |
|---|---|---|---|
| FR-ASM-01 | Loại câu hỏi MVP: **SingleChoice** (đáp án là chữ hoặc hình), **FillBlank** (gõ đáp án), **Matching** (ghép cặp), **Ordering** (sắp xếp từ thành câu) | M (Ordering: S) | UC-12 |
| FR-ASM-02 | Phần đề (stem) có thể chứa chữ, hình, audio, hoặc kết hợp | M | UC-12 |
| FR-ASM-03 | Mỗi câu hỏi có: giải thích đáp án, tag kỹ năng (Reading/Listening), tag dạng bài theo blueprint, cấp độ, chủ đề | M | UC-12 |
| FR-ASM-04 | Chế độ luyện tập: chấm ngay, hiện giải thích, làm lại không giới hạn | M | UC-12 |
| FR-ASM-05 | Đề thi thử được tạo theo **Exam Blueprint** (cấu hình phần, số câu, điểm, thời gian, dạng bài) | M | UC-24 |
| FR-ASM-06 | Hai kiểu đề: **Fixed** (bộ câu cố định) và **Generated** (rút ngẫu nhiên từ ngân hàng theo blueprint) | M (Generated: S) | UC-24 |
| FR-ASM-07 | Thi thử: đồng hồ do server quản lý, tự lưu câu trả lời, tiếp tục được sau khi mất mạng, tự nộp khi hết giờ | M | UC-13 |
| FR-ASM-08 | Kết quả: tổng điểm, điểm theo phần, theo dạng bài, danh sách câu sai kèm giải thích | M | UC-14 |
| FR-ASM-09 | Lịch sử các lần thi và biểu đồ điểm theo thời gian | M | UC-15 |
| FR-ASM-10 | Hiển thị **mức đánh giá** ("Sẵn sàng sang A2" / "Cần ôn thêm" / "Nên học lại") và gợi ý bài học theo dạng bài yếu (BR-EXAM-11) | M | UC-14 |

### FR-PROG – Tiến độ
| ID | Yêu cầu | Ưu tiên |
|---|---|---|
| FR-PROG-01 | Theo dõi trạng thái từng bài: NotStarted / InProgress / Completed | M |
| FR-PROG-02 | % hoàn thành của khóa, của unit | M |
| FR-PROG-03 | Thống kê: số bài đã học, số câu đã làm, tỷ lệ đúng theo kỹ năng, điểm thi thử gần nhất và cao nhất | M |
| FR-PROG-04 | Chuỗi ngày học (streak) | C |

### FR-INFO – Info Hub
| ID | Yêu cầu | Ưu tiên |
|---|---|---|
| FR-INFO-01 | Bài viết theo chuyên mục: **Ausbildung**, **Công nhận văn bằng**, **Lộ trình học & chứng chỉ tiếng Đức**. Du học đại học Đức để V2 (Q-CR1-02) | M |
| FR-INFO-02 | Mỗi bài hiển thị: nguồn chính thức (link), **ngày truy cập nguồn**, **ngày rà soát gần nhất**, nhãn "Thông tin chính thức" hoặc "Tham khảo", tuyên bố miễn trừ | M |
| FR-INFO-03 | Hiển thị cảnh báo khi bài đã quá hạn rà soát | M |
| FR-INFO-04 | Trang công khai, tối ưu SEO (SSR, meta, sitemap) | M |

### FR-RPT – Báo lỗi
| ID | Yêu cầu | Ưu tiên |
|---|---|---|
| FR-RPT-01 | Nút "Báo lỗi" trên mục từ, câu ví dụ, câu hỏi, khối bài học, bài Info Hub | M |
| FR-RPT-02 | Báo lỗi gồm: loại lỗi (sai đáp án, sai chính tả, audio lỗi, khác), mô tả ngắn | M |
| FR-RPT-03 | Admin có hàng đợi báo lỗi, đổi được trạng thái Open → Resolved / Rejected, và gắn cờ NeedsUpdate cho nội dung liên quan | M |

### FR-CMS – Quản trị nội dung
| ID | Yêu cầu | Ưu tiên |
|---|---|---|
| FR-CMS-01 | CRUD cho cây chương trình, bài học (kéo-thả thứ tự khối), từ vựng, ngữ pháp, câu hỏi, đề thi, blueprint | M |
| FR-CMS-02 | Upload media (audio, hình) lên object storage, có kiểm tra loại file và dung lượng | M |
| FR-CMS-03 | Quy trình trạng thái nội dung: Draft → InReview → Published, và các trạng thái NeedsUpdate, Archived | M |
| FR-CMS-04 | Import CSV/JSON có dry-run, báo cáo lỗi, hủy được theo batch | M |
| FR-CMS-05 | Quản lý Source và License. Mọi bản ghi nội dung tham chiếu tới chúng | M |
| FR-CMS-06 | Sinh bản nháp bằng AI từ mẫu prompt có phiên bản, có AI kiểm định | S |
| FR-CMS-07 | Tạo audio TTS hàng loạt cho các mục chưa có audio | S |
| FR-CMS-08 | Xem trước (preview) bài học như người học trước khi publish | M |
| FR-CMS-09 | Lịch sử phiên bản của câu hỏi và bài học (ai sửa, khi nào) | S |
| FR-CMS-10 | Trang Attribution công khai, tự liệt kê mọi Source đang được dùng | M |

### FR-ADM – Quản trị hệ thống
| ID | Yêu cầu | Ưu tiên |
|---|---|---|
| FR-ADM-01 | Dashboard: tổng người dùng, người dùng hoạt động 7 ngày, số lượt thi, báo lỗi đang mở, nội dung theo trạng thái, bài Info Hub quá hạn | M |
| FR-ADM-02 | Tìm người dùng, khóa / mở khóa tài khoản | S |
| FR-ADM-03 | Theo dõi chi phí AI và TTS đã dùng trong tháng | S |

---

## 2. Non-functional Requirements

| ID | Nhóm | Yêu cầu | Cách đo |
|---|---|---|---|
| NFR-01 | Hiệu năng – frontend | Trang học tải xong (LCP) **< 2,5 giây** trên Android tầm trung, mạng 4G | Lighthouse mobile ở chế độ throttling, đo trên staging |
| NFR-02 | Hiệu năng – API | p95 **< 300 ms** cho API đọc. p95 < 800 ms cho nộp bài thi | Log thời gian request (Serilog) |
| NFR-03 | Dung lượng | Audio nén (Opus/MP3 khoảng 48–64 kbps, mono). Ảnh WebP **< 150 KB**. JS bundle ban đầu của trang học < 200 KB gzip | Báo cáo build |
| NFR-04 | Quy mô | Đáp ứng ~1.000 người dùng đăng ký, **100 người dùng đồng thời**, 30 người thi cùng lúc, chỉ trên 1 VPS | Load test (k6) ở Phase 5 |
| NFR-05 | Sẵn sàng | Mục tiêu 99% mỗi tháng (một VPS, chấp nhận bảo trì ngắn có báo trước) | Uptime monitor (ví dụ UptimeRobot free) |
| NFR-06 | Sao lưu | Backup DB hằng ngày, giữ 7 bản ngày + 4 bản tuần, lưu **ngoài VPS** (R2). **RPO ≤ 24 giờ, RTO ≤ 4 giờ** | Diễn tập khôi phục mỗi quý |
| NFR-07 | Toàn vẹn thi | Không mất câu trả lời đã lưu khi rớt mạng. Giờ thi tính theo server | Integration test |
| NFR-08 | Bảo mật | Theo OWASP Top 10. HTTPS bắt buộc (HSTS). Cookie HttpOnly + Secure + SameSite=Lax. Chống CSRF cho request đổi trạng thái. CSP | Checklist bảo mật Phase 5 |
| NFR-09 | Bảo mật – xác thực | Hash mật khẩu bằng ASP.NET Core Identity (PBKDF2). Khóa tạm 15 phút sau 5 lần sai. Rate limit đăng nhập, đăng ký, quên mật khẩu theo IP | Test |
| NFR-10 | Quyền riêng tư | Thu thập tối thiểu. Lưu bằng chứng đồng ý. Xóa tài khoản trong 30 ngày. Không gửi dữ liệu người học cho API AI | Review Phase 3 |
| NFR-11 | Khả dụng & tiếp cận | Mobile-first, từ 360px trở lên. Theo WCAG 2.1 AA cơ bản: tương phản, focus, label, alt, điều khiển được bằng bàn phím | axe / Lighthouse a11y ≥ 90 |
| NFR-12 | Tương thích | Chrome Android, Safari iOS, Chrome/Edge desktop, 2 phiên bản gần nhất | Test thủ công |
| NFR-13 | Ngôn ngữ | Giao diện 100% tiếng Việt, văn phong thân thiện. Font hiển thị đúng ký tự đặc biệt tiếng Đức (ä ö ü ß, ẞ) và tiếng Việt. Audio TTS dùng giọng de-DE | Review UI |
| NFR-14 | SEO | Landing, catalog, Info Hub render phía server, có meta/OG tags, sitemap.xml | Lighthouse SEO ≥ 90 |
| NFR-15 | Bảo trì | Mọi business rule ở mục 04 đều có unit test. Ranh giới module được kiểm tra bằng architecture test | CI |
| NFR-16 | Quan sát | Log có cấu trúc kèm correlation id. Sentry cho lỗi frontend và backend. Không log mật khẩu, token, nội dung PII | Review |
| NFR-17 | Chi phí | Hạ tầng **≤ ~10 USD/tháng** ở beta. AI và TTS có trần chi phí tháng, cấu hình được | FR-ADM-03 |
| NFR-18 | Pháp lý nội dung | 100% nội dung đã publish có Source + License hợp lệ cho mục đích thương mại | Truy vấn kiểm tra trong CI / dashboard |
