# Module 3 – Lexicon (từ vựng & ngữ pháp tiếng Đức)

> Lexicon là **kho tri thức ngôn ngữ dùng chung**. Bài học và câu hỏi không chép từ vựng vào bên trong mình mà *tham chiếu* tới Lexicon. Vì vậy sửa một lỗi ở một chỗ là mọi nơi dùng từ đó đều đúng theo (BR-CAT-04).

## 1. Mục tiêu & phạm vi
| Trong phạm vi | Ngoài phạm vi |
|---|---|
| Mục từ tiếng Đức có đủ thông tin hình thái (giống, số nhiều, chia động từ Präsens; Perfekt từ A2) | Từ điển song ngữ đầy đủ (chỉ phạm vi A1, sau đó A2…) |
| Câu ví dụ có bản dịch tiếng Việt, audio | Phân tích ngữ pháp câu tự động (NLP) |
| Chủ đề (gia đình, mua sắm…) | Tra cứu realtime qua API ngoài (AD-04) |
| Điểm ngữ pháp có giải thích tiếng Việt | |
| Tìm kiếm có và không có Umlaut, tiếng Việt không dấu | |
| Admin CRUD + importer CSV/JSON + CLI import Wiktionary và Tatoeba | |
| "Sổ từ của tôi" (Should) | Flashcards, SRS (V2) |

Nguồn yêu cầu: UC-10, 11, 22 · FR-LEX-01..07 · BR-LEX-01..04, BR-CNT-04, 10 · US-13, 14.

## 2. Kiến thức bạn sẽ học
| Khái niệm | Giải thích | Trong module |
|---|---|---|
| **Mô hình hóa dữ liệu ngôn ngữ** | Một "từ" có nhiều dạng: danh từ có giống và số nhiều, động từ có ngôi và thì. Cần chọn phần nào là cột, phần nào là JSON, và lý do chọn | `vocabulary_entries` + `forms` jsonb |
| **Khóa tự nhiên vs khóa đại diện** | `id` (UUID) là khóa đại diện. `(lemma, loại từ, số đồng âm)` là khóa tự nhiên, được dùng để chống trùng khi import | UQ index |
| **Đồng âm khác nghĩa (homograph)** | "der See" (hồ) và "die See" (biển) là hai mục từ khác nhau. Cột `homograph_no` và `gender` phân biệt chúng | BR-CNT-10 |
| **Chuẩn hóa để tìm kiếm** | Lưu thêm cột "phiên bản tìm kiếm" của chữ (đã bỏ dấu, chuyển thường) để so khớp nhanh | `lemma_search`, `meaning_vi_search` |
| **Trigram (pg_trgm)** | Tách chuỗi thành các cụm 3 ký tự rồi so độ giống nhau. Nhờ vậy tìm được cả khi gõ sai nhẹ, và dùng được index GIN | `WHERE lemma_search % 'maedchn'` |
| **Streaming file lớn** | Đọc file 2,9 GB từng dòng một (`IAsyncEnumerable`) thay vì nạp hết vào RAM | CLI import |
| **Mapping dữ liệu bên ngoài** | Dữ liệu Wiktionary có cấu trúc riêng (`pos`, `forms`, `tags`, `senses`). Phải viết lớp ánh xạ sang mô hình của mình và ghi lại các trường hợp không ánh xạ được | `WiktionaryMapper` |
| **Share-alike trong thực tế** | Nghĩa tiếng Việt được dịch từ định nghĩa của Wiktionary là tác phẩm phái sinh, nên giữ license CC BY-SA. Hệ thống phải ghi rõ điều này | BR-CNT-04 |
| **Value object** | `GermanNounInfo(gender, plural, number)`, `VerbForms(...)` tự kiểm tra tính hợp lệ của chính nó | Domain |

## 3. Mô hình miền

### 3.1 `VocabularyEntry` (🧾 có ContentMeta)
| Thuộc tính | Ví dụ | Quy tắc |
|---|---|---|
| `Lemma` | `Mädchen`, `anrufen`, `groß` | Bắt buộc. Danh từ viết hoa chữ đầu |
| `LemmaSearch` | `maedchen` | Tự sinh bằng `GermanTextNormalizer` |
| `HomographNo` | 1 | ≥ 1 |
| `PartOfSpeech` | `Noun` | Enum |
| `Gender` | `n` | Bắt buộc với danh từ khi publish (BR-LEX-01) |
| `Plural` / `NounNumber` | `Mädchen` / `Normal` | `NoPlural` (ví dụ *die Milch*) thì `Plural` null. `PluralOnly` (ví dụ *die Eltern*) thì `Gender` null, và UI hiển thị "chỉ có số nhiều" |
| `Ipa` | `ˈmɛːtçən` | Tùy chọn |
| `IsSeparable`, `IsIrregular` | true / true (anrufen) | Chỉ có nghĩa với động từ |
| `Forms` (jsonb) | Xem [Phase 3 – 02 §3.3](../../phase-3/02-database-design.md) | A1: động từ có đủ 6 ngôi `praesens`. A2+: thêm `perfekt` (BR-LEX-02) |
| `CefrLevel` | `A1` | |
| `MeaningVi` | `cô gái, bé gái` | Bắt buộc. ≤ 200 ký tự |
| `MeaningViSearch` | `co gai, be gai` | Tự sinh |
| `NotesVi` | `Danh từ giống trung dù chỉ người nữ (vì đuôi -chen)` | Tùy chọn. Đây là chỗ để **giải thích mẹo ngữ pháp** |
| `AudioMediaId` | | Khuyến nghị có. Thiếu thì **cảnh báo** chứ không chặn publish |
| `Topics` | `gia-dinh` | N-N |
| `Examples` | 1–3 câu | Khuyến nghị ≥ 1 |

**Hàm domain:**
- `ValidateForPublish()`: trả danh sách lỗi như `MISSING_GENDER`, `MISSING_PLURAL_INFO`, `MISSING_PRAESENS_FORMS`, `MISSING_MEANING`. ContentOps `PublishGuard` gọi hàm này qua `IPublishable`.
- `DisplayLabel()`: danh từ hiển thị `"das Mädchen"`, động từ hiển thị `"anrufen"`.
- `TtsText()`: danh từ đọc `"das Mädchen"` (kèm mạo từ, xem ContentOps 4.8).

### 3.2 `ExampleSentence` (🧾)
`TextDe`, `TextVi`, `CefrLevel?`, `AudioMediaId?`. Một câu có thể làm ví dụ cho nhiều mục từ và nhiều điểm ngữ pháp.

### 3.3 `GrammarPoint` (🧾)
| Thuộc tính | Ví dụ |
|---|---|
| `Slug` | `possessivartikel-mein-dein` |
| `TitleVi` | Mạo từ sở hữu mein / dein |
| `Structure` | `mein + danh từ giống đực/trung · meine + danh từ giống cái/số nhiều` |
| `ExplanationMd` | Giải thích bằng tiếng Việt (Markdown), có bảng |
| `CommonMistakesMd` | "Sai: *mein Mutter* → Đúng: *meine Mutter*" |
| `CefrLevel` | A1 |
| `Examples` | ≥ 2 câu có audio (US-14) |
| `PracticeSetId` | Liên kết sang Assessment (bộ câu hỏi luyện riêng điểm ngữ pháp này) |

### 3.4 `Topic`
`Slug`, `TitleVi`. Seed khoảng 15 chủ đề A1: chào hỏi, bản thân, gia đình, số đếm, thời gian, ăn uống, mua sắm, nhà ở, thành phố, giao thông, công việc, sức khỏe, thời tiết, quần áo, sở thích.

## 4. Chức năng chi tiết

### 4.1 Tìm kiếm từ (US-13, BR-LEX-04)
**Thuật toán:**
1. Chuẩn hóa từ khóa `q` bằng **cả hai** normalizer. Ví dụ `"Mädchen"` → de: `maedchen`, vi: `madchen`. Còn `"cô gái"` → `co gai`.
2. Truy vấn:
   ```sql
   SELECT … FROM lexicon.vocabulary_entries
   WHERE status = 'Published'
     AND (lemma_search ILIKE :q || '%'            -- khớp tiền tố (ưu tiên)
          OR lemma_search % :q                    -- gần đúng (trigram)
          OR meaning_vi_search ILIKE '%' || :qvi || '%')
   ORDER BY (lemma_search = :q) DESC,             -- khớp chính xác lên đầu
            (lemma_search LIKE :q || '%') DESC,
            similarity(lemma_search, :q) DESC
   LIMIT 20;
   ```
3. Lọc thêm theo `cefrLevel`, `topic`, `pos` nếu có.

**Hiệu năng:** có GIN index trigram trên hai cột search. Với khoảng 650 đến vài nghìn mục từ thì truy vấn chạy dưới 10 ms.

### 4.2 Chi tiết từ
- **Danh từ:** `GenderBadge` (màu + chữ der/die/das), số nhiều, IPA, audio, nghĩa, ghi chú, ví dụ, ghi công nguồn.
- **Động từ:** `ConjugationTable` (6 ngôi). Động từ tách hiển thị theo mẫu `rufe … an`, kèm chú thích "phần *an* đứng cuối câu". Có nhãn "bất quy tắc" nếu cần.
- **Tính từ, trạng từ, cụm từ:** hiển thị dạng đơn giản.
- **Ghi công:** dòng nhỏ "Nguồn: Wiktionary · CC BY-SA 4.0 · [link]". Ghi công lấy từ `source_id`, `license_id`, `original_ref` (BR-CNT-02).

### 4.3 Admin – CRUD từ vựng
Form sửa từ **thay đổi theo loại từ**:
- Chọn `Noun` thì hiện giống và số nhiều.
- Chọn `Verb` thì hiện lưới chia động từ 6 ô, có nút "Điền từ Wiktionary" nếu `original_ref` có sẵn dữ liệu.

Ngoài ra:
- Nút "Tạo audio" gọi TTS (ContentOps 2b). Trước khi có 2b, Admin upload file audio thủ công.
- Lọc danh sách theo `status`, `cefrLevel`, `pos`, `origin`, "thiếu audio", "thiếu ví dụ".

### 4.4 Importer CSV/JSON (dùng pipeline của ContentOps)
- `VocabularyCsvImporter`: các cột theo template ở [Phase 3 – 03 §9](../../phase-3/03-api-design.md).
- `VocabularyJsonImporter`: dùng khi có `forms` (bảng chia động từ).
- Khóa tự nhiên: `de|{lemma}|{pos}|{homograph_no}`.

### 4.5 CLI `import wiktionary`: con đường chính để có khoảng 650 từ A1
```
aduo-cli import wiktionary \
   --file data/raw/de-extract.jsonl.gz \
   --wordlist data/a1-lemmas.txt \
   --level A1 --dry-run
```

**Đầu vào:**
1. File dump `de-extract.jsonl.gz` của kaikki (khoảng 296 MB, Phase 1 file 02 §1.1).
2. `a1-lemmas.txt`: danh sách lemma **do bạn tự lập** khi đối chiếu độ phủ với PDF Goethe A1 Wortliste (dùng nội bộ, không publish danh sách này, xem Phase 1 file 04).

**Xử lý:**
- Đọc file từng dòng (gzip stream).
- Chỉ giữ những entry có `word` nằm trong wordlist, và `pos` thuộc loại từ cần dùng.
- Ánh xạ dữ liệu:

  | Wiktionary | → Lexicon |
  |---|---|
  | `pos: noun` + tag giới tính trong `head_templates` / `forms` | `Gender` |
  | `forms` có tag `plural` + `nominative` | `Plural` |
  | `forms` có tag `present` + ngôi | `Forms.praesens` |
  | `forms` có tag `past participle`, trợ động từ | `Forms.perfekt` |
  | `sounds[].ipa` | `Ipa` |
  | `sounds[].audio` + URL Commons | Ứng viên audio (**bắt buộc kiểm tra license từng file**, BR-CNT-11) |
  | `senses[].glosses` | Ghi vào cột tạm `gloss_en` (staging). Sau đó AI dịch sang `MeaningVi` |
  | Tên trang + chỉ số sense | `OriginalRef` |

- Từ có nhiều entry (đồng âm) thì tạo nhiều `HomographNo`.
- Ghi theo lô 500 dòng vào trạng thái **Draft**, kèm Source = Wiktionary, License = CC BY-SA.

**Đầu ra:** báo cáo gồm số từ tìm thấy, số từ **không tìm thấy** (để bạn tự bổ sung bằng tay), số từ thiếu giống hoặc thiếu số nhiều, số từ có nhiều nghĩa.

⚠️ **Cấu trúc JSON của wiktextract cần được kiểm tra trên dữ liệu thật** trước khi viết mapper. Các tag cụ thể có thể khác với dự kiến. Đây là bước "khám phá dữ liệu" đầu tiên của module.

**Nghĩa tiếng Việt:** sau khi import, chạy job AI "dịch gloss sang tiếng Việt ngắn gọn, phù hợp người học A1" (ContentOps 2b). Trước khi có 2b, bạn nhập nghĩa bằng tay hoặc qua CSV. Bản dịch này giữ license CC BY-SA (BR-CNT-04).

### 4.6 CLI `import tatoeba`
```
aduo-cli import tatoeba --sentences sentences.tar.bz2 --links links.tar.bz2 --lang deu --target vie --cc0-only
```
- Chỉ lấy câu tiếng Đức có liên kết tới câu tiếng Việt. Nếu thiếu thì để `TextVi` trống, AI dịch sau và mục đó bắt buộc qua duyệt.
- Lọc theo độ dài (≤ 12 từ cho A1) và theo việc câu có chứa lemma mục tiêu.
- Ghi Source = Tatoeba, License = CC0 hoặc CC BY 2.0 FR theo từng câu.
- Không lấy audio của Tatoeba nếu trường license trống (Phase 1 file 02).

### 4.7 Sổ từ của tôi (Should)
Bảng nhỏ `lexicon.saved_entries(user_id, entry_id, saved_at)`, có nút ☆ trên thẻ từ. Đây là tiền đề cho Flashcards ở V2.

## 5. Dữ liệu & API
- Bảng: [Phase 3 – 02 §3.3](../../phase-3/02-database-design.md). Nếu làm Sổ từ thì thêm `saved_entries`.
- API: [Phase 3 – 03 §4](../../phase-3/03-api-design.md). Thêm `GET/POST/DELETE /me/saved-entries` nếu làm Sổ từ.
- Interface cho module khác: `ILexiconReader.GetEntriesAsync(ids)`, `GetGrammarPointAsync(id)`. Catalog dùng các hàm này để dựng khối bài học.

## 6. Giao diện
| Màn hình | Ghi chú |
|---|---|
| Tra từ (`/tu-vung`) | Ô tìm kiếm có debounce 300 ms. Chip lọc theo cấp độ và chủ đề. Danh sách thẻ từ. Trạng thái rỗng |
| Chi tiết từ | Bottom sheet khi mở từ bài học, trang riêng khi mở từ tra cứu |
| Ngữ pháp (`/ngu-phap`) | Danh sách theo cấp độ. Chi tiết gồm cấu trúc, giải thích, bảng, ví dụ có audio, lỗi thường gặp, nút "Luyện tập" |
| Admin Từ vựng | DataTable + form động theo loại từ + tab Lịch sử |
| Admin Ngữ pháp | Form + MarkdownEditor + chọn câu ví dụ |

## 7. Test cases
| ID | Kịch bản | Kết quả mong đợi |
|---|---|---|
| LX-T01 | Publish danh từ thiếu giống | 422 `MISSING_GENDER` |
| LX-T02 | Publish danh từ `PluralOnly` (die Eltern) không có giống | OK |
| LX-T03 | Publish động từ A1 thiếu ngôi `du` | 422 `MISSING_PRAESENS_FORMS` |
| LX-T04 | Tìm "maedchen" | Ra *das Mädchen* ở vị trí đầu |
| LX-T05 | Tìm "Mädchen" | Cùng kết quả với LX-T04 |
| LX-T06 | Tìm "co gai" | Ra *das Mädchen* |
| LX-T07 | Tìm "madchn" (gõ sai) | Vẫn ra *Mädchen* (trigram) |
| LX-T08 | Tìm từ chỉ ở trạng thái Draft | Người học không thấy |
| LX-T09 | Tạo trùng `(de, Haus, Noun, 1)` | 409 `DUPLICATE_ENTRY` |
| LX-T10 | Thêm "See" (m) và "See" (f) | Hai mục, `homograph_no` lần lượt 1 và 2 |
| LX-T11 | CLI dry-run với wordlist 650 từ | Báo cáo đủ 4 nhóm số liệu, DB không thay đổi |
| LX-T12 | CLI chạy lại lần 2 (không dry-run) | Không tạo bản ghi trùng |
| LX-T13 | Chi tiết từ được import | Có dòng ghi công Wiktionary + CC BY-SA |
| LX-T14 | Động từ tách hiển thị | "ich rufe … an" |
| LX-T15 | `GermanTextNormalizer("STRASSE")` và `("Straße")` | Cùng ra `strasse` |

## 8. Các bước triển khai
1. **Khám phá dữ liệu:** tải dump kaikki, đọc khoảng 50 entry mẫu (danh từ, động từ tách, động từ bất quy tắc, từ đồng âm), ghi lại cấu trúc JSON thực tế vào `docs/phase-4/notes/wiktionary-structure.md`.
2. Entity + value object + `ValidateForPublish` + unit test (LX-T01..03, T15).
3. Migration (`pg_trgm` index), seed Topics.
4. API cho người học: tìm kiếm, chi tiết, ngữ pháp. Integration test LX-T04..10.
5. Admin CRUD + đăng ký với `IContentRegistry` + importer CSV/JSON.
6. CLI `import wiktionary` + `import tatoeba`.
7. UI: tra từ, chi tiết, ngữ pháp, `GenderBadge`, `ConjugationTable`, admin.
8. **Nhập nội dung thật:** lập `a1-lemmas.txt`, import, duyệt đợt đầu khoảng 100 từ. Đo thời gian duyệt thực tế, so với ước tính 25 giờ, rồi cập nhật kế hoạch.

## 9. Definition of Done
- [ ] LX-T01..15 pass.
- [ ] Có ≥ 100 từ A1 Published với đủ giống, số nhiều, bảng chia động từ, nghĩa tiếng Việt.
- [ ] Ghi chú cấu trúc Wiktionary đã được lưu lại.
- [ ] Ghi công hiển thị đúng trên mọi mục từ được import.

## 10. [ĐÃ CHỐT – CR-002, phương án 2] Thay đổi liên quan
> Đã chốt ngày 2026-11-22 (CR-002, phương án 2). Đây là kế hoạch chính thức.

- **Ngữ pháp công khai cho khách (A4):** thêm cột `is_public` cho `grammar_points`. Điểm ngữ pháp công khai được **render phía server** (SSR) tại `/ngu-phap/[slug]`, có metadata và JSON-LD để SEO, giống cách Brottin làm. Bố cục một bài nên theo mẫu đã chứng minh hiệu quả: giải thích ngắn → cấu trúc → ví dụ có dịch → bảng tổng hợp → **lỗi thường gặp (cặp sai/đúng + lý do)** → lưu ý sử dụng → bài liên quan → luyện tập ngay.
- **Bộ chơi thử công khai (A5):** thêm cột `is_public_practice` cho `vocabulary_entries` (khoảng 50 từ A1 chọn tay). Endpoint công khai chỉ đọc từ nhóm này.
- **Thẻ SRS (A3):** Lexicon cung cấp `ILexiconReader.GetEntriesAsync(ids)` cho Engagement. **Không** lưu gì về người học trong Lexicon.
- **Cụm từ mỗi ngày (B2, 1.1):** loại nội dung `DailyPhrase` đăng ký với `IContentRegistry`. Nằm trong Engagement (xem 08-engagement §3.5), không nằm ở đây.
- **Bộ từ theo nghề Ausbildung (B5, 1.1):** thêm bảng `topics` loại `Vocational` (điều dưỡng, nhà hàng–khách sạn, cơ điện tử). Chất lượng chuyên ngành khó duyệt khi làm một mình, nên cần quy trình riêng: chỉ nhập từ có nguồn tham chiếu, AI không tự đặt nghĩa chuyên ngành.
- Test bổ sung: **LX-T16** điểm ngữ pháp `is_public=false` không xem được khi chưa đăng nhập, **LX-T17** trang ngữ pháp công khai có `<title>`, meta description và JSON-LD.

## 11. [ĐÃ CHỐT – CR-003, phương án 2] Thẻ từ giàu thông tin

> Đã chốt ngày 2026-11-22. Nguồn: [CR-003 A2](../../change-requests/CR-003-personalization-from-brottin.md), [deep dive §3.1](../../research/02-brottin-deep-dive.md).

**Thêm vào `VocabularyEntry`:**
| Thuộc tính | Kiểu | Nội dung |
|---|---|---|
| `Synonyms` | jsonb (mảng `{lemma, noteVi?}`) | Từ đồng nghĩa |
| `Collocations` | jsonb (mảng `{textDe, textVi}`) | Cụm từ đi kèm (ví dụ "Blutdruck messen") |
| `WordFamily` | jsonb (mảng `{entryId?, lemma, pos, meaningVi}`) | Họ từ. Nếu từ liên quan đã có trong Lexicon thì lưu `entryId` để chạm vào mở thẻ đó |
| `UsageNotesVi` | text | Lưu ý cách dùng, lỗi hay gặp, sắc thái (Markdown ngắn) |

**Quy tắc (BR-LEX-05):** thẻ từ hiển thị **nguồn** (Source + License, đã có ở BR-CNT-02) và **nhãn "Có hỗ trợ AI"** khi `Origin` là `AIDraft` hoặc `AIDraftHumanEdited`. Bốn trường mới đều **không bắt buộc**: thiếu thì UI ẩn mục đó, không chặn publish. Chỉ giống, số nhiều (BR-LEX-01) và bảng chia động từ (BR-LEX-02) vẫn bắt buộc.

**Nguồn dữ liệu:** Wiktionary qua `wiktextract` có các trường đồng nghĩa, từ phái sinh, từ liên quan. 🟡 **Chưa kiểm tra trên dữ liệu thật.** Bước khám phá dữ liệu (§8 bước 1) phải trả lời: trường nào có, độ phủ bao nhiêu với danh sách A1. Nếu độ phủ thấp thì dùng AI viết nháp (qua pipeline ContentOps, có nhãn và duyệt). **Không** lấy từ nền tảng khác (CR-003 D1).

**Giao diện thẻ (dùng chung cho tra từ, bài học, phiên ôn):** mạo từ có màu + từ + IPA + audio, rồi nghĩa tiếng Việt, ví dụ có audio, 4 chip mở rộng (Đồng nghĩa, Cụm đi kèm, Họ từ, Cách dùng), mục Lưu ý chỉ hiện khi có. Cuối thẻ: dòng nguồn, nhãn AI, nút "Báo lỗi", nút "Lưu từ".

**Test bổ sung:** **LX-T18** thẻ có `Origin=AIDraftHumanEdited` hiển thị nhãn AI. **LX-T19** thẻ thiếu `Synonyms` vẫn publish được và UI ẩn chip. **LX-T20** chạm vào từ trong `WordFamily` có `entryId` mở đúng thẻ.
