# 04 – Folder Structure & Code Conventions

## 1. Repository (monorepo)
```
ADUO/
├── docs/                      # Tài liệu các phase, CR, decision log
├── backend/
│   ├── Aduo.sln
│   ├── src/
│   │   ├── Aduo.Api/          # Host web: Program.cs, middleware, cấu hình, OpenAPI
│   │   ├── Aduo.Core/         # Toàn bộ module: domain, feature, persistence
│   │   └── Aduo.Cli/          # Lệnh dòng lệnh: seed, create-admin, import ETL
│   └── tests/
│       ├── Aduo.Core.Tests/         # Unit test (business rules)
│       ├── Aduo.Api.IntegrationTests/  # Test qua HTTP + Postgres thật (Testcontainers)
│       └── Aduo.ArchitectureTests/  # Kiểm tra ranh giới module (NetArchTest)
├── frontend/                  # Next.js app (người học + /admin)
├── deploy/
│   ├── compose.dev.yml        # Postgres cho máy dev
│   ├── compose.yml            # Production / staging: caddy, web, api, postgres, backup
│   ├── Caddyfile
│   └── .env.example           # Mẫu biến môi trường (KHÔNG có giá trị thật)
├── .github/workflows/         # ci.yml, deploy.yml
├── .editorconfig
└── README.md
```
**Vì sao 3 project backend chứ không phải 1 hay 8?**
- Tách `Aduo.Core` khỏi `Aduo.Api` để `Aduo.Cli` dùng lại được toàn bộ domain, DbContext và validator mà không phải tham chiếu tới một web project.
- Không tách mỗi module thành một project, vì ranh giới đã được kiểm tra bằng architecture test, đủ cho một người (Phase 3 file 01 AR-03).

Tổng cộng có 3 project code và 3 project test.

## 2. Backend

### 2.1 `Aduo.Core`
```
Aduo.Core/
├── Common/                    # Dùng chung, KHÔNG chứa logic nghiệp vụ của module nào
│   ├── Results/               # Result<T>, Error, ErrorCodes
│   ├── Paging/                # PageRequest, PagedResult<T>, SortParser
│   ├── Content/               # ContentMeta (owned type), ContentStatus, Origin, IPublishable
│   ├── Text/                  # GermanTextNormalizer (ä→ae, ß→ss), VietnameseNormalizer (bỏ dấu)
│   ├── Security/              # Permissions (hằng số), RolePermissionMap
│   └── Time/                  # (dùng TimeProvider của .NET)
├── Infrastructure/
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── Migrations/
│   │   └── Interceptors/      # AuditInterceptor (created/updated_at), ContentVersionInterceptor
│   ├── Storage/               # IObjectStorage, R2ObjectStorage (AWSSDK.S3)
│   ├── Email/                 # IEmailSender, SmtpEmailSender (MailKit)
│   ├── Ai/                    # ILlmClient, AnthropicLlmClient
│   ├── Speech/                # ITtsClient, AzureTtsClient
│   └── Jobs/                  # JobRunner (BackgroundService), IJobHandler
└── Modules/
    ├── Identity/
    ├── Catalog/
    ├── Lexicon/
    ├── Assessment/
    ├── Progress/
    ├── InfoHub/
    └── ContentOps/
```

### 2.2 Cấu trúc một module (ví dụ `Assessment`)
```
Modules/Assessment/
├── AssessmentModule.cs        # AddAssessment(services) + MapAssessmentEndpoints(app)
├── Contracts/                 # Interface công khai cho module khác: IAssessmentReader
├── Domain/                    # Entity + logic thuần (không phụ thuộc EF/HTTP)
│   ├── Question.cs
│   ├── QuestionBodies/        # SingleChoiceBody, FillBlankBody, MatchingBody, OrderingBody
│   ├── Grading/               # IAnswerGrader + mỗi loại một grader (BR-ASM-01..04)
│   ├── ExamBlueprint.cs
│   ├── ExamAttempt.cs         # Start, SaveAnswer, CloseSection, Submit (BR-EXAM-03..07)
│   └── PassBand.cs
├── Persistence/               # IEntityTypeConfiguration<> cho các bảng schema "assessment"
├── Features/
│   ├── Attempts/
│   │   ├── StartAttempt/
│   │   │   ├── StartAttemptEndpoint.cs
│   │   │   ├── StartAttemptHandler.cs
│   │   │   └── StartAttemptResponse.cs
│   │   ├── SaveAnswer/        # Endpoint, Handler, Request, Validator
│   │   ├── RecordAudioPlay/
│   │   ├── CloseSection/
│   │   ├── SubmitAttempt/
│   │   └── GetAttemptResult/
│   ├── Practice/…
│   └── Admin/
│       ├── Questions/…        # CRUD, AutoCheck
│       └── Exams/…
└── Jobs/
    ├── AutoSubmitExpiredAttemptsJob.cs
    └── RecalculateQuestionFlagsJob.cs
```

**Luồng xử lý một request:**
1. `Endpoint` nhận HTTP và gọi `Validator`.
2. `Handler` được inject `AppDbContext`, `TimeProvider` và các service cần thiết.
3. `Handler` gọi **phương thức của domain entity**, nơi kiểm tra business rule. Ví dụ `attempt.SaveAnswer(aqId, response, now)` trả về `Result`.
4. `SaveChanges`, rồi trả DTO.

Logic nghiệp vụ nằm trong `Domain/` để unit test được mà không cần DB.

### 2.3 `Aduo.Api`
```
Aduo.Api/
├── Program.cs                 # Đăng ký module, auth, rate limit, ProblemDetails, OpenAPI, health
├── Middleware/                # CorrelationId, SecurityHeaders
├── Auth/                      # Cấu hình JWT bearer (đọc từ cookie hoặc header), Google, antiforgery, permission policies
├── appsettings.json           # Giá trị không bí mật
└── appsettings.Development.json
```

### 2.4 `Aduo.Cli`
Các lệnh dùng `System.CommandLine`:
- `seed`: dữ liệu tra cứu, idempotent.
- `create-admin --email`
- `import wiktionary --file de-extract.jsonl.gz --wordlist a1.txt --dry-run`: đọc file lớn, lọc theo danh sách lemma mục tiêu, map sang `vocabulary_entries` ở trạng thái Draft.
- `import tatoeba --sentences … --links … --cc0-only`
- `tts generate --missing --level A1`

### 2.5 Quy tắc đặt tên (C#)
| Loại | Quy tắc | Ví dụ |
|---|---|---|
| Namespace | Theo thư mục | `Aduo.Core.Modules.Assessment.Features.Attempts.SaveAnswer` |
| Class, method, property | PascalCase | `ExamAttempt.CloseSection()` |
| Biến cục bộ, tham số | camelCase | `attemptId` |
| Field private | `_camelCase` | `_db` |
| Interface | Tiền tố `I` | `IObjectStorage` |
| Async | Hậu tố `Async`, luôn nhận `CancellationToken ct` | `HandleAsync(req, ct)` |
| Slice | `{Verb}{Noun}` + `Endpoint` / `Handler` / `Request` / `Response` / `Validator` | `SubmitAttemptHandler` |
| Mã lỗi | `UPPER_SNAKE` trong `ErrorCodes` | `SECTION_CLOSED` |
| Test | `Method_Scenario_ExpectedResult` | `SaveAnswer_AfterDeadlinePlusGrace_ReturnsSectionClosed` |

**Quy tắc code:**
- Bật nullable reference types và `TreatWarningsAsErrors`.
- Không dùng `DateTime.Now`.
- Không có `async void`.
- Không viết SQL nối chuỗi.
- Một file một kiểu public.
- Chỉ tạo interface khi có từ 2 cài đặt trở lên, hoặc khi cần mock dịch vụ bên ngoài (ví dụ `ILlmClient`). Tránh abstraction thừa.

## 3. Frontend (Next.js App Router)
```
frontend/
├── src/
│   ├── app/
│   │   ├── (public)/                  # SSR/ISR, SEO
│   │   │   ├── page.tsx               # Landing
│   │   │   ├── khoa-hoc/page.tsx      # Catalog
│   │   │   ├── khoa-hoc/[slug]/page.tsx
│   │   │   ├── thong-tin/page.tsx     # Info Hub
│   │   │   ├── thong-tin/[slug]/page.tsx
│   │   │   ├── nguon-du-lieu/page.tsx # Attribution
│   │   │   └── dieu-khoan/, quyen-rieng-tu/
│   │   ├── (auth)/
│   │   │   ├── dang-nhap/, dang-ky/, quen-mat-khau/, dat-lai-mat-khau/, xac-minh-email/
│   │   ├── (learn)/                   # Cần đăng nhập, client-side
│   │   │   ├── layout.tsx             # Guard + bottom nav (mobile)
│   │   │   ├── bat-dau/               # Onboarding: mục đích + cấp độ mục tiêu
│   │   │   ├── hoc/                   # Dashboard
│   │   │   ├── bai-hoc/[id]/
│   │   │   ├── tu-vung/, tu-vung/[id]/
│   │   │   ├── ngu-phap/, ngu-phap/[slug]/
│   │   │   ├── luyen-tap/[setId]/
│   │   │   ├── kiem-tra/, kiem-tra/[slug]/, kiem-tra/lam-bai/[attemptId]/, kiem-tra/ket-qua/[attemptId]/
│   │   │   ├── tien-do/
│   │   │   └── ho-so/
│   │   └── admin/                     # Guard permission
│   │       ├── page.tsx               # Dashboard
│   │       ├── duyet/                 # Review queue
│   │       ├── khoa-hoc/, bai-hoc/[id]/, tu-vung/, ngu-phap/, cau-hoi/, de-kiem-tra/
│   │       ├── import/, ai/, media/, nguon/, thong-tin/, bao-loi/, nguoi-dung/, cai-dat/
│   ├── features/                      # Theo nghiệp vụ, mỗi feature một thư mục
│   │   ├── auth/       { api.ts, hooks.ts, schemas.ts, components/ }
│   │   ├── lesson/     { …, components/LessonBlockRenderer.tsx }
│   │   ├── vocabulary/ { …, components/GenderBadge.tsx, ConjugationTable.tsx }
│   │   ├── practice/   { …, components/questions/SingleChoice.tsx, FillBlank.tsx, Matching.tsx, Ordering.tsx }
│   │   ├── exam/       { …, store.ts (Zustand), offlineQueue.ts, components/ExamTimer.tsx }
│   │   ├── progress/, infohub/, reports/
│   │   └── admin/…
│   ├── components/
│   │   ├── ui/                        # shadcn/ui (Button, Dialog, Toast…)
│   │   └── common/                    # AudioPlayer, Markdown, EmptyState, ErrorState, PageSkeleton, Attribution
│   ├── lib/
│   │   ├── api/                       # client.ts (openapi-fetch + XSRF), schema.d.ts (sinh tự động)
│   │   ├── strings.ts                 # Toàn bộ chuỗi tiếng Việt
│   │   ├── format.ts                  # Ngày giờ (Asia/Ho_Chi_Minh), số
│   │   └── auth.ts                    # useMe(), hasPermission()
│   └── styles/globals.css
├── public/
├── tests/                             # Playwright e2e
└── package.json
```

**Quyết định về URL:** đường dẫn tiếng Việt không dấu (`/bai-hoc`, `/kiem-tra`) để gần gũi với người dùng và tốt cho SEO (Q6). Tên file và component trong code vẫn viết bằng tiếng Anh.

**Quy tắc đặt tên (TS):**
- Component viết PascalCase, mỗi component một file.
- Hook có tiền tố `use`.
- Schema Zod có hậu tố `Schema`.
- Query key theo dạng mảng `['lesson', id]`.
- Không import chéo trực tiếp giữa các `features/`. Phần dùng chung đưa vào `components/` hoặc `lib/`.

## 4. Git & quy trình
- Nhánh `main` luôn deploy được. Mỗi tính năng làm trên nhánh `feat/<module>-<mô-tả>`, merge qua Pull Request (dù chỉ có 1 người, để CI chạy).
- Commit theo **Conventional Commits**: `feat(assessment): auto-submit expired attempts`.
- `.gitignore` bao gồm `.env*` (trừ `.env.example`), `appsettings.*.local.json`, `node_modules`, `bin/obj`, file dữ liệu thô (`/data/raw`).
