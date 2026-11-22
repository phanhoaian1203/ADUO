# 04 – License & Copyright Assessment

> Đây là phân tích kỹ thuật, không phải tư vấn pháp lý. Với các mục ⚖️, cần đọc điều khoản gốc hoặc hỏi nhà cung cấp bằng email và lưu lại câu trả lời.

## 1. Ba lớp giấy phép phải tách riêng

| Lớp | Ví dụ trong dự án | Áp dụng cho |
|---|---|---|
| **Mã nguồn** | MIT / Apache 2.0 của thư viện, code của chính bạn | Code backend và frontend |
| **Dataset** | CC BY-SA (Wiktionary), CC BY 2.0 FR / CC0 (Tatoeba), CC BY-SA 4.0 / LGPL (OpenThesaurus) | Từ vựng, câu ví dụ đã import |
| **Nội dung giáo dục / media** | All rights reserved (Goethe, telc, DW), giấy phép từng file audio (Commons) | Bài giảng, đề, audio, hình ảnh |

**Nguyên tắc:** giấy phép của wrapper (ví dụ thư viện đọc dữ liệu Wiktionary) **không** phải giấy phép của dữ liệu.

## 2. Ma trận giấy phép gặp trong dự án

| Giấy phép | Dùng thương mại | Phải ghi công | Share-alike | Được sửa | Nguồn trong dự án |
|---|---|---|---|---|---|
| CC0 / Public Domain | ✅ | Không bắt buộc (nên ghi) | Không | ✅ | Tatoeba (tập CC0), Common Voice |
| CC BY | ✅ | ✅ | Không | ✅ | Tatoeba (CC BY 2.0 FR), Leipzig text corpora (🟡) |
| **CC BY-SA** | ✅ | ✅ | **✅** | ✅ | **Wiktionary**, OpenThesaurus |
| CC BY-NC | ⛔ | ✅ | — | ✅ | Một phần dữ liệu Leipzig (🟡). **Loại bỏ nếu có kế hoạch thu phí** |
| CC BY-ND | ✅ | ✅ | — | ⛔ | Không gặp |
| Riêng / All rights reserved | ⛔ | — | — | ⛔ | Goethe, telc, DW, DWDS |
| Không xác định | ⚖️ | — | — | — | Danh sách từ trôi nổi, repo chép lại Goethe list |

## 3. Vấn đề share-alike (SA): quyết định kiến trúc quan trọng

**Vấn đề:** Wiktionary là nguồn từ vựng chính và dùng CC BY-SA. Nếu bạn **chỉnh sửa** một mục từ (ví dụ dịch định nghĩa sang tiếng Việt, rút gọn nghĩa), thì mục từ phái sinh đó **phải** được phân phối theo CC BY-SA.

**Cách xử lý đề xuất:**
1. **Tách lớp dữ liệu theo license trong database.** Mỗi `VocabularyEntry` và `ExampleSentence` mang theo `LicenseId` và `SourceId`.
2. Hiển thị phần ghi công ngay trên UI, ở cuối mục từ hoặc ở trang Nguồn. Ví dụ: *"Nguồn: 한국어기초사전, Viện Quốc ngữ Quốc gia, CC BY-SA 2.0 KR"*, kèm link.
3. **Chấp nhận** rằng lớp dữ liệu từ điển của nền tảng (gồm cả phần phái sinh) là CC BY-SA. Việc này **không** lây sang:
   - **Mã nguồn**: phần mềm không phải tác phẩm phái sinh của dataset.
   - **Bài giảng và câu hỏi tự biên soạn**: đây là tác phẩm riêng, chỉ *tham chiếu* từ vựng. Nếu chép nguyên văn định nghĩa Wiktionary vào câu hỏi thì phần đó vẫn thuộc SA.
4. Nếu sau này muốn giữ độc quyền một phần nội dung (gói premium), phần đó phải là **nội dung tự biên soạn 100%**, có cờ `License = Proprietary`.

**Ưu điểm:** dùng hợp pháp được nguồn tốt nhất, kể cả khi có mục đích thương mại.
**Nhược điểm:** không thể "khóa" phần từ điển. Cần làm trang Attribution.
**Phương án thay thế:** tự xây từ điển từ đầu. Bị loại vì quá tốn công với một người.

## 4. Những gì tuyệt đối không làm
- Không scraping hay tải hàng loạt từ: Goethe, telc, DW, DWDS, các app luyện thi.
- Không nhập đề mẫu (Übungssätze), câu hỏi hay audio của Goethe/telc/DW vào hệ thống khi chưa có văn bản cho phép.
- Không nhúng lại audio của Tatoeba hoặc Commons nếu **trường license trống**.
- Không dùng nội dung CC BY-NC nếu có kế hoạch thu phí.
- Không ghi "chính thức" cho câu hỏi tự biên soạn. Gắn nhãn *"Bài kiểm tra do nền tảng biên soạn theo khung CEFR, không phải đề thi Goethe hay telc"*.

## 5. Danh sách NEEDS LEGAL VERIFICATION (cần làm trước Phase 4)

| # | Mục | Hành động |
|---|---|---|
| 1 | Giấy phép dữ liệu trích xuất của kaikki.org (trang không ghi) và điều khoản Wikimedia | Đọc Terms of Use của Wikimedia, kiểm tra README của wiktextract |
| 2 | Giấy phép từng gói tải của Leipzig (CC BY hay CC BY-NC) | Mở trang download bằng trình duyệt |
| 3 | Điều khoản của Goethe về việc **liên kết** tới Übungssätze | Đọc trang goethe.de bằng trình duyệt |
| 4 | Quyền lưu và phát lại audio tạo bởi Azure TTS | Đọc Microsoft Product Terms (mục Speech) |
| 5 | Giấy phép từng file audio phát âm trên Wikimedia Commons (được link từ Wiktionary) | Script ETL đọc metadata license của từng file, chỉ giữ file cho phép thương mại |
| 6 | Điều khoản liên kết và nhúng của DW, vhs-Lernportal | Đọc Impressum / Nutzungsbedingungen |

## 6. Pháp lý người dùng (tóm tắt, chi tiết ở Phase 2)
- **Nghị định 13/2023/NĐ-CP về bảo vệ dữ liệu cá nhân** và **Luật Bảo vệ dữ liệu cá nhân** (nếu đã có hiệu lực, 🟡 cần kiểm tra văn bản hiện hành). Cần chuẩn bị:
  - Chính sách quyền riêng tư.
  - Cơ chế xin đồng ý của người dùng.
  - Thu thập tối thiểu: email, tên hiển thị, mục tiêu học.
  - Quyền xóa tài khoản.
- **Điều khoản sử dụng**, có tuyên bố miễn trừ: không cam kết đỗ thi, được tuyển hay được cấp visa. Thông tin chỉ để tham khảo và có ngày cập nhật.
- Không giới hạn tuổi (Q-P2-04). Người dùng dưới 16 tuổi cần cha mẹ hoặc người giám hộ đồng ý, được nêu trong ô đồng ý (BR-AUTH-04). 🟡 Kiểm tra lại NĐ 13/2023 ở Phase 5.
