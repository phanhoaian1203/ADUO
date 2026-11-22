# 03 – Dataset & API Catalog

Mỗi nguồn được đánh giá theo 17 trường của mục 5.10. Mỗi bảng đặt nhiều nguồn cạnh nhau theo cột để dễ so sánh. Nhãn xác minh theo [00-README](00-README.md).

> **Cập nhật theo [CR-001](../change-requests/CR-001-german-first.md):** catalog chỉ còn các nguồn tiếng Đức, Ausbildung, Recognition và API dịch vụ.

## Tổng quan phân nhóm

| Nhóm | Nguồn |
|---|---|
| **A – Direct Integration** | Azure AI Speech (TTS de-DE), Claude API (dùng trong CMS), DeepL API Free (V2) |
| **B – Download & Import** | Wiktionary/kaikki DE, Tatoeba, OpenThesaurus (V2), Leipzig Corpora (⚖️ sau khi xác minh) |
| **C – External Reference** | Goethe Wortlisten và Übungssätze, telc, DW Deutsch lernen, vhs-Lernportal, DWDS, anabin, Anerkennung in Deutschland, BQ-Portal, make-it-in-germany, BERUFENET, BA Ausbildungssuche |
| **D – Not Recommended** | Repo GitHub chép lại Goethe list, Common Voice (làm audio bài học), file đề chia sẻ trên Scribd |

---

## 1. Tiếng Đức

| Trường | Wiktionary qua kaikki.org | Leipzig Corpora Collection | OpenThesaurus | DWDS |
|---|---|---|---|---|
| Language | DE (+ định nghĩa EN) | DE (+ nhiều ngôn ngữ khác) | DE | DE |
| Category | Vocabulary, Grammar (hình thái), IPA, Audio link | Corpus, Frequency, Example | Synonyms | Dictionary, Frequency, IPA |
| Provider | Wiktionary / Tatu Ylonen | Universität Leipzig | openthesaurus.de | BBAW |
| Source URL | https://kaikki.org/dictionary/rawdata.html | https://wortschatz.uni-leipzig.de/en/download | https://www.openthesaurus.de/about/download | https://www.dwds.de/d/api |
| Resource Type | Dataset | Dataset | API + Dataset | API |
| Availability | Free | Free | Free | Free (xem online) |
| License | 🟡 Trang không ghi. Nội dung gốc Wiktionary là CC BY-SA | 🟡 Text corpora: CC BY. Dữ liệu khác: CC BY-NC (theo trang Usage, đọc gián tiếp) ⚖️ | ✅ CC BY-SA 4.0 hoặc LGPL | ✅ Điều khoản riêng: mọi việc tái sử dụng cần phép |
| Commercial Use | ✅ (CC BY-SA) | ⚖️ tùy từng gói | ✅ | ⛔ |
| Redistribution | ✅ có điều kiện SA | ⚖️ | ✅ (có link nguồn) | ⛔ |
| API Support | Không | Có REST API (chưa kiểm tra) | ✅ 60 request/phút | ✅ |
| Download Support | ✅ `de-extract.jsonl.gz` 295,8 MB | ✅ gói 10K–1M câu | ✅ MySQL dump 6,41 MB | Tải danh sách đầu mục (theo điều khoản) |
| Data Format | JSONL | TXT (UTF-8) + script MySQL | TXT, SQL | JSON |
| Quality | Cao nhưng không đồng nhất. Có giống, số nhiều, chia động từ | Cao (corpus thật) | Trung bình | Rất cao |
| Integration | Script ETL lọc theo danh sách từ mục tiêu | Lấy danh sách tần suất + câu ngắn | V2 | Chỉ liên kết |
| Limitations | File lớn. Nghĩa giải thích bằng tiếng Anh, cần dịch sang tiếng Việt | Câu tin tức khó với A1 | Ít giá trị ở A1 | Cấm crawl và TDM |
| Recommendation | **B (chính)** | **B ⚖️** | **B (V2)** | **C** |

| Trường | Goethe Wortlisten A1/A2/B1 | Goethe / telc Übungssätze | DW Deutsch lernen | vhs-Lernportal |
|---|---|---|---|---|
| Provider | Goethe-Institut | Goethe-Institut / telc gGmbH | Deutsche Welle | Deutscher Volkshochschul-Verband |
| Source URL | https://www.goethe.de/pro/relaunch/prf/sw/Goethe-Zertifikat_A1_Fit1_Wortliste.pdf | https://www.goethe.de/de/spr/prf/ueb/pa1.html | https://www.dw.com/de/deutsch-lernen/s-2055 | https://www.vhs-lernportal.de/ |
| Type | PDF | PDF + audio | Website (khóa học) | Website (khóa học) |
| License | ✅ Có bản quyền (DWDS cũng ghi chú như vậy) | 🟡 có bản quyền | 🟡 không truy cập được | 🟡 không đọc được điều khoản |
| Commercial / Redistribution | ⛔ | ⛔ | ⚖️ | ⚖️ |
| Recommendation | **C**: dùng nội bộ để đối chiếu độ phủ | **C** | **C** | **C** |

---

## 2. Ausbildung / Recognition (đều là Group C, nguồn thông tin chính thức)

| Trường | anabin | BQ-Portal | Anerkennung in Deutschland | Make it in Germany | BA Jobsuche / Ausbildungssuche API |
|---|---|---|---|---|---|
| Provider | KMK – ZAB | IW, thay mặt BMWE | BIBB | Chính phủ Liên bang Đức | Bundesagentur für Arbeit (qua bund.dev) |
| URL | https://anabin.kmk.org/ | https://www.bq-portal.de/ | https://www.anerkennung-in-deutschland.de/ | https://www.make-it-in-germany.com/ | https://ausbildungssuche.api.bund.dev/ |
| Nội dung | Đánh giá bằng học thuật, trường, bằng cấp | Hệ thống đào tạo nghề 108 quốc gia, 6.832 hồ sơ nghề | Anerkennungs-Finder: nghề tham chiếu, cơ quan thẩm quyền | Visa, Ausbildung, đời sống | Tin tuyển chỗ học nghề |
| Trạng thái | ✅ | ✅ (có **API mới, miễn phí, cần liên hệ**) | 🟡 lỗi DNS khi truy cập | 🟡 chặn bot | 🟡 không render được |
| Đề xuất | C | C (API: V3) | C | C | C ở MVP, cân nhắc A ở V3 |

---

## 3. API dịch vụ

| Trường | Azure AI Speech | DeepL API Free | Claude API | Google Cloud TTS |
|---|---|---|---|---|
| Chức năng | TTS, STT, Pronunciation Assessment | Dịch | LLM: sinh và phân loại nội dung | TTS |
| Language | de-DE 🟡 (cần đọc voice list) | 🟡 VI chưa xác minh | Đa ngữ | 🟡 |
| Auth | Subscription key + region | API key | API key | Service account |
| Rate / Free tier | ✅ 0,5 triệu ký tự TTS neural/tháng. 5 giờ STT/tháng | ✅ 50.000 ký tự/tháng | Không có free tier. Haiku 5.5 $0,10 / $0,50 cho mỗi triệu token | 🟡 |
| Response | Audio (mp3/ogg/wav) | JSON | JSON | Audio |
| Lưu kết quả | ⚖️ Chưa thấy điều khoản cấm. Cần đọc Microsoft Product Terms | ⚖️ cần đọc điều khoản | ✅ Output thuộc người dùng (theo Commercial Terms, cần xác nhận lại) ⚖️ | ⚖️ |
| .NET | ✅ SDK NuGet chính thức | ✅ DeepL.net | ✅ SDK C# | ✅ |
| Recommendation | **A**: tạo audio **một lần** trong CMS, lưu lên storage | V2 (hỗ trợ dịch cho Admin) | **A trong CMS**, không public ở MVP | Dự phòng |

> Ghi chú: Bất kỳ ô nào ghi 🟡 hoặc ⚖️ đều phải được xác minh lại trước khi dùng thật. Những ô đó **không** phải kết luận cuối cùng.
