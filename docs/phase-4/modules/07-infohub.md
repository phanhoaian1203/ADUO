# Module 7 – InfoHub (thông tin chính thống)

> InfoHub là nơi người học tìm hiểu **con đường sang Đức**. Đây cũng là cửa vào nền tảng qua Google, vì các trang này công khai và tối ưu SEO. Rủi ro lớn nhất ở đây không phải kỹ thuật mà là **thông tin sai hoặc đã cũ** (R4). Vì vậy module được thiết kế xoay quanh **nguồn chính thức** và **chu kỳ rà soát**.

## 1. Mục tiêu & phạm vi
| Trong phạm vi (MVP) | Ngoài phạm vi |
|---|---|
| 3 chuyên mục: **Ausbildung**, **Công nhận văn bằng (Recognition)**, **Lộ trình học & chứng chỉ tiếng Đức** | Du học đại học Đức (V2, Q-CR1-02) |
| Bài viết Markdown + nguồn chính thức + ngày truy cập + phân loại Chính thức / Tham khảo | Tư vấn hồ sơ cá nhân, môi giới (ngoài phạm vi dự án) |
| Chu kỳ rà soát + banner quá hạn + job tự gắn cờ | Tự động theo dõi thay đổi trên trang nguồn (V3) |
| SSR, SEO (meta, sitemap, structured data cơ bản) | Bình luận |
| Admin soạn bài, nút "Đã rà soát" | |

Nguồn yêu cầu: UC-16, 29 · FR-INFO-01..04 · BR-INFO-01..06 · US-41, US-56.

## 2. Kiến thức bạn sẽ học
| Khái niệm | Giải thích | Trong module |
|---|---|---|
| **SSR / ISR & SEO** | Server render sẵn HTML nên Google đọc được ngay. ISR làm mới trang theo chu kỳ mà không phải build lại toàn site | `(public)/thong-tin` |
| **Metadata, sitemap, robots** | `generateMetadata`, `sitemap.ts`, `robots.ts` của Next.js. Thẻ Open Graph khi chia sẻ link lên Facebook hoặc Zalo | |
| **Structured data (JSON-LD)** | Mô tả bài viết theo schema.org (`Article`, `dateModified`) để công cụ tìm kiếm hiểu nội dung | |
| **Nội dung có thời hạn** | Dữ liệu có "hạn dùng" (`review_due_at`) và job tự gắn cờ khi hết hạn. Đây là mẫu thiết kế dùng được ở nhiều nơi khác | BR-INFO-03, 04 |
| **Trách nhiệm thông tin** | Phân biệt thông tin chính thức với tham khảo. Câu miễn trừ. Không viết con số khi không có nguồn | BR-INFO-02, 05 |

## 3. Mô hình miền
| Entity | Thuộc tính | Quy tắc |
|---|---|---|
| `Article` (🧾) | `Category` (Ausbildung / Recognition / LearningPath), `Slug`, `Title`, `Summary` (≤ 200 ký tự, dùng làm meta description), `BodyMd`, `Classification` (Official / Reference), `LastReviewedAt`, `ReviewDueAt` | Publish khi có ≥ 1 `ArticleSource` với `IsOfficial=true` (BR-INFO-01). `ReviewDueAt = LastReviewedAt + N tháng`, N theo chuyên mục (BR-INFO-03) |
| `ArticleSource` | `Title`, `Publisher`, `Url`, `AccessedAt`, `IsOfficial` | `Url` phải là https |

**Thời hạn rà soát mặc định (cấu hình được):**
| Chuyên mục | N |
|---|---|
| Ausbildung (visa, thủ tục) | 3 tháng |
| Recognition | 6 tháng |
| LearningPath | 6 tháng |

## 4. Kế hoạch nội dung MVP (8–10 bài)
| # | Chuyên mục | Bài | Nguồn chính thức nên dẫn (đã xác định ở Phase 1) |
|---|---|---|---|
| 1 | Ausbildung | Ausbildung là gì? Khác gì so với du học đại học | make-it-in-germany.com |
| 2 | Ausbildung | Cần tiếng Đức trình độ nào? (thường là B1, có ngoại lệ) | make-it-in-germany (§16a AufenthG), IHK |
| 3 | Ausbildung | Điều kiện visa Ausbildung (§16a): hợp đồng đào tạo, chứng minh tài chính, giới hạn tuổi khi tìm chỗ học | make-it-in-germany, IHK. **Chỉ trích dẫn con số kèm nguồn và ngày** (BR-INFO-05) |
| 4 | Ausbildung | Ausbildung điều dưỡng (Pflege): học nghề khác với công nhận bằng như thế nào | make-it-in-germany, anerkennung-in-deutschland |
| 5 | Ausbildung | Tìm chỗ học nghề ở đâu (cổng chính thức) | Bundesagentur für Arbeit, make-it-in-germany |
| 6 | Recognition | Ba loại công nhận: học thuật / nghề được quản lý / nghề đào tạo (BQFG) | anabin, anerkennung-in-deutschland, BQ-Portal (BR-INFO-06) |
| 7 | Recognition | Dùng Anerkennungs-Finder như thế nào | anerkennung-in-deutschland.de |
| 8 | LearningPath | Lộ trình A1 → B1: mỗi cấp gồm những gì, học trên ADUO thế nào | Khung CEFR (Hội đồng châu Âu), Goethe |
| 9 | LearningPath | Các kỳ thi chứng chỉ tiếng Đức (Goethe, telc, ÖSD): khác nhau ra sao, đăng ký ở đâu | goethe.de, telc.net, osd.at (chỉ liên kết) |
| 10 | LearningPath | Tài nguyên học miễn phí uy tín (DW, vhs-Lernportal) | dw.com, vhs-lernportal.de (liên kết) |

⚠️ Một số trang nguồn chưa đọc trực tiếp được trong Phase 1 (🟡). **Bạn phải tự mở và đọc trang gốc bằng trình duyệt trước khi viết**, rồi ghi `AccessedAt` là ngày bạn đọc.

## 5. Chức năng chi tiết

### 5.1 Trang công khai
- **`/thong-tin`:**
  - Tab theo chuyên mục.
  - Thẻ bài gồm tiêu đề, tóm tắt, "Rà soát: dd/mm/yyyy", nhãn Chính thức / Tham khảo. Bài quá hạn có badge vàng.
- **`/thong-tin/[slug]`:**
  - Tiêu đề, nhãn phân loại, ngày rà soát.
  - Nội dung Markdown (đã sanitize).
  - Khối **Nguồn** (danh sách link + ngày truy cập).
  - **Banner quá hạn** (BR-INFO-04).
  - **Câu miễn trừ**: "Nền tảng không cam kết kết quả thi, tuyển dụng hay cấp visa. Hãy kiểm tra nguồn chính thức trước khi nộp hồ sơ."
  - Bài liên quan.
- **SEO:** `generateMetadata` (title, description = summary, OG), JSON-LD `Article` (`datePublished`, `dateModified = LastReviewedAt`), `sitemap.xml` có mọi bài Published, ISR revalidate 1 giờ.

### 5.2 Liên kết với phần học
- Dashboard hiện bài theo `learningPurpose` của người học. Map do Admin cấu hình, ví dụ Ausbildung → bài 2, 3. Nursing → bài 4.
- Onboarding có link "Vì sao là B1?" dẫn tới bài 2.
- Interface `IInfoHubReader.GetFeaturedForPurposeAsync(purpose)`.

### 5.3 Admin
- Form gồm chuyên mục, tiêu đề, slug (tự sinh), tóm tắt (đếm ký tự), MarkdownEditor (xem trước), phân loại, danh sách nguồn (thêm, xóa, **đánh dấu chính thức**, ngày truy cập).
- **Checklist riêng cho bài Info Hub** (bổ sung cho checklist chung):
  1. Mọi con số đều có nguồn và ngày.
  2. Không có cam kết.
  3. Không có lương hay chi phí môi giới.
  4. Phân biệt đúng các loại Recognition (nếu là bài Recognition).
  5. Đã mở lại mọi link nguồn hôm nay.
- **"Đã rà soát":** `POST /admin/articles/{id}/mark-reviewed` đặt `LastReviewedAt = today`, tính lại `ReviewDueAt`, và chuyển `NeedsUpdate → Published` nếu đang bị gắn cờ.
- Dashboard admin có thẻ "N bài quá hạn rà soát".

### 5.4 Job `MarkOverdueArticles` (hằng ngày)
Tìm bài Published có `review_due_at < today`, chuyển sang `NeedsUpdate` qua ContentOps lifecycle. Bài vẫn hiển thị nhưng có banner. Kèm log và số liệu cho dashboard.

## 6. Dữ liệu & API
- Bảng: [Phase 3 – 02 §3.6](../../phase-3/02-database-design.md).
- API: [Phase 3 – 03 §7, §8](../../phase-3/03-api-design.md).

## 7. Test cases
| ID | Kịch bản | Kết quả mong đợi |
|---|---|---|
| IH-T01 | Publish bài không có nguồn chính thức | 422 |
| IH-T02 | Publish bài Ausbildung ngày 22/11/2026 | `ReviewDueAt = 22/02/2027` |
| IH-T03 | Job chạy khi bài quá hạn | Bài chuyển NeedsUpdate, trang hiện banner |
| IH-T04 | Bấm "Đã rà soát" | Bài về Published, `ReviewDueAt` được tính lại |
| IH-T05 | Trang bài viết khi chưa đăng nhập | 200, HTML có title, meta description, JSON-LD |
| IH-T06 | `sitemap.xml` | Có các bài Published, không có bài Draft |
| IH-T07 | URL nguồn `http://` | 400 |
| IH-T08 | Markdown có `<iframe>` | Bị loại khi render |
| IH-T09 | Dashboard người học mục đích Nursing | Hiện bài 4 |

## 8. Các bước triển khai
1. Entity + migration + `IPublishable` (BR-INFO-01) + tính `ReviewDueAt`.
2. API công khai + admin + job.
3. Frontend trang công khai (SSR, metadata, JSON-LD, sitemap).
4. Admin form + checklist riêng.
5. **Nội dung:** đọc nguồn gốc, viết 8–10 bài (khoảng 25 giờ, Phase 1 file 05).

## 9. Definition of Done
- [ ] IH-T01..09 pass. Lighthouse SEO ≥ 90 cho trang bài viết (NFR-14).
- [ ] ≥ 5 bài Published, mỗi bài có nguồn chính thức đã được mở lại trong tuần.

## 10. [ĐÃ CHỐT – CR-002, phương án 2] Thay đổi liên quan
> Đã chốt ngày 2026-11-22 (CR-002, phương án 2). Đây là kế hoạch chính thức.

- **Chuyên mục mới `StudyTips` (B4, Release 1.1):** cụm bài SEO theo mẫu "tiêu đề dạng câu hỏi → quy tắc → ngoại lệ → bảng từ người Việt hay nhầm → FAQ → nút vào bài tập". Ví dụ: "Cách nhớ der/die/das theo đuôi từ", "Goethe, telc, ÖSD khác nhau thế nào". Bài do ta tự viết, không chép của ai. Chuyên mục này **không** cần nguồn chính thức bắt buộc (BR-INFO-01 chỉ áp dụng cho Ausbildung, Recognition, LearningPath), nhưng nếu nhắc tới quy định thi thì vẫn phải dẫn nguồn.
- **Trang hướng dẫn lịch thi (B6, 1.1):** **không chép lịch**. Chỉ dẫn cách tra trên cổng chính thức của đơn vị thi, ghi rõ ngày rà soát, và cảnh báo khi quá hạn. Mẫu trung thực này học từ cách Deutsch Tiger trình bày lịch thi Goethe.
- **Bài học từ TPT Glanz:** dữ liệu có thời hạn mà không tự hết hạn làm mất lòng tin (trang "đơn đang mở" của họ vẫn hiển thị đơn nhập học 09/2026 và 10/2026 khi tôi xem ngày 22/11/2026). Cơ chế `ReviewDueAt` và job `MarkOverdueArticles` của ta giải quyết đúng vấn đề này.
- **Giữ nguyên BR-INFO-05:** không in lương, chi phí, "để dành được bao nhiêu" khi chưa có nguồn chính thức kèm ngày, dù đối thủ làm.
- Test bổ sung: **IH-T10** bài `StudyTips` không có nguồn vẫn publish được, **IH-T11** bài nhắc ngày thi mà thiếu nguồn bị chặn.
