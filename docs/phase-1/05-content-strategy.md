# 05 – Content Strategy

> **Phiên bản 2 (CR-001):** chỉ tiếng Đức.

## 1. Mô hình kết hợp 6 lớp

| Lớp | Nguồn | Tỷ trọng MVP (ước tính) | Công sức |
|---|---|---|---|
| 1. Dataset có sẵn | Wiktionary/kaikki, Tatoeba, Leipzig (⚖️) | ~60% **từ vựng và câu ví dụ** | Viết script ETL một lần |
| 2. API ngoài | Azure TTS (audio) | ~90% **audio** | Thấp, chạy batch |
| 3. Liên kết chính thống | Goethe, DW, vhs-Lernportal, make-it-in-germany, anabin, Anerkennung, BQ-Portal… | 100% **tài liệu gốc và thông tin pháp lý** | Thấp |
| 4. Tự biên soạn | Bạn (người duyệt cuối) | Bài giảng, ngữ pháp, **toàn bộ câu hỏi** | **Cao nhất** |
| 5. AI viết bản nháp | Claude API trong CMS | Hỗ trợ lớp 4 | Trung bình |
| 6. Kiểm duyệt | Admin, sau này có cộng tác viên | 100% nội dung trước khi publish | Liên tục |

**Thực tế:** ngân hàng câu hỏi và bài giảng A1 sẽ chiếm khoảng 70% công sức nội dung, vì không có nguồn mở nào. AI giúp giảm công sức này nhưng **không loại bỏ được bước duyệt**.

## 2. Pipeline nhập dữ liệu (11 bước) – áp dụng cho Group B

```
[1 Tìm nguồn] → [2 Xác minh nhà cung cấp] → [3 Xác minh license → ghi vào bảng Sources/Licenses]
      → [4 Tải về / gọi API → lưu file thô (raw) kèm checksum + ngày]
      → [5 Phân tích cấu trúc] → [6 Chuẩn hóa → bảng staging]
      → [7 Kiểm tra trùng (lemma + POS + language)] → [8 Kiểm tra chất lượng tự động]
      → [9 Duyệt trong CMS: Draft → InReview → Approved / Rejected]
      → [10 Publish vào bảng chính] → [11 Lưu SourceVersion; lần import sau thì diff]
```

**Quyết định kỹ thuật:** bước 4–8 chạy bằng **công cụ dòng lệnh .NET** (console app trong solution, dùng chung domain), không chạy trong web request.
- **Lý do:** file lớn (kaikki 2,9 GB), chạy lâu, cần chạy lại được nhiều lần.
- **Phương án thay thế:** script Python. Nhanh viết hơn, nhưng thêm một stack và không dùng lại được entity/validation.
- Admin UI chỉ làm hai việc: (a) import CSV/JSON nhỏ, (b) duyệt staging.

## 3. Content Provenance – trường bắt buộc

| Trường | Ví dụ |
|---|---|
| ContentId | GUID |
| ContentType | Vocabulary / Example / Grammar / Question / Lesson / Media / InfoArticle |
| SourceId → Source (Name, Url, Provider) | Wiktionary (qua kaikki.org) |
| LicenseId → License (Code, Name, Url, AllowsCommercial, RequiresAttribution, ShareAlike) | CC-BY-SA-2.0-KR |
| OriginalRef | Định danh ở nguồn gốc (tên trang Wiktionary + số nghĩa, ID câu Tatoeba…) |
| ImportBatchId, ImportedAt | |
| Origin | Imported / HumanAuthored / AIDraft / AIDraftHumanEdited |
| AIModel (nếu có) | claude-haiku-5-5 |
| VerificationStatus | Draft / InReview / Approved / Rejected / NeedsUpdate |
| ReviewedBy, ReviewedAt | |
| Version, UpdatedAt | |
| ReviewDueAt (cho Info Hub) | Ví dụ 6 tháng sau ngày rà soát gần nhất |

Với **Info Hub** (Ausbildung, visa, Recognition) có thêm: `OfficialSourceUrl`, `SourceAccessedAt`, `IsOfficial` (thông tin chính thức hay tham khảo). Hệ thống tự gắn nhãn "Cần rà soát" khi quá `ReviewDueAt`.

## 4. AI hỗ trợ tạo nội dung

| Ứng dụng | Đầu vào | Rủi ro | Kiểm soát |
|---|---|---|---|
| Dịch nghĩa từ sang tiếng Việt (Wiktionary chủ yếu giải nghĩa bằng tiếng Anh) | Mục từ Wiktionary | Dịch sai sắc thái. Bản dịch là phái sinh nên mang license CC BY-SA | Duyệt 100%. Hiển thị nguồn gốc |
| Sinh câu ví dụ đơn giản theo cấp độ | Từ + cấp độ + danh sách ngữ pháp cho phép | Câu không tự nhiên | Ưu tiên câu thật (Tatoeba, Wiktionary). AI chỉ lấp chỗ trống |
| Giải thích ngữ pháp (tiếng Việt) | Điểm ngữ pháp + ví dụ | Sai quy tắc | Đối chiếu với nguồn tham khảo. Duyệt 100% |
| Sinh câu hỏi theo **dạng bài A1** (der/die/das, chia động từ, trật tự câu, đọc hiểu, nghe hiểu) | Loại câu, chủ đề, danh sách từ đã học | Đáp án sai, nhiều đáp án đúng | Kiểm tra tự động (đúng 1 đáp án, độ dài) + duyệt |
| Phân loại chủ đề và cấp độ | Danh sách từ | Sai cấp | Duyệt theo mẫu ngẫu nhiên |
| Hội thoại theo tình huống (chào hỏi, mua sắm, hẹn giờ; phỏng vấn Ausbildung ở V2) | Bối cảnh + cấp độ | Không tự nhiên | Duyệt. Tạo audio bằng TTS |

**Quy tắc:**
1. Nội dung AI luôn bắt đầu ở trạng thái `AIDraft`. Không có đường tắt để publish thẳng.
2. Mọi câu hỏi và bài kiểm tra đều ghi "do nền tảng biên soạn". Không bao giờ ghi "đề chính thức", "đề Goethe" hay "đề telc".
3. Prompt template được lưu và đánh phiên bản trong DB, để có thể truy ngược.

## 5. Kế hoạch nội dung MVP (để ước lượng công sức)

| Hạng mục | Số lượng | Nguồn | Ước tính công duyệt |
|---|---|---|---|
| Phát âm, bảng chữ cái, Umlaut, ß | ~6 bài | Tự soạn + TTS de-DE | 10 giờ |
| Từ vựng A1 | ~650 | Wiktionary (giống, số nhiều, chia động từ), đối chiếu độ phủ với Goethe A1 Wortliste. AI dịch nghĩa sang tiếng Việt | 25 giờ (duyệt nghĩa tiếng Việt) |
| Bài học A1 | ~30 | Tự soạn theo chủ đề A1 (giới thiệu bản thân, gia đình, mua sắm, nhà ở, thời gian, công việc…) | 60 giờ |
| Ngữ pháp A1 | ~25 điểm | Tự soạn + AI nháp | 30 giờ |
| Câu hỏi A1 | 300–400 | AI nháp + kiểm định + duyệt | 50–65 giờ |
| Bài kiểm tra cuối A1 | 2–3 đề (Hören + Lesen) | Câu từ ngân hàng + hội thoại dài có audio TTS | 15 giờ |
| Info Hub | ~8–10 bài (Ausbildung, Recognition, Lộ trình và chứng chỉ) | Tự viết từ nguồn chính thức | 25 giờ |
| **Tổng** | | | **~215–230 giờ nội dung**. Ít hơn bản trước, nhưng A1 làm **sâu hơn** (riêng phần code tính ở file 07) |

## 6. Quy trình duyệt cho người làm một mình (đã xác nhận: không có cộng tác viên)

Một mình bạn không thể bảo đảm chất lượng ngôn ngữ như một giáo viên. Vì vậy, thay vì "duyệt kỹ mọi thứ", ta **giảm số chỗ có thể sai** và **phát hiện lỗi sớm**:

1. **Ưu tiên dữ liệu có thẩm quyền, hạn chế văn bản tự viết.**
   - Thông tin ngữ pháp của từ (giống, số nhiều, chia động từ) và câu ví dụ ưu tiên lấy nguyên từ Wiktionary và Tatoeba (đã có người biên soạn).
   - AI chỉ viết lời giải thích tiếng Việt và câu hỏi.
2. **Kiểm tra chéo bằng AI thứ hai.**
   - Mỗi câu hỏi do AI sinh ra được một lượt gọi riêng với prompt "kiểm định" (đóng vai giáo viên, chỉ ra lỗi ngữ pháp, đáp án mơ hồ, nhiều đáp án đúng).
   - Câu bị gắn cờ thì bạn xem kỹ, câu không bị gắn cờ thì bạn xem lướt.
   - Chi phí thêm khoảng vài USD cho toàn bộ MVP.
3. **Kiểm tra tự động bằng code**:
   - Đúng một đáp án đúng.
   - Các đáp án không trùng nhau.
   - Từ vựng trong câu hỏi thuộc danh sách từ đã học ở cấp đó.
   - Độ dài câu nằm trong giới hạn.
4. **Bạn duyệt theo checklist** (đáp án đúng? có tự nhiên không? phù hợp cấp độ?). Không cần thành thạo ngôn ngữ mới duyệt được mọi mục.
5. **Người học là người duyệt cuối.**
   - Nhãn "Beta".
   - Nút "Báo lỗi" trên mỗi câu hỏi và mỗi mục từ.
   - Thống kê câu hỏi có tỷ lệ sai bất thường (ví dụ trên 90% người làm sai) để xem lại.
6. **Học song song:** bạn đang tự xây sản phẩm, nên dùng chính nền tảng này để học tiếng Đức A1 cũng là cách duyệt tốt. Tập trung một ngôn ngữ (CR-001) giúp việc này khả thi hơn.

**Phương án nếu muốn chất lượng cao hơn (đề xuất để cân nhắc, có tốn phí):** sau beta, thuê một giáo viên hoặc freelancer duyệt mẫu khoảng 20% ngân hàng câu hỏi. Chỉ làm nếu có người dùng thật.
