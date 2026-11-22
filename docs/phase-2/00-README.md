# PHASE 2 – Business Analysis

**Trạng thái:** Đã xác nhận ngày 2026-11-22. **Cập nhật v2 theo CR-001** (chỉ tiếng Đức). Q-CR1-01..02 đã xác nhận. Phase 3 đã xong. **Bổ sung theo CR-002:** xem [06-cr002-addendum.md](06-cr002-addendum.md).
**Phạm vi:** MVP = **German A1 + Info Hub** (PD-08, theo [CR-001](../change-requests/CR-001-german-first.md)). Không có code.

| File | Nội dung |
|---|---|
| [01-actors-use-cases.md](01-actors-use-cases.md) | Actors, sơ đồ use case, danh sách 30 use case, đặc tả 7 use case trọng yếu |
| [02-requirements.md](02-requirements.md) | Functional Requirements theo module (MoSCoW) và 18 Non-functional Requirements có cách đo |
| [03-business-rules.md](03-business-rules.md) | Khoảng 70 business rule có ID: Identity, Privacy, Catalog, Progress, Assessment, **Exam (blueprint "Kiểm tra cuối A1")**, **Từ vựng tiếng Đức**, Content Ops, Info Hub, Báo lỗi |
| [04-user-stories.md](04-user-stories.md) | 6 Epic, khoảng 35 user story, mỗi story có Acceptance Criteria dạng Given/When/Then |
| [05-processes-diagrams.md](05-processes-diagrams.md) | 7 quy trình nghiệp vụ, 7 activity diagram, 4 sequence diagram (Mermaid) |

## Thuật ngữ (Glossary)

| Thuật ngữ | Nghĩa trong dự án |
|---|---|
| Program | Một chương trình học có mục tiêu riêng. MVP: "Tiếng Đức theo CEFR". V2: "Tiếng Đức cho Ausbildung" |
| Exam Blueprint | Cấu hình cấu trúc đề: phần, số câu, điểm, thời gian, dạng bài |
| Fixed / Generated exam | Đề có bộ câu cố định / đề rút ngẫu nhiên theo blueprint |
| Snapshot | Bản sao câu hỏi lưu cùng lượt thi, giúp kết quả không đổi khi câu hỏi được sửa |
| Source / License | Nguồn dữ liệu và giấy phép. Bắt buộc với mọi nội dung |
| Origin | Nguồn gốc tạo ra nội dung: Imported / HumanAuthored / AIDraft / AIDraftHumanEdited |
| Mức đánh giá | Ngưỡng % do nền tảng đặt để gợi ý "Sẵn sàng sang A2" / "Cần ôn thêm" (BR-EXAM-11) |
| NeedsUpdate | Nội dung đã publish nhưng đang bị nghi có lỗi hoặc đã cũ. Vẫn hiển thị kèm nhãn |

## Truy vết (Traceability)

Use case (UC-xx) → Functional Requirement (FR-xx) → Business Rule (BR-xx) → User Story / AC (US-xx) → *test case (Phase 5)*. Mỗi user story đều ghi các FR và BR liên quan trong ngoặc.

## Những điểm đã kế thừa từ Phase 1 có căn cứ
- Khung thời lượng phần thi viết Goethe A1 (Hören khoảng 20 phút, Lesen 25 phút) được dùng làm tham chiếu cho BR-EXAM-02. Đề do nền tảng tự soạn.
- Giấy phép CC BY-SA của Wiktionary được đưa vào BR-CNT-02 và BR-CNT-04. Dữ liệu hình thái của Wiktionary là cơ sở cho BR-LEX-01..02.
- Không dùng dữ liệu cấm thương mại (PD-05) được đưa vào BR-CNT-03.
- Làm một mình, không có người duyệt: BR-CNT-05 (cho phép publish thẳng nhưng bắt buộc qua checklist), BR-CNT-07 (AI kiểm định), BR-ASM-08 và BR-RPT-02 (phát hiện lỗi nhờ dữ liệu từ người học).
