# Module 6 – Progress (tiến độ học)

> Progress biến hành động của người học thành **cảm giác đang tiến bộ**: biết mình đã học tới đâu, mạnh và yếu ở kỹ năng nào, nên học gì tiếp theo. Với hành trình dài A1 → B1 (rủi ro R11), đây là module giữ chân người học.

## 1. Mục tiêu & phạm vi
| Trong phạm vi | Ngoài phạm vi |
|---|---|
| Enrollment (tự động và chủ động) | Streak, nhắc học (V2) |
| Trạng thái bài InProgress / Completed + vị trí khối cuối | Huy hiệu, điểm thưởng (V2+) |
| % hoàn thành của khóa và unit | Bảng xếp hạng |
| Nhật ký trả lời (`answer_events`) | Gợi ý cá nhân hóa bằng ML (V3) |
| Tỷ lệ đúng theo kỹ năng trong 90 ngày, điểm kiểm tra gần nhất và cao nhất | |
| Dashboard người học ("Tiếp tục học") + trang Tiến độ | |

Nguồn yêu cầu: UC-15 · FR-PROG-01..03, FR-CAT-06, 07 · BR-PROG-01..04, BR-CAT-07 · US-12 (AC3, AC4), US-40.

## 2. Kiến thức bạn sẽ học
| Khái niệm | Giải thích | Trong module |
|---|---|---|
| **Event log (append-only)** | Ghi lại *sự kiện đã xảy ra* (một lần trả lời) và không bao giờ sửa. Mọi thống kê đều được tính từ log này, nên có thể tính lại bất cứ lúc nào nếu công thức thay đổi | `answer_events` |
| **Read model / số liệu tổng hợp** | Bảng `lesson_progress` là "trạng thái hiện tại" để đọc nhanh. Log là "lịch sử đầy đủ" | Hai loại bảng |
| **Upsert** | `INSERT … ON CONFLICT DO UPDATE`: tạo mới hoặc cập nhật trong một câu lệnh, an toàn khi có nhiều request cùng lúc | `PUT /me/lessons/{id}/progress` |
| **Trạng thái một chiều** | Completed không quay lại InProgress (BR-PROG-02). Đây là bất biến đơn giản nhưng phải test | `LessonProgress.Complete()` |
| **Giao tiếp giữa module qua interface** | Assessment không ghi thẳng vào bảng của Progress, mà gọi `IProgressWriter` | AR-03 |
| **Aggregation query** | `GROUP BY skill` với điều kiện thời gian, có index phù hợp | Tỷ lệ đúng 90 ngày |
| **Ẩn danh dữ liệu** | Khi xóa tài khoản, thay `user_id` bằng một ID ẩn danh thay vì xóa sự kiện, để thống kê câu hỏi vẫn đúng | `IUserDataEraser` |

## 3. Mô hình miền
| Entity | Thuộc tính | Bất biến / quy tắc |
|---|---|---|
| `Enrollment` | `(UserId, CourseId)`, `EnrolledAt`, `LastActivityAt` | Rời khóa không xóa tiến độ (BR-CAT-07). MVP không có nút "rời khóa" |
| `LessonProgress` | `(UserId, LessonId)`, `Status`, `LastBlockId`, `ReachedEnd` (bool), `BestExercisePct`, `StartedAt`, `CompletedAt` | Completed khi `ReachedEnd` **và** (bài không có bài tập **hoặc** `BestExercisePct ≥ ngưỡng`). Ngưỡng mặc định 60%, lấy từ `lessons.pass_threshold_pct` hoặc settings (BR-PROG-01). Completed không quay lại (BR-PROG-02) |
| `PracticeSession` | Tóm tắt một lượt luyện tập | Do Assessment tạo, ghi qua writer |
| `AnswerEvent` | `UserId`, `QuestionId`, `Skill`, `CategoryId`, `Source` (Practice / Exam), `SourceId`, `Fraction`, `OccurredAt` | Chỉ ghi thêm. Không bao giờ sửa hay xóa (chỉ ẩn danh) |

*Ghi chú thiết kế:* cột `reached_end` được thêm vào `lesson_progress`, cần bổ sung vào Phase 3 – 02. Lý do: "đã tới khối cuối" và "điểm bài tập" là hai điều kiện độc lập, có thể xảy ra theo bất kỳ thứ tự nào.

## 4. Chức năng chi tiết

### 4.1 Ghi vị trí & enroll
`PUT /me/lessons/{id}/progress {lastBlockId}`:
1. Kiểm tra bài đã Published và `lastBlockId` thuộc bài đó.
2. Upsert `enrollments(user, course của bài)`, cập nhật `last_activity_at`.
3. Upsert `lesson_progress`: `status` = InProgress nếu chưa có, cập nhật `last_block_id`. Nếu khối đó là **khối cuối** của bài thì `reached_end = true`.
4. Gọi `TryComplete()`.

### 4.2 Nhận kết quả bài tập
`IProgressWriter.RecordPracticeCompleted(userId, lessonId?, setId, scorePct)` do Assessment gọi:
- Cập nhật `best_exercise_pct = max(cũ, mới)`.
- Gọi `TryComplete()`.

`TryComplete()` kiểm tra điều kiện của BR-PROG-01. Thỏa thì đặt `Completed` và `completed_at`, rồi trả về `justCompleted = true` để frontend hiện "Hoàn thành!".

### 4.3 Ghi sự kiện trả lời
`IProgressWriter.RecordAnswers(IEnumerable<AnswerEvent>)` do Assessment gọi (luyện tập: mỗi câu một lần; kiểm tra: cả lô khi nộp bài). Ghi theo lô.

### 4.4 Dashboard người học (`GET /me/dashboard`)
| Khối | Cách tính |
|---|---|
| Chào + mục tiêu | Từ Identity: `displayName`, `targetLevel`, `learningPurpose` |
| Thanh tiến độ cấp độ | % Completed trên tổng bài Published của các khóa thuộc cấp đang học |
| **Tiếp tục học** | `lesson_progress` InProgress mới nhất. Nếu không có thì lấy bài **chưa học đầu tiên** theo thứ tự trong khóa. Nếu chưa enroll khóa nào thì lấy bài 1 của khóa A1 |
| Unit hiện tại | Các bài của unit chứa bài "Tiếp tục học", kèm trạng thái từng bài |
| Bài kiểm tra | Đề A1 đầu tiên chưa làm. Hiện gợi ý "nên làm khi đã xong ≥ 80%" (chỉ gợi ý, không khóa) |
| Dành cho bạn | 1–2 bài Info Hub theo `learningPurpose` (qua `IInfoHubReader`) |

Dashboard gọi nhiều module, mỗi module **một query**. Không cache phía server vì đây là dữ liệu cá nhân. Frontend dùng `staleTime` 30 giây.

### 4.5 Trang Tiến độ (`GET /me/progress`, US-40)
- Mỗi khóa: % hoàn thành, số bài Completed / tổng.
- **Tỷ lệ đúng theo kỹ năng (90 ngày, BR-PROG-04):**
  ```sql
  SELECT skill, SUM(fraction) / COUNT(*) AS accuracy, COUNT(*) AS answers
  FROM progress.answer_events
  WHERE user_id = :u AND occurred_at >= now() - interval '90 days'
  GROUP BY skill;
  ```
  Kỹ năng có dưới 10 lượt trả lời thì hiện "Chưa đủ dữ liệu" để tránh kết luận sai.
- Bài kiểm tra: điểm gần nhất, điểm cao nhất, số lượt (qua `IAssessmentReader`).
- Người học mới thì hiện trạng thái rỗng kèm nút "Bắt đầu bài đầu tiên".

### 4.6 % khóa (BR-PROG-03)
`% = Completed (của các bài đang Published) / số bài đang Published`. Bài bị Archive không còn được tính, kể cả khi người học đã hoàn thành nó.

### 4.7 Xóa tài khoản
Cài đặt `IUserDataEraser`:
- `answer_events.user_id` → `00000000-0000-7000-8000-000000000000` (UUID ẩn danh cố định).
- Xóa `enrollments`, `lesson_progress`, `practice_sessions` của người dùng đó (không cần cho thống kê).

## 5. Dữ liệu & API
- Bảng: [Phase 3 – 02 §3.5](../../phase-3/02-database-design.md), thêm cột `reached_end`.
- API: `PUT /me/lessons/{id}/progress`, `POST /me/courses/{id}/enroll`, `GET /me/dashboard`, `GET /me/progress`.
- Interface: `IProgressWriter` (cho Assessment), `IProgressReader` (cho Catalog: % khóa, trạng thái bài).

## 6. Giao diện
Dashboard theo wireframe [Phase 3 – 05 §3.3](../../phase-3/05-ux-design.md). Trang Tiến độ gồm:
- Thẻ % theo khóa.
- 2 thanh tỷ lệ đúng (Hören / Lesen), cùng Grammar và Vocabulary nếu có.
- Danh sách điểm kiểm tra.
- Câu chữ khích lệ, ví dụ "Bạn đã học 12 bài · Tuần này 3 bài".

Màn hình chúc mừng khi hoàn thành bài, và lớn hơn khi hoàn thành unit.

## 7. Test cases
| ID | Kịch bản | Kết quả mong đợi |
|---|---|---|
| PR-T01 | Mở bài lần đầu | Có enrollment, lesson_progress InProgress |
| PR-T02 | Tới khối cuối, bài không có bài tập | Completed |
| PR-T03 | Tới khối cuối, bài tập 50% | InProgress |
| PR-T04 | Sau PR-T03, làm lại được 80% | Completed, `justCompleted=true` |
| PR-T05 | Làm bài tập 80% **trước**, tới khối cuối **sau** | Completed (thứ tự không quan trọng) |
| PR-T06 | Đã Completed, làm lại được 20% | Vẫn Completed, best vẫn 80% |
| PR-T07 | Gửi `lastBlockId` không thuộc bài | 400 |
| PR-T08 | Hai request progress đồng thời | Không lỗi trùng khóa (upsert) |
| PR-T09 | Khóa có 10 bài, xong 5, Archive 1 bài đã xong | 4/9 = 44% |
| PR-T10 | Tỷ lệ đúng 90 ngày: có event 100 ngày trước | Event đó không được tính |
| PR-T11 | Skill có 5 lượt trả lời | "Chưa đủ dữ liệu" |
| PR-T12 | Dashboard người học mới | "Tiếp tục học" trỏ tới bài 1 A1 |
| PR-T13 | Xóa tài khoản rồi chạy job | `answer_events` còn nguyên số dòng nhưng `user_id` ẩn danh, progress đã bị xóa |

## 8. Các bước triển khai
1. Entity + migration (thêm `reached_end`).
2. Domain `LessonProgress.TryComplete` + unit test PR-T02..06.
3. `IProgressWriter` / `IProgressReader` + gắn vào Assessment và Catalog.
4. API progress, dashboard, tiến độ + integration test.
5. `IUserDataEraser` cho Progress.
6. UI dashboard, trang Tiến độ, màn hình chúc mừng.

## 9. Definition of Done
- [ ] PR-T01..13 pass.
- [ ] Học thử một unit hoàn chỉnh: % và "Tiếp tục học" đều đúng sau mỗi bài.

## 10. [ĐÃ CHỐT – CR-002, phương án 2] Thay đổi liên quan
> Đã chốt ngày 2026-11-22 (CR-002, phương án 2). Đây là kế hoạch chính thức.

- **Dashboard được thay bằng "Bàn học" (A1):** `GET /me/dashboard` vẫn có, nhưng giờ gom thêm dữ liệu từ Engagement (streak, nhiệm vụ, số thẻ cần ôn, cấp độ). Wireframe mới ở [08-engagement §5](08-engagement.md). Các khối cũ (Tiếp tục học, Unit hiện tại, Bài kiểm tra gợi ý, Dành cho bạn) giữ nguyên.
- **Streak không còn nằm ở Progress**, mà do Engagement quản lý. Progress chỉ cung cấp sự kiện hoàn thành bài và bài tập.
- Progress gọi `IEngagementWriter.OnLessonCompleted` và `OnExerciseDone` để Engagement cập nhật nhiệm vụ (tách thành interface để Progress không phụ thuộc cài đặt).
- Test bổ sung: **PR-T14** hoàn thành bài thì nhiệm vụ "hoàn thành 1 bài" tăng tiến độ.
