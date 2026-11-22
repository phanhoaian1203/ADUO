# 03 – API Design (RESTful, `/api/v1`)

## 1. Quy ước chung (API Conventions)

### 1.1 URL & HTTP
- Tiền tố `/api/v1`. URL viết `kebab-case`, tài nguyên ở dạng **số nhiều**: `/api/v1/vocabulary-entries/{id}`.
- Ba nhóm quyền tách theo tiền tố:
  - `/api/v1/...`: công khai hoặc dành cho người học.
  - `/api/v1/me/...`: dữ liệu của chính người đang đăng nhập.
  - `/api/v1/admin/...`: quản trị.
- Phương thức HTTP:

| Phương thức | Dùng khi |
|---|---|
| GET | Đọc |
| POST | Tạo mới hoặc chạy hành động |
| PUT | Thay toàn bộ, hoặc upsert có định danh (ví dụ lưu câu trả lời) |
| PATCH | Sửa một phần |
| DELETE | Xóa |

- Hành động nghiệp vụ không phải CRUD được đặt thành **sub-resource động từ**: `POST /attempts/{id}/submit`, `POST /admin/questions/{id}/publish`.

### 1.2 JSON
- Khóa `camelCase`. Thời gian theo ISO 8601 UTC (`2026-11-22T08:30:00Z`). ID là chuỗi UUID.
- Enum là chuỗi (`"Published"`). Không trả `null` thừa (cấu hình `WhenWritingNull`).
- Nội dung rich text là Markdown, trường có hậu tố `Md`.

### 1.3 Phân trang, lọc, sắp xếp
- Request: `?page=1&pageSize=20` (tối đa 100), `?sort=-createdAt,title` (dấu `-` là giảm dần), lọc bằng tham số rõ tên, ví dụ `?status=Draft&cefrLevel=A1&q=haus`.
- Response:
```json
{ "items": [ ... ], "page": 1, "pageSize": 20, "totalCount": 642 }
```
- Mỗi endpoint chỉ cho phép sắp xếp theo danh sách trường được whitelist. Trường không hợp lệ trả 400 `INVALID_SORT`.

### 1.4 Lỗi: RFC 9457 ProblemDetails
```json
{
  "type": "https://<domain>/errors/section-closed",
  "title": "Phần thi đã đóng",
  "status": 409,
  "code": "SECTION_CLOSED",
  "detail": "Phần Hören đã hết giờ lúc 08:50:00Z.",
  "traceId": "00-4bf9…-01",
  "errors": { "email": ["Email không hợp lệ"] }
}
```
| HTTP | Khi nào | Ví dụ `code` |
|---|---|---|
| 400 | Request sai định dạng / validation | `VALIDATION_FAILED` (kèm `errors`), `INVALID_SORT` |
| 401 | Chưa đăng nhập | `UNAUTHENTICATED` |
| 403 | Không đủ quyền, chưa xác minh email | `FORBIDDEN`, `EMAIL_NOT_VERIFIED` |
| 404 | Không tìm thấy, hoặc nội dung chưa Published (với người học) | `NOT_FOUND` |
| 409 | Xung đột trạng thái | `SECTION_CLOSED`, `ATTEMPT_ALREADY_SUBMITTED`, `CONCURRENCY_CONFLICT`, `DUPLICATE_ENTRY` |
| 422 | Vi phạm business rule | `LICENSE_NOT_COMMERCIAL`, `PUBLISH_CHECK_FAILED`, `QUESTION_BANK_INSUFFICIENT`, `AI_BUDGET_EXCEEDED`, `AUDIO_PLAY_LIMIT` |
| 429 | Vượt rate limit | `RATE_LIMITED` (kèm header `Retry-After`) |
| 500 | Lỗi hệ thống | `INTERNAL_ERROR` (không lộ chi tiết) |

### 1.5 Bảo mật mỗi request
- Cookie xác thực (HttpOnly). Mọi request thay đổi dữ liệu phải gửi header **`X-XSRF-TOKEN`** (file 01 §4.2).
- Concurrency: các API sửa nội dung ở admin nhận `version` trong body. Nếu lệch thì trả 409 `CONCURRENCY_CONFLICT`.
- **OpenAPI** sinh tự động (`Microsoft.AspNetCore.OpenApi`) tại `/api/openapi/v1.json`, chỉ bật ở Development và Staging. Frontend sinh kiểu TypeScript từ file này.

### 1.6 Quy ước quyền trong bảng endpoint
`Public` = không cần đăng nhập · `Auth` = đã đăng nhập · `Verified` = đã xác minh email · `perm:x` = cần permission x (file 01 §4.3).

---

## 2. Identity – `/api/v1/auth`, `/api/v1/me`

| Method | Endpoint | Quyền | Mô tả | Rule |
|---|---|---|---|---|
| GET | `/auth/csrf` | Public | Cấp cookie XSRF-TOKEN | |
| POST | `/auth/register` | Public | Đăng ký | BR-AUTH-01..04 |
| POST | `/auth/login` | Public | Đăng nhập | BR-AUTH-06 |
| POST | `/auth/logout` | Auth | Đăng xuất | |
| GET | `/auth/external/google?returnUrl=` | Public | Chuyển hướng sang Google | BR-AUTH-09 |
| GET | `/auth/external/google/callback` | Public | Google gọi lại | |
| POST | `/auth/confirm-email` | Public | Xác minh email `{userId, token}` | FR-ID-02 |
| POST | `/auth/resend-confirmation` | Auth | Gửi lại email (3 lần/giờ) | |
| POST | `/auth/forgot-password` | Public | Luôn trả 202 (chống dò email) | US-04 |
| POST | `/auth/reset-password` | Public | `{email, token, newPassword}` | BR-AUTH-08 |
| GET | `/me` | Auth | Hồ sơ + quyền + trạng thái xác minh | |
| PATCH | `/me` | Auth | Sửa `displayName`, `learningPurpose`, `targetLevel`, `avatarMediaId` | FR-ID-06 |
| POST | `/me/change-password` | Auth | | FR-ID-07 |
| POST | `/me/delete` | Auth | `{password, confirmText:"XÓA"}` | BR-PRIV-02 |

**POST `/auth/register`**
```json
// Request
{ "displayName": "Lan", "email": "lan@example.com", "password": "Abc12345",
  "acceptTerms": true, "termsVersion": "2026-11", "privacyVersion": "2026-11" }
// 201 Created → giống GET /me
{ "id": "0193…", "displayName": "Lan", "email": "lan@example.com", "emailConfirmed": false,
  "learningPurpose": null, "targetLevel": null, "permissions": ["learning.access"] }
```
Validation:
- `displayName` dài 2–50 ký tự.
- `email` đúng định dạng và ≤ 254 ký tự.
- `password` đạt BR-AUTH-02.
- `acceptTerms` bắt buộc là `true`.

Lỗi: 400 `VALIDATION_FAILED`, 409 `EMAIL_TAKEN`.

---

## 3. Catalog & Learning

| Method | Endpoint | Quyền | Mô tả |
|---|---|---|---|
| GET | `/programs` | Public | Chương trình và các cấp độ (chỉ Published) |
| GET | `/courses?levelCode=A1` | Public | Danh sách khóa. Nếu đã đăng nhập thì kèm `progressPct` |
| GET | `/courses/{slug}` | Public | Chi tiết khóa: các unit, danh sách bài (tiêu đề, `isPreview`, thời lượng) |
| GET | `/lessons/{id}` | Public nếu `isPreview`, ngược lại Auth | Nội dung bài: các khối đã resolve (từ vựng, ngữ pháp, bộ bài tập **không kèm đáp án**) |
| PUT | `/me/lessons/{id}/progress` | Auth | `{lastBlockId}`. Lưu vị trí, tự enroll (BR-CAT-07) |
| POST | `/me/courses/{id}/enroll` | Auth | Tham gia khóa |
| GET | `/me/dashboard` | Auth | Bài "Tiếp tục học", tiến độ khóa, bài kiểm tra gợi ý, bài Info Hub theo mục đích học |

**GET `/lessons/{id}`** (response rút gọn)
```json
{ "id":"…","title":"Mạo từ sở hữu mein/dein","estimatedMinutes":12,
  "progress":{"status":"InProgress","lastBlockId":"…"},
  "blocks":[
    {"id":"…","type":"Markdown","markdown":"…"},
    {"id":"…","type":"VocabularyList","entries":[{"id":"…","lemma":"Mutter","gender":"f","plural":"Mütter","meaningVi":"mẹ","audioUrl":"https://media.<domain>/audio/…mp3","attribution":{"source":"Wiktionary","license":"CC BY-SA 4.0","url":"…"}}]},
    {"id":"…","type":"Exercise","questionSetId":"…","questionCount":8}
  ],
  "next":{"id":"…","title":"…"}, "previous":null }
```

## 4. Lexicon

| Method | Endpoint | Quyền | Mô tả |
|---|---|---|---|
| GET | `/vocabulary-entries?q=&cefrLevel=&topic=&pos=&page=` | Auth | Tìm từ. `q` khớp `lemma_search` và `meaning_vi_search` (BR-LEX-04) |
| GET | `/vocabulary-entries/{id}` | Auth | Chi tiết: giống, số nhiều, `forms` (bảng chia), ví dụ, audio, ghi công |
| GET | `/grammar-points?cefrLevel=A1` | Auth | Danh sách |
| GET | `/grammar-points/{slug}` | Auth | Chi tiết + ví dụ + `practiceSetId` |
| GET | `/topics` | Public | Chủ đề |

## 5. Assessment – luyện tập

| Method | Endpoint | Quyền | Mô tả |
|---|---|---|---|
| POST | `/me/practice-sessions` | Auth | `{questionSetId}`. Trả về session cùng các câu hỏi (**không kèm đáp án**) |
| POST | `/me/practice-sessions/{id}/answers` | Auth | `{questionId, response}`. Trả **ngay** `{isCorrect, fraction, correctAnswer, explanationMd}` (chế độ luyện tập) |
| POST | `/me/practice-sessions/{id}/complete` | Auth | Tổng kết. Cập nhật tiến độ bài (BR-PROG-01) |

`response` theo từng loại câu hỏi:
- SingleChoice: `{"optionId":"b"}`.
- FillBlank: `{"blanks":{"1":"komme"}}`.
- Matching: `{"pairs":[["l1","r2"]]}`.
- Ordering: `{"tokens":["ich","heiße","Lan"]}`.

## 6. Assessment – bài kiểm tra cuối cấp

| Method | Endpoint | Quyền | Mô tả | Rule |
|---|---|---|---|---|
| GET | `/exams?cefrLevel=A1` | Auth | Danh sách đề + số lần tôi đã làm + điểm cao nhất | |
| GET | `/exams/{slug}` | Auth | Giới thiệu: các phần, thời lượng, số câu, lượt nghe, câu miễn trừ | BR-EXAM-10 |
| POST | `/exams/{id}/attempts` | Verified + `assessment.take` | Bắt đầu, hoặc trả lại lượt đang làm dở | BR-EXAM-06, 07 |
| GET | `/me/attempts/{id}` | Auth (chủ sở hữu) | Trạng thái hiện tại: phần đang làm, câu hỏi (không kèm đáp án), đáp án đã lưu, `deadlineAt`, `serverNow` | |
| PUT | `/me/attempts/{id}/answers/{attemptQuestionId}` | Auth | Lưu hoặc ghi đè câu trả lời (idempotent) | BR-EXAM-04 |
| POST | `/me/attempts/{id}/audio-plays/{attemptQuestionId}` | Auth | Ghi nhận một lần phát. Trả `{playsUsed, playsLeft}`. Vượt giới hạn thì trả 422 `AUDIO_PLAY_LIMIT` | BR-EXAM-08 |
| POST | `/me/attempts/{id}/sections/{sectionNo}/close` | Auth | Chủ động chuyển phần (không quay lại được) | BR-EXAM-03 |
| POST | `/me/attempts/{id}/submit` | Auth | Nộp bài (idempotent: nộp lại thì trả kết quả cũ) | BR-EXAM-05 |
| GET | `/me/attempts/{id}/result` | Auth | Kết quả, kèm đáp án và giải thích (chỉ khi đã nộp) | BR-EXAM-11 |
| GET | `/me/attempts?page=` | Auth | Lịch sử | FR-ASM-09 |

**POST `/exams/{id}/attempts`** → `201` (mới) hoặc `200` (đang làm dở)
```json
{ "attemptId":"…","status":"InProgress","serverNow":"2026-11-22T08:30:00Z",
  "sections":[
    {"sectionNo":1,"skill":"Listening","titleVi":"Hören","deadlineAt":"2026-11-22T08:50:00Z","closed":false,"maxAudioPlays":2,
     "questions":[{"attemptQuestionId":"…","type":"SingleChoice","stemMd":"…","stemAudioUrl":"…","options":[{"id":"a","text":"…"}],"audioPlaysUsed":0,"savedResponse":null}]},
    {"sectionNo":2,"skill":"Reading","titleVi":"Lesen","deadlineAt":null,"closed":false,"locked":true}
  ] }
```
- Phần chưa tới lượt (`locked: true`) **không** trả câu hỏi về.
- `deadlineAt` của phần 2 chỉ được đặt khi phần 1 đóng.

**PUT `/me/attempts/{id}/answers/{aqId}`**
- Request: `{ "response": { "optionId": "c" } }`.
- Trả 204 nếu lưu được.
- Trả 409 `SECTION_CLOSED` nếu đã quá `deadlineAt + 30s` hoặc phần đã đóng.
- Trả 409 `ATTEMPT_ALREADY_SUBMITTED` nếu lượt đã nộp.

**GET `/me/attempts/{id}/result`**
```json
{ "score":21,"maxScore":30,"scorePct":70.0,"band":"ReadyForNext","bandLabelVi":"Sẵn sàng sang A2",
  "sections":[{"skill":"Listening","score":10,"max":15},{"skill":"Reading","score":11,"max":15}],
  "byCategory":[{"code":"ARTICLE_CHOICE","titleVi":"Chọn mạo từ","correct":4,"total":5}],
  "recommendations":[{"lessonId":"…","title":"…"}],
  "disclaimerVi":"Đây là bài kiểm tra do nền tảng biên soạn…",
  "questions":[{"sectionNo":1,"stemMd":"…","yourResponse":{…},"correctAnswer":{…},"isCorrect":false,"explanationMd":"…"}] }
```

## 7. Progress, Info Hub, Báo lỗi

| Method | Endpoint | Quyền | Mô tả |
|---|---|---|---|
| GET | `/me/progress` | Auth | % các khóa, số bài hoàn thành, tỷ lệ đúng theo kỹ năng (90 ngày), điểm kiểm tra gần nhất và cao nhất |
| GET | `/articles?category=` | Public | Danh sách bài (tiêu đề, tóm tắt, `lastReviewedAt`, `isOverdue`) |
| GET | `/articles/{slug}` | Public | Chi tiết + nguồn + phân loại + miễn trừ |
| GET | `/attributions` | Public | Danh sách Source + License đang được dùng (FR-CMS-10) |
| POST | `/me/error-reports` | Verified + `reports.create` | `{targetType, targetId, category, message}`. Lỗi: 409 `REPORT_ALREADY_OPEN`, 429 khi vượt 10 lần/ngày |
| GET | `/me/error-reports` | Auth | Các báo lỗi của tôi (S) |

---

## 8. Admin – `/api/v1/admin` (tất cả yêu cầu `perm:content.manage` trừ khi ghi khác)

| Nhóm | Endpoint chính | Ghi chú |
|---|---|---|
| Catalog | `GET/POST /admin/courses`, `GET/PATCH/DELETE /admin/courses/{id}`. Tương tự cho `/admin/units`, `/admin/lessons` | DELETE chỉ áp dụng khi chưa từng Published, nếu đã Published thì dùng `/archive` |
| Khối bài học | `PUT /admin/lessons/{id}/blocks` (thay toàn bộ danh sách khối, có thứ tự), `GET /admin/lessons/{id}/preview` | Kéo-thả (US-52) |
| Lexicon | `GET/POST /admin/vocabulary-entries`, `GET/PATCH /admin/vocabulary-entries/{id}`. Tương tự cho `/admin/example-sentences`, `/admin/grammar-points`, `/admin/topics` | Lọc theo `status`, `origin`, `sourceId`, `cefrLevel`, `q` |
| Questions | `GET/POST /admin/questions`, `GET/PATCH /admin/questions/{id}`, `POST /admin/questions/{id}/auto-check`. Tương tự cho `/admin/question-sets` | Auto-check theo BR-ASM-05 |
| Exams | `GET/POST /admin/blueprints`, `GET/POST /admin/exams`, `PUT /admin/exams/{id}/questions`, `POST /admin/exams/{id}/validate` | Lỗi 422 `QUESTION_BANK_INSUFFICIENT` (BR-EXAM-09) |
| Vòng đời (mọi loại nội dung) | `POST /admin/{resource}/{id}/submit-review`, `POST /admin/{resource}/{id}/publish` (`perm:content.publish`, body `{version, checklist:{…}}`), `POST …/return-to-draft` `{note}`, `POST …/archive` | BR-CNT-05, 06. Publish thiếu Source/License hoặc chưa qua kiểm tra thì trả 422 `PUBLISH_CHECK_FAILED` kèm danh sách lý do |
| Review queue | `GET /admin/review-queue?type=&origin=&flagged=&page=` | US-54 |
| Sources & Licenses | `GET/POST/PATCH /admin/sources`, `GET /admin/licenses` | BR-CNT-03 |
| Media | `POST /admin/media` (multipart), `GET /admin/media?kind=&q=` | Kiểm tra theo file 01 §5 |
| Import | `POST /admin/imports` (multipart: `contentType`, `sourceId`, `file`), trả về báo cáo dry-run. `POST /admin/imports/{id}/commit`. `POST /admin/imports/{id}/cancel`. `GET /admin/imports/{id}`. `GET /admin/imports/templates/{contentType}` (tải file CSV mẫu) | BR-CNT-10, 12. Lỗi 422 `LICENSE_NOT_COMMERCIAL` |
| AI & TTS | `GET /admin/prompt-templates`, `POST /admin/ai-drafts` `{templateId, params}` trả 202 `{jobId}`, `POST /admin/tts-jobs` `{targetType, ids[]}` trả 202, `GET /admin/jobs/{id}`, `GET /admin/usage?month=2026-11` | Job bất đồng bộ + polling (Phase 2 §10). Lỗi 422 `AI_BUDGET_EXCEEDED` |
| Info Hub | `GET/POST/PATCH /admin/articles`, `POST /admin/articles/{id}/mark-reviewed` | `perm:infohub.manage`, BR-INFO-01..04 |
| Báo lỗi | `GET /admin/error-reports?status=Open&targetType=`, `POST /admin/error-reports/{id}/resolve` `{note}`, `POST …/reject` `{note}` | `perm:reports.manage`. Resolve thì đóng luôn các báo lỗi cùng nội dung (US-57) |
| Users | `GET /admin/users?q=`, `POST /admin/users/{id}/lock`, `POST /admin/users/{id}/unlock` | `perm:users.manage` |
| Dashboard | `GET /admin/dashboard` | Cache 5 phút (FR-ADM-01) |
| Settings | `GET /admin/settings`, `PATCH /admin/settings` | Các giá trị *(cấu hình)* |

## 9. Template import CSV (ví dụ: từ vựng)
```
lemma,part_of_speech,gender,plural,noun_number,ipa,is_separable,is_irregular,cefr_level,meaning_vi,topics,original_ref
Mutter,Noun,f,Mütter,Normal,ˈmʊtɐ,false,false,A1,mẹ,gia-dinh,wiktionary:Mutter#1
anrufen,Verb,,,,ˈanˌʁuːfn̩,true,true,A1,gọi điện,giao-tiep,wiktionary:anrufen#1
```
- Bảng chia động từ không đi qua CSV (khó biểu diễn). Có hai cách: import JSON, hoặc để CLI lấy tự động từ dữ liệu Wiktionary.
- Template JSON được mô tả bằng JSON Schema, tải về tại `/admin/imports/templates/{contentType}?format=json`.
