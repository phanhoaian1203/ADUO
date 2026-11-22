# 05 – Business Processes, Activity & Sequence Diagrams

Sơ đồ viết bằng Mermaid, hiển thị được trên GitHub và VS Code (cần extension Markdown Preview Mermaid).

## 1. Bản đồ quy trình nghiệp vụ

| Quy trình | Ai | Tần suất | Sơ đồ |
|---|---|---|---|
| P1. Người học mới bắt đầu | Guest → Student | Mỗi người dùng mới | §2 |
| P2. Học bài và cập nhật tiến độ | Student | Hằng ngày | §3 |
| P3. Bài kiểm tra cuối cấp | Student, System | Hằng tuần | §4, §7, §8 |
| P4. Nhập dữ liệu từ nguồn mở | Admin | Theo đợt | §5, §9 |
| P5. Tạo nội dung bằng AI và duyệt | Admin | Hằng tuần | §6, §10 |
| P6. Rà soát Info Hub | Admin, System | 3–6 tháng/bài | §11 |
| P7. Xử lý báo lỗi | Student → Admin | Liên tục | §12 |

---

## 2. Activity – Người học mới (P1)

```mermaid
flowchart TD
  A([Vào landing page]) --> B{Đã có tài khoản?}
  B -- Chưa --> C[Xem catalog / học bài mẫu]
  C --> D[Bấm Đăng ký]
  D --> E[Nhập thông tin + tick đồng ý điều khoản]
  E --> F{Hợp lệ?}
  F -- Không --> E
  F -- Có --> G[Tạo tài khoản Unverified + gửi email xác minh]
  G --> H[Chọn mục đích học + cấp độ mục tiêu]
  B -- Có --> L[Đăng nhập] --> I
  H --> I[Dashboard theo mục tiêu]
  I --> J[Bắt đầu bài đầu tiên]
  G -.song song.-> K[Mở email & xác minh] --> M[Mở khóa thi thử, báo lỗi]
```

## 3. Activity – Học bài và cập nhật tiến độ (P2)

```mermaid
flowchart TD
  A([Mở bài học]) --> B{Guest & bài không phải preview?}
  B -- Có --> X[Yêu cầu đăng ký] --> Z([Kết thúc])
  B -- Không --> C[Tự enroll nếu chưa enroll]
  C --> D[LessonProgress = InProgress, lưu vị trí khối]
  D --> E[Học lần lượt các khối]
  E --> F{Bài có bài tập?}
  F -- Không --> G[Tới khối cuối] --> H[Completed]
  F -- Có --> I[Làm bài tập] --> J{Điểm >= 60%?}
  J -- Có --> H
  J -- Không --> K[Gợi ý ôn lại, giữ InProgress] --> I
  H --> L[Cập nhật % khóa / unit] --> M[Gợi ý bài tiếp theo] --> Z
```

## 4. Activity – Bài kiểm tra cuối A1 (P3)

```mermaid
flowchart TD
  A([Chọn đề]) --> B{Email đã xác minh?}
  B -- Chưa --> B1[Yêu cầu xác minh] --> Z([Kết thúc])
  B -- Rồi --> C{Có lượt đang làm dở?}
  C -- Có --> D[Tiếp tục lượt cũ]
  C -- Không --> E[Tạo ExamAttempt + snapshot câu hỏi + DeadlineAt phần Hören = now + 20']
  D --> F
  E --> F[Làm phần Hören: audio tối đa 2 lần, tự lưu]
  F --> G{Hết 20' hoặc bấm 'Sang phần Lesen'?}
  G -- Chưa --> F
  G -- Rồi --> H[Khóa phần Hören, DeadlineAt phần Lesen = now + 25']
  H --> I[Làm phần Lesen: tự lưu]
  I --> J{Bấm Nộp hoặc hết giờ?}
  J -- Chưa --> I
  J -- Bấm nộp --> K[Submitted]
  J -- Hết giờ --> K2[System AutoSubmitted]
  K --> L[Chấm điểm: 1 điểm/câu đúng, tính % và mức đánh giá]
  K2 --> L
  L --> M[Cập nhật thống kê câu hỏi & tiến độ] --> N[Trang kết quả + giải thích + miễn trừ] --> Z
```

## 5. Activity – Nhập dữ liệu (P4)

```mermaid
flowchart TD
  A([Tìm nguồn]) --> B[Xác minh nhà cung cấp & đọc giấy phép gốc]
  B --> C{License cho phép thương mại?}
  C -- Không / không rõ --> C1[Ghi NEEDS LEGAL VERIFICATION, dừng] --> Z([Kết thúc])
  C -- Có --> D[Tạo Source + License trong CMS]
  D --> E{Dữ liệu lớn?}
  E -- Có --> F[Tải file thô → CLI ETL: parse, chuẩn hóa]
  E -- Không --> G[Map sang template CSV/JSON → Upload trong CMS]
  F --> H[Dry-run: validate, dedup, kiểm license]
  G --> H
  H --> I{Admin chấp nhận báo cáo?}
  I -- Không --> J[Sửa mapping / file] --> H
  I -- Có --> K[Tạo ImportBatch, bản ghi Draft + provenance]
  K --> L[Xử lý trùng: bỏ qua / gộp / tạo mới]
  L --> M[Duyệt & Publish theo lô (P5)] --> Z
```

## 6. Activity – AI tạo bản nháp và duyệt (P5)

```mermaid
flowchart TD
  A([Chọn Prompt Template + tham số]) --> B{Còn trong trần chi phí tháng?}
  B -- Không --> B1[Chặn, báo đã dùng/trần] --> Z([Kết thúc])
  B -- Có --> C[Gọi LLM: sinh JSON theo schema]
  C --> D{JSON hợp lệ?}
  D -- Không --> D1[Ghi lỗi, cho thử lại] --> Z
  D -- Có --> E[Kiểm tra tự động BR-ASM-05]
  E --> F[Gọi LLM kiểm định: lỗi ngữ pháp, đáp án mơ hồ]
  F --> G[Lưu Draft, Origin=AIDraft, kèm cờ cảnh báo]
  G --> H[Admin mở hàng đợi duyệt]
  H --> I{Có cờ cảnh báo?}
  I -- Có --> J[Xem kỹ, sửa hoặc loại]
  I -- Không --> K[Xem nhanh theo checklist]
  J --> L{Đạt checklist?}
  K --> L
  L -- Không --> M[Trả về Draft + ghi chú / Archive]
  L -- Có --> N[Published, ghi ReviewedBy/At, Version++] --> Z
```

---

## 7. Sequence – Bắt đầu thi và lưu câu trả lời

```mermaid
sequenceDiagram
  actor S as Student
  participant FE as Next.js (browser)
  participant API as ASP.NET Core API
  participant DB as PostgreSQL

  S->>FE: Bấm "Bắt đầu"
  FE->>API: POST /exams/{examId}/attempts
  API->>DB: Tìm attempt InProgress của (user, exam)
  alt Đã có lượt dở
    DB-->>API: attempt
    API-->>FE: 200 attempt (resume)
  else Chưa có
    API->>DB: Đọc câu hỏi Published theo đề/blueprint
    API->>DB: INSERT ExamAttempt + AttemptQuestion snapshot, Section1(Hören).DeadlineAt = now+20'
    API-->>FE: 201 attempt {sections, deadlines (UTC), serverNow}
  end
  FE->>FE: Đồng hồ = deadline - serverNow (bù lệch giờ máy)
  loop Mỗi lần chọn đáp án
    S->>FE: Chọn đáp án
    FE->>API: PUT /attempts/{id}/answers/{questionId}
    API->>API: Kiểm tra now <= DeadlineAt + 30s và section chưa khóa
    alt Hợp lệ
      API->>DB: UPSERT AttemptAnswer
      API-->>FE: 204 (hiện "Đã lưu")
    else Quá hạn
      API-->>FE: 409 SECTION_CLOSED
    end
  end
```

## 8. Sequence – Nộp bài (thủ công và tự động)

```mermaid
sequenceDiagram
  actor S as Student
  participant FE as Browser
  participant API as API
  participant JOB as Background job (mỗi 1 phút)
  participant DB as PostgreSQL

  par Nộp thủ công
    S->>FE: Nộp bài + xác nhận
    FE->>API: POST /attempts/{id}/submit
    API->>DB: Lock attempt (status = InProgress?)
    API->>API: Chấm điểm từ snapshot
    API->>DB: UPDATE status=Submitted, score, section scores
    API->>DB: Cập nhật QuestionStats
    API-->>FE: 200 result
  and Tự động khi hết giờ
    JOB->>DB: SELECT attempts InProgress AND lastDeadline + 30s < now
    JOB->>API: GradeAndClose(attempt) (cùng service chấm điểm)
    API->>DB: UPDATE status=AutoSubmitted, score
  end
  Note over API,DB: Chấm điểm idempotent: attempt đã đóng thì trả kết quả cũ, không chấm lại
```

## 9. Sequence – Import CSV trong CMS

```mermaid
sequenceDiagram
  actor A as Admin
  participant FE as CMS UI
  participant API as API (ContentOps)
  participant ST as Object storage (R2)
  participant DB as PostgreSQL

  A->>FE: Chọn loại, Source, upload file
  FE->>API: POST /admin/imports (multipart, dryRun=true)
  API->>DB: Kiểm tra Source.License.AllowsCommercial
  alt Không cho phép thương mại
    API-->>FE: 422 LICENSE_NOT_ALLOWED
  else Hợp lệ
    API->>ST: Lưu file gốc (checksum)
    API->>API: Parse + validate + dedup
    API-->>FE: 200 báo cáo {valid, errors[], duplicates[]}
    A->>FE: Xác nhận import
    FE->>API: POST /admin/imports/{id}/commit
    API->>DB: INSERT ImportBatch + bản ghi Draft (transaction)
    API-->>FE: 200 {batchId, created}
  end
```

## 10. Sequence – AI tạo câu hỏi

```mermaid
sequenceDiagram
  actor A as Admin
  participant API as API (ContentOps)
  participant DB as PostgreSQL
  participant LLM as Claude API

  A->>API: POST /admin/ai-drafts {templateId, params}
  API->>DB: Kiểm tra AiUsage tháng này < trần
  API->>DB: Đọc PromptTemplate (version) + danh sách từ cho phép
  API->>LLM: Generate (structured output JSON)
  LLM-->>API: N câu hỏi
  API->>API: Validate schema + kiểm tra tự động
  API->>LLM: Review (prompt kiểm định)
  LLM-->>API: Kết quả kiểm định từng câu
  API->>DB: INSERT Questions (Draft, AIDraft, model, templateVersion, flags) + AiUsage
  API-->>A: Danh sách câu nháp + cờ
```
> Đồng bộ hay bất đồng bộ: tạo 20 câu có thể mất 20–60 giây. Ở Phase 3 cần chọn giữa (a) gọi đồng bộ có timeout dài, hoặc (b) job nền kèm polling. **Đề xuất (b)**, vì ổn định hơn với reverse proxy.

## 11. Activity – Rà soát Info Hub (P6)

```mermaid
flowchart TD
  A([Job hằng ngày]) --> B[Tìm bài Published có ReviewDueAt < hôm nay]
  B --> C[Chuyển NeedsUpdate + banner cảnh báo]
  C --> D[Dashboard: N bài quá hạn]
  D --> E[Admin mở nguồn chính thức, đối chiếu]
  E --> F{Nội dung còn đúng?}
  F -- Có --> G[Bấm 'Đã rà soát': LastReviewedAt = hôm nay, tính lại ReviewDueAt]
  F -- Không --> H[Sửa bài + cập nhật ngày truy cập nguồn] --> G
  G --> I[Published]
```

## 12. Activity – Báo lỗi (P7)

```mermaid
flowchart TD
  A([Student bấm Báo lỗi]) --> B{Đã xác minh email & dưới 10 lượt/ngày?}
  B -- Không --> B1[Từ chối, giải thích lý do]
  B -- Có --> C[Tạo ErrorReport Open]
  C --> D{≥ 3 người báo cùng nội dung?}
  D -- Có --> E[Nội dung → NeedsUpdate]
  D -- Không --> F[Vào hàng đợi]
  E --> F
  F --> G[Admin xem]
  G --> H{Lỗi thật?}
  H -- Có --> I[Sửa nội dung → Resolved, đóng các báo lỗi cùng nội dung → Published]
  H -- Không --> J[Rejected + ghi chú]
```
