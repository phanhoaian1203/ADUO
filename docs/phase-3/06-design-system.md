# 06 – Design System

Dựa trên **Tailwind CSS + shadcn/ui**. Token được khai báo bằng CSS variables (`globals.css`), để đổi theme mà không phải sửa component. MVP chỉ có giao diện sáng. Dark mode để V2, nhưng token đã chuẩn bị sẵn.

## 1. Màu sắc
| Token | Giá trị (gợi ý) | Dùng cho |
|---|---|---|
| `--primary` | `#1F4FD8` (xanh dương đậm) | Nút chính, link, thanh tiến độ |
| `--primary-foreground` | `#FFFFFF` | Chữ trên nền primary |
| `--accent` | `#F2B705` (vàng) | Điểm nhấn, chúc mừng, huy hiệu (gợi nhớ màu cờ Đức, dùng tiết chế) |
| `--background` / `--foreground` | `#FFFFFF` / `#111827` | Nền, chữ chính |
| `--muted` / `--muted-foreground` | `#F3F4F6` / `#6B7280` | Nền phụ, chữ phụ |
| `--border` | `#E5E7EB` | Viền |
| `--success` | `#15803D` | Đúng, hoàn thành |
| `--warning` | `#B45309` | Cảnh báo, NeedsUpdate, offline |
| `--danger` | `#B91C1C` | Sai, lỗi, xóa |
| **`--gender-m`** (der) | `#2563EB` xanh dương | Nhãn mạo từ **der** |
| **`--gender-f`** (die) | `#DC2626` đỏ | Nhãn mạo từ **die** |
| **`--gender-n`** (das) | `#16A34A` xanh lá | Nhãn mạo từ **das** |

- Cách tô màu theo giống (xanh dương / đỏ / xanh lá) là quy ước phổ biến trong sách học tiếng Đức.
- **Luôn đi kèm chữ** `der/die/das` trong nhãn (BR-LEX-03).
- Mọi cặp màu chữ / nền phải đạt tương phản ≥ 4.5:1. Cần kiểm tra lại bằng công cụ khi triển khai.

## 2. Typography
| Thành phần | Font | Lý do |
|---|---|---|
| Toàn bộ giao diện | **Be Vietnam Pro** (Google Fonts, SIL Open Font License) | Thiết kế cho tiếng Việt, dấu đẹp. Hỗ trợ ký tự Latin mở rộng (ä ö ü ß). Tải bằng `next/font`, tự host, subset `latin` + `vietnamese` |
| IPA, bảng chia động từ (tùy chọn) | `ui-monospace` của hệ thống | Căn cột đều |

| Cấp | Mobile | Desktop | Dùng cho |
|---|---|---|---|
| `display` | 28/36 semibold | 40/48 | Tiêu đề landing |
| `h1` | 24/32 semibold | 30/38 | Tiêu đề trang |
| `h2` | 20/28 semibold | 24/32 | Tiêu đề khối |
| `body` | **16/26** regular | 16/26 | Nội dung. **Không dùng cỡ nhỏ hơn 16 cho nội dung học** |
| `lemma` | 22/30 semibold | 24/32 | Từ tiếng Đức trong thẻ từ vựng |
| `small` | 14/20 | 14/20 | Chú thích, ghi công nguồn |

## 3. Spacing, bo góc, đổ bóng
- Thang khoảng cách mặc định của Tailwind (4px). Padding trang mobile `16px`, desktop `24–32px`. Chiều rộng nội dung học tối đa `720px` cho dễ đọc.
- Bo góc: `--radius: 12px` cho card, `10px` cho nút, `9999px` cho pill (nhãn giống, cấp độ).
- Đổ bóng: chỉ dùng `shadow-sm` cho card nổi và bottom sheet. Phần còn lại dùng viền.

## 4. Component (danh sách cho MVP)
| Nhóm | Component | Ghi chú |
|---|---|---|
| Cơ bản (shadcn) | Button (primary / secondary / ghost / destructive), Input, Textarea, Select, Checkbox, RadioGroup, Dialog, Sheet (bottom sheet), Toast, Tabs, Badge, Progress, Skeleton, Tooltip, DropdownMenu, Table | Nằm trong `components/ui` |
| Học tập | `LessonBlockRenderer`, `VocabularyCard`, `GenderBadge`, `ConjugationTable`, `GrammarCard`, `DialogueBlock`, `AudioPlayer` (tốc độ, đếm lượt nghe), `Attribution` | Trong `features/lesson`, `features/vocabulary` |
| Câu hỏi | `SingleChoiceQuestion`, `FillBlankQuestion` (có bàn phím phụ **ä ö ü ß** cho mobile), `MatchingQuestion` (chạm-chạm thay cho kéo-thả trên mobile), `OrderingQuestion` (chạm để chọn từ theo thứ tự), `AnswerFeedback` | Dùng chung cho luyện tập và kiểm tra (prop `mode`) |
| Kiểm tra | `ExamTimer`, `SaveIndicator`, `QuestionNavigator`, `SectionTransitionDialog`, `ResultSummary`, `BandBadge` | |
| Điều hướng | `BottomNav` (mobile), `SideNav` (desktop), `PageHeader` (nút quay lại + tiêu đề + ⚑) | |
| Trạng thái | `EmptyState`, `ErrorState`, `OfflineBanner`, `OverdueBanner` (Info Hub) | |
| Admin | `DataTable` (lọc, sắp xếp, chọn nhiều), `StatusBadge`, `ReviewChecklist`, `ImportReport`, `MarkdownEditor` (textarea + xem trước), `MediaPicker`, `BlockEditor` (kéo-thả bằng `dnd-kit`) | |

**Quyết định về FillBlank trên mobile:** bàn phím Android tiếng Việt thường không tiện gõ ä/ö/ü/ß. Vì vậy có thanh phím phụ ngay trên ô nhập, đồng thời chấp nhận cách gõ `ae/oe/ue/ss` nếu câu hỏi bật tùy chọn này (BR-ASM-02).

**Quyết định về Matching và Ordering:** kéo-thả trên điện thoại nhỏ dễ chạm nhầm, nên dùng thao tác **chạm để chọn**. Kéo-thả chỉ có trên desktop, và là tùy chọn.

## 5. Giọng văn (UX writing)
| Nên | Không nên |
|---|---|
| "Bạn đã hoàn thành 3/5 bài của Unit 3" | "Tiến độ: 60%" (khô cứng) |
| "Chưa đúng rồi. **der Tisch** – danh từ giống đực." | "Sai." |
| "Phần Hören đã hết giờ. Mình chuyển sang Lesen nhé." | "Error 409 SECTION_CLOSED" |
| "Thông tin này được rà soát lần cuối ngày 22/11/2026. Hãy kiểm tra nguồn chính thức trước khi nộp hồ sơ." | "Thông tin chính xác 100%" |

Định dạng: ngày `dd/mm/yyyy`, giờ 24h, múi giờ `Asia/Ho_Chi_Minh`, số thập phân dùng dấu phẩy (`70,5%`).
