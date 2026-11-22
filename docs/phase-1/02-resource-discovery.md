# 02 – Resource Discovery Report (tiếng Đức, Ausbildung, Recognition)

Ngày nghiên cứu: 2026-11-22. Nhãn xác minh theo quy ước ở [00-README.md](00-README.md).

> **Cập nhật theo [CR-001](../change-requests/CR-001-german-first.md):** dự án tập trung 100% vào tiếng Đức.

---

## 1. Tiếng Đức

### 1.1 Wiktionary qua kaikki.org (wiktextract) – ✅ Đã xác minh (dữ liệu) / 🟡 (giấy phép)
- [kaikki.org/dictionary/rawdata.html](https://kaikki.org/dictionary/rawdata.html):
  - Dump JSONL, trích xuất từ Wiktionary.
  - Bản trích từ Wiktionary tiếng Đức: `de-extract.jsonl.gz` 295,8 MB (2,9 GB khi giải nén).
  - Bản tiếng Đức trong Wiktionary tiếng Anh: 353.033 dạng từ (file 1,0 GB, đã deprecated).
  - Cập nhật khoảng hàng tuần, dump ngày 2026-10-01.
- Trang **không ghi giấy phép**, chỉ đề nghị trích dẫn Ylonen (LREC 2022). Nội dung gốc từ Wiktionary là CC BY-SA (theo điều khoản Wikimedia; chưa đọc trực tiếp trong đợt này).
- Chứa **giống danh từ (der/die/das), số nhiều, bảng chia động từ, IPA, link audio** (audio nằm trên Wikimedia Commons, mỗi file có giấy phép riêng).
- **Đánh giá:** nguồn **tốt nhất** cho từ vựng và ngữ pháp hình thái tiếng Đức (Group B). Có điều kiện share-alike.

### 1.2 OpenThesaurus – ✅ Đã xác minh
- [API](https://www.openthesaurus.de/about/api):
  - Miễn phí, giới hạn 60 request/phút/IP.
  - User-Agent phải có thông tin liên hệ.
  - Phải đặt link tới openthesaurus.de.
  - Dùng số lượng lớn thì nên tải bản download.
- [Download](https://www.openthesaurus.de/about/download):
  - Giấy phép kép CC BY-SA 4.0 hoặc LGPL, được phân phối lại.
  - Định dạng: text zip, MySQL dump 6,41 MB.
- **Giá trị:** chỉ là từ đồng nghĩa, nên **thấp cho A1**. Để V2.

### 1.3 DWDS – ✅ Đã xác minh → ⛔ cho việc lưu trữ
- [API](https://www.dwds.de/d/api) có: tần suất từ, snippet từ điển, IPA, và danh sách từ Goethe A1/A2/B1 dạng CSV/JSON.
- [Điều khoản](https://www.dwds.de/d/nutzungsbedingungen): mọi việc sử dụng nội dung (gồm truy vấn tự động, crawl, TDM) **chỉ được phép khi có sự cho phép rõ ràng**.
- Chính DWDS ghi danh sách từ Goethe là có bản quyền của Goethe-Institut.
- **Group C** (liên kết tra cứu). ⚖️ Nếu muốn dùng tần suất từ để xếp hạng, phải xin phép.

### 1.4 Leipzig Corpora Collection – 🟡
- Trang download chặn bot.
- Theo [trang Terms of Usage](https://wortschatz.uni-leipzig.de/en/usage) (qua kết quả tìm kiếm, chưa đọc trực tiếp): các **corpus văn bản tải về dùng CC BY**, còn dữ liệu và ứng dụng khác dùng **CC BY-NC**.
- Định dạng: text UTF-8 (sentences, words kèm tần suất), có script import MySQL. Kích thước chuẩn 10K đến 1M câu.
- **Giá trị:** lấy **danh sách tần suất từ** để xếp thứ tự học, và **câu ví dụ thật**. ⚖️ Phải đọc lại giấy phép của từng gói trước khi tải.

### 1.5 Danh sách từ Goethe (A1 / A2 / B1) – ✅ tồn tại / ⛔ sao chép
- PDF chính thức:
  - [A1 Fit1](https://www.goethe.de/pro/relaunch/prf/sw/Goethe-Zertifikat_A1_Fit1_Wortliste.pdf)
  - [A1 SD1](https://www.goethe.de/pro/relaunch/prf/ca/A1_SD1_Wortliste_02.pdf)
  - [B1](https://www.goethe.de/pro/relaunch/prf/da/Goethe-Zertifikat_B1_Wortliste.pdf)
- Có ghi bản quyền. A2 có khoảng 1.300 đơn vị từ vựng.
- [Repo GitHub chép lại danh sách](https://github.com/ilkermeliksitki/goethe-institute-wordlist) xếp **Group D**, vì là bản sao không được phép.
- **Cách dùng hợp pháp:** dùng PDF làm **danh sách kiểm tra độ phủ** (cho Admin đối chiếu nội bộ). Mục từ, nghĩa, ví dụ lấy từ Wiktionary hoặc tự viết. Một danh sách từ đơn lẻ (như "der Tisch") nói chung không được bảo hộ, nhưng **tập hợp và cách sắp xếp** của danh sách thì có thể được bảo hộ. ⚖️ Không xuất bản lại "danh sách Goethe A1" dưới dạng một bộ hoàn chỉnh.

### 1.6 Đề luyện thi Goethe / telc – 🟡 / Group C
- goethe.de/prf trả lỗi 403. Đề mẫu (Übungssätze) là tài liệu có bản quyền.
- Chỉ **liên kết** tới trang chính thức. Tự soạn đề theo **định dạng** (format bài thi không được bảo hộ, nội dung câu hỏi thì có).

### 1.7 DW Deutsch lernen, vhs-Lernportal – 🟡 / Group C
- dw.com từ chối truy cập. vhs-Lernportal chỉ đọc được trang giới thiệu "khóa học online Deutsch als Zweitsprache".
- Cả hai là khóa học miễn phí chất lượng cao. Làm **liên kết "học thêm"** trong Resource Library. Không nhúng hay sao chép.

### 1.8 Audio phát âm
| Nguồn | Trạng thái | Ghi chú |
|---|---|---|
| Wikimedia Commons (qua link trong Wiktionary) | 🟡 | Mỗi file một giấy phép (thường CC BY-SA hoặc CC0). Phải lưu license của từng file |
| Lingua Libre | 🟡 | Đóng góp lên Commons. Chưa kiểm tra trong đợt này |
| Mozilla Common Voice | 🟡 CC0 (theo [Wikipedia](https://en.wikipedia.org/wiki/Common_Voice) và FAQ) | Là dữ liệu huấn luyện ASR (câu đọc bởi tình nguyện viên, chất lượng không đồng đều). **Không phù hợp làm audio bài học** |
| TTS (Azure / Google) | ✅ một phần | Phương án thực tế nhất để có audio đồng nhất. Xem §3 |


### 1.9 Câu ví dụ – Tatoeba – ✅ Đã xác minh
Nguồn: [tatoeba.org/en/downloads](https://tatoeba.org/en/downloads)
- **Câu:** CC BY 2.0 FR. Một phần có CC0 1.0 và có bản tải riêng chỉ gồm các câu CC0.
- **Audio:** giấy phép do người đóng góp chọn. Nếu trường license trống thì **không được dùng ngoài Tatoeba**.
- **Định dạng:** tar.bz2 chứa TSV (sentences, links, sentences_with_audio). Cập nhật mỗi thứ Bảy.
- Có câu tiếng Đức, liên kết được với bản dịch (có thể có tiếng Việt). **Chất lượng không đồng đều**, cần lọc và duyệt.



### 1.10 Cấu trúc bài thi Goethe-Zertifikat A1: Start Deutsch 1 (dùng làm chuẩn tham chiếu) – 🟡 một phần
Theo [Durchführungsbestimmungen A1 (Goethe-Institut)](https://www.goethe.de/pro/relaunch/prf/ar/Durchfuehrungsbestimmungen_A1_Start_Deutsch_1.pdf) và [trang thông tin Goethe](https://www.goethe.de/ins/bo/de/spr/prf/gzsd1/inf.html), qua kết quả tìm kiếm (trang goethe.de trả lỗi 403 khi đọc trực tiếp):
- Gồm 4 phần: **Hören, Lesen, Schreiben, Sprechen** (Sprechen thi theo nhóm).
- Phần thi viết kéo dài **65 phút, không nghỉ**: Hören khoảng 20 phút, Lesen 25 phút, Schreiben 20 phút.
- ⚪ **Chưa xác minh được:** thời lượng Sprechen, điểm từng phần, ngưỡng đỗ, số lần phát audio của từng Teil.
- **Cách dùng trong dự án:** chỉ mượn **khung thời lượng và kỹ năng** để thiết kế "Bài kiểm tra cuối A1" của nền tảng. **Không** sao chép đề, **không** gọi bài kiểm tra là "đề Goethe", **không** ước tính "điểm Goethe".

---


## 2. Ausbildung và Recognition

### 2.1 Ausbildung – visa §16a AufenthG – 🟡
Trang make-it-in-germany.com chặn bot. Thông tin dưới đây lấy từ kết quả tìm kiếm trích nguyên văn trang này và các trang IHK:

- **Ngoại ngữ:** thường yêu cầu **B1**. Có ngoại lệ nếu doanh nghiệp đào tạo xác nhận người học đủ trình độ, hoặc nếu có khóa học tiếng chuẩn bị ([make-it-in-germany – Berufsausbildung](https://www.make-it-in-germany.com/de/visum-aufenthalt/arten/berufsausbildung), [IHK Schwaben](https://www.ihk.de/schwaben/produktmarken/fachkraefte-fuer-schwaben/internationale-auszubildende/visum-berufsausbildung-4711648)).
- **Các điều kiện khác:**
  - Có hợp đồng đào tạo cho một nghề được công nhận.
  - Doanh nghiệp có quyền đào tạo theo BBiG.
  - Chứng minh tài chính (IHK ghi mức 1.048 EUR/tháng, thời điểm 06/2026).
  - Visa tìm chỗ học nghề có giới hạn tuổi 35.
- ⚠️ Đây là **số liệu thay đổi theo năm**. Theo nguyên tắc dự án: **không tự viết con số** trong Info Hub. Chỉ trích dẫn kèm nguồn và ngày truy cập, và nhắc người học kiểm tra lại.
- **Các API tìm chỗ học nghề:**
  - [Jobsuche API](https://jobsuche.api.bund.dev/) và [Ausbildungssuche API](https://ausbildungssuche.api.bund.dev/) (bund.dev, Bundesagentur für Arbeit): trang tồn tại nhưng không render được nội dung. 🟡
  - BERUFENET (thông tin nghề): 🟡, chưa xác định có API.
  - **Đề xuất:** chỉ liên kết ở MVP. Có thể tích hợp ở V3.

### 2.2 Recognition – phân biệt ba loại thủ tục
| Loại | Áp dụng khi | Cơ sở / công cụ chính thức | Trạng thái |
|---|---|---|---|
| **Academic Recognition** (đánh giá bằng cấp học thuật) | Đi học đại học, hoặc cần chứng minh giá trị bằng đại học | **anabin** (KMK/ZAB, [anabin.kmk.org](https://anabin.kmk.org/)): dữ liệu về bằng cấp, trường và trạng thái. **Zeugnisbewertung** của ZAB ([zab.kmk.org](https://zab.kmk.org/de/zeugnisbewertung)) | ✅ anabin đã đọc |
| **Professional Recognition – nghề được quản lý** (reglementierte Berufe, ví dụ điều dưỡng, bác sĩ, giáo viên) | **Bắt buộc** phải được công nhận mới được hành nghề | Cơ quan có thẩm quyền của từng bang, tùy nghề. Tra bằng **Anerkennungs-Finder** trên anerkennung-in-deutschland.de (BIBB) | 🟡 trang không truy cập được trong đợt này |
| **Vocational Qualification Recognition** – nghề **không** quản lý, nghề đào tạo kép (theo BQFG) | Không bắt buộc, nhưng giúp ích khi xin việc và xin visa | Thường do IHK/HWK (IHK FOSA) xử lý. **BQ-Portal** ([bq-portal.de](https://www.bq-portal.de/), IW/BMWE) cung cấp thông tin hệ thống đào tạo của 108 quốc gia và 6.832 hồ sơ nghề, **có API mới (miễn phí, cần liên hệ)** | ✅ BQ-Portal đã đọc |

anabin tự giới thiệu anabin, "Anerkennung in Deutschland" và BQ-Portal là **"ba nguồn thông tin trung tâm"**. Info Hub nên dẫn đúng ba nguồn này và giải thích nên dùng nguồn nào trong tình huống nào.

---

## 3. API dịch vụ

| API | Chức năng | Free tier đã xác minh | Ghi chú | Tích hợp .NET |
|---|---|---|---|---|
| **Azure AI Speech** | TTS neural, STT, Pronunciation Assessment | ✅ F0: **0,5 triệu ký tự TTS neural/tháng**, **5 giờ STT/tháng** ([pricing](https://azure.microsoft.com/en-us/pricing/details/cognitive-services/speech-services/)) | Giá trả phí không đọc được trên trang. Danh sách giọng de-DE 🟡 (chưa đọc trang voice list). Không tìm thấy điều khoản cấm lưu audio đã tổng hợp ⚖️, cần đọc Product Terms | SDK chính thức `Microsoft.CognitiveServices.Speech` (NuGet) |
| Google Cloud TTS | TTS | 🟡 trang giá không đọc được | Phương án thay thế | REST / Google.Cloud.TextToSpeech.V1 |
| **DeepL API Free** | Dịch thuật | ✅ **50.000 ký tự/tháng** ([pro-api](https://www.deepl.com/pro-api)) | Danh sách ngôn ngữ (có tiếng Việt không) 🟡 | REST, có SDK `DeepL.net` |
| **Claude API (Anthropic)** | Sinh bản nháp nội dung, giải thích, phân loại | ✅ Giá theo [platform.claude.com/pricing](https://platform.claude.com/docs/en/about-claude/pricing): Haiku 5.5 có $0,10 / $0,50 cho mỗi triệu token (input/output, prompt ≤ 100K). Sonnet 5.5 có $2 / $10. Batch giảm 50% | Dùng **offline trong CMS** (sinh nháp, sau đó duyệt), không gọi realtime cho người học ở MVP | REST, có SDK C# |
| OpenThesaurus | Từ đồng nghĩa DE | ✅ 60 request/phút | V2 | HttpClient + JSON |

**Ước tính chi phí AI cho việc tạo nội dung MVP (giả định):** 3.000 mục nội dung × khoảng 2.000 token mỗi mục ≈ 6 triệu token. Dùng Haiku 5.5 thì tốn khoảng **dưới 5 USD**. Dùng Sonnet 5.5 kèm Batch khoảng **20–40 USD**. Đây là ước tính, không phải báo giá.

---

## 4. Dự án GitHub / mã nguồn tham khảo
| Dự án | Dùng làm gì | Trạng thái |
|---|---|---|
| [tatuylonen/wiktextract](https://github.com/tatuylonen/wiktextract) | Hiểu schema JSON của kaikki | ✅ có tồn tại; license code chưa kiểm tra |
| FSRS (open-spaced-repetition) | Thuật toán spaced repetition cho V2 | 🟡 chưa kiểm tra trong đợt này |

---


## 5. Tổng kết: dùng gì cho MVP (chỉ tiếng Đức)

| Nhu cầu MVP | Nguồn | Nhóm |
|---|---|---|
| Từ vựng, giống danh từ, số nhiều, chia động từ, IPA | Wiktionary qua kaikki (`de-extract`) | B (CC BY-SA) |
| Thứ tự học theo tần suất | Leipzig (sau khi xác minh license của gói tải) | B ⚖️ |
| Câu ví dụ | Tatoeba (ưu tiên tập CC0), Wiktionary | B |
| Audio | Azure TTS giọng de-DE. Bổ sung audio Commons có license rõ ràng | A |
| Phạm vi từ vựng A1 (đối chiếu độ phủ) | PDF Goethe A1 Wortliste (chỉ dùng nội bộ) | C |
| Khung thời lượng và kỹ năng của bài kiểm tra A1 | Durchführungsbestimmungen Goethe A1 (tham chiếu) | C |
| Bài giảng ngữ pháp, câu hỏi, bài kiểm tra | **Tự xây dựng**: AI viết nháp, kiểm tra tự động, rồi duyệt | — |
| Học thêm ngoài nền tảng | DW Deutsch lernen, vhs-Lernportal, Goethe Übungssätze (liên kết) | C |
| Info Hub | make-it-in-germany, anabin, Anerkennung in Deutschland, BQ-Portal, IHK (liên kết + tóm tắt có nguồn) | C |
