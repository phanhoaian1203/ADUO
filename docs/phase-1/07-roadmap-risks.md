# 07 – Initial Roadmap & Risk Assessment

## 1. Roadmap

**Đã xác nhận:** thời gian không cố định, hiện đang rảnh nhiều. Vì vậy roadmap đi **theo mốc hoàn thành (milestone)**. Cột thời lượng chỉ để tham khảo, tính theo khoảng 15–20 giờ/tuần. Nếu làm gần full-time thì chia cho khoảng 2,5.

**Khuyến nghị:** tận dụng giai đoạn rảnh hiện tại để làm liền mạch Phase 2 và Phase 3. Đây là những phần cần tập trung liên tục, khó làm ngắt quãng. Phần nhập nội dung (Phase 4) dễ chia nhỏ để làm lúc rảnh rỗi rải rác.

| Giai đoạn | Nội dung chính | Đầu ra | Thời lượng ước tính |
|---|---|---|---|
| **1. Product & Resource Discovery** | Tài liệu này. Xác minh nốt các mục ⚖️ | 9 tài liệu Phase 1, decision log | 1–2 tuần (đang ở đây) |
| **2. Business Analysis** | Use case, user story + acceptance criteria, business rules (chấm điểm, blueprint, workflow duyệt), activity diagram, sequence diagram | SRS gọn | 2–3 tuần |
| **3. Architecture & UI/UX** | ERD, API contract (OpenAPI), cấu trúc thư mục, user flow, wireframe, design system | Tài liệu thiết kế + prototype Figma (hoặc wireframe) | 3–4 tuần |
| **4. MVP Development** | Theo từng module, **lần lượt**: Identity → Catalog/Lesson → Lexicon + Import → Assessment (Exercise) → Exam + Result → Progress → CMS/Review → Info Hub. **Song song:** nhập nội dung | Ứng dụng chạy ở staging | 12–16 tuần |
| **5. Testing & Deployment** | Test tích hợp, test bảo mật cơ bản, test hiệu năng nhẹ, deploy production, backup | Production + runbook | 2–3 tuần |
| **6. Beta Launch & Optimization** | 20–50 người học thật (qua trường THPT, trung tâm ở Quảng Trị). Thu phản hồi, sửa lỗi, đo retention | Báo cáo beta, backlog V2 | 4–6 tuần |
| **Tổng tới beta** | | | **~6–8 tháng** |

**Mốc kiểm soát phạm vi (CR-001):** sau khi xong module Exam ở giai đoạn 4, nếu đã trễ hơn 30% kế hoạch thì ra beta với **phần A1 đã hoàn thành** (ví dụ 2/3 số unit) cộng Info Hub, rồi bổ sung các unit còn lại sau beta. Không cắt chất lượng duyệt.

## 2. Risk Assessment

| # | Rủi ro | Khả năng | Ảnh hưởng | Giảm thiểu |
|---|---|---|---|---|
| R1 | **Khối lượng nội dung vượt sức** (khoảng 215–230 giờ cho nội dung A1) | Cao | Cao | **CR-001: chỉ tiếng Đức, MVP chỉ A1.** AI viết nháp. Có mốc kiểm soát phạm vi |
| R2 | **Chất lượng nội dung** (sai ngữ pháp, sai đáp án). Đã xác nhận không có người duyệt chuyên môn | **Rất cao** | Cao | Quy trình duyệt cho người làm một mình (file 05 §6): dữ liệu có thẩm quyền, AI kiểm tra chéo, kiểm tra tự động, nhãn Beta, nút "Báo lỗi", thống kê câu bất thường |
| R3 | **Vi phạm bản quyền** do nhập nhầm nguồn | Trung bình | Cao | Bắt buộc có provenance và license ở DB. Danh sách cấm (file 04 §4). Trang Attribution |
| R4 | **Thông tin pháp lý lỗi thời** (điều kiện visa Ausbildung, mức chứng minh tài chính, thủ tục công nhận) | Cao | Cao (gây hại cho người học) | Luôn liên kết nguồn chính thức. Có `ReviewDueAt`. Banner "Cập nhật ngày…". Không tự viết con số |
| R5 | Khung bài thi chứng chỉ thay đổi (Goethe, telc ra phiên bản mới) | Trung bình | Trung bình | Blueprint là dữ liệu cấu hình |
| R6 | Overengineering, sa vào kỹ thuật mà không ra sản phẩm | Trung bình | Cao | Modular monolith. Mỗi module đi đủ vòng (phân tích → test) rồi mới làm module tiếp |
| R7 | Bỏ dở vì kiệt sức (solo) | Trung bình | Cao | Mục tiêu nhỏ từng tuần. Có mốc beta cố định |
| R8 | Chi phí vận hành tăng (audio, AI) | Thấp | Trung bình | Audio tạo một lần và lưu trên R2. AI chỉ dùng trong CMS. Theo dõi chi tiêu |
| R9 | Bảo mật và dữ liệu cá nhân (NĐ 13/2023) | Trung bình | Cao | Identity chuẩn. Rate limiting. Thu thập tối thiểu. Có chính sách quyền riêng tư |
| R10 | Không có người dùng (cạnh tranh với app miễn phí) | Trung bình | Trung bình | Beta có chủ đích qua trường và trung tâm địa phương. Tập trung lộ trình và thông tin chính thống |
| R11 | **(CR-001) Người học bỏ cuộc giữa hành trình dài A1 → B1** | Cao | Cao | Hiển thị lộ trình và mốc rõ ràng. Bài ngắn 10–15 phút. Ra A2 sớm (Release 1.1). Streak và nhắc học ở V2 |

## 3. Đánh giá khả thi cho Solo Developer

- **Phần mềm:** **khả thi**. LMS kèm quiz và CMS là phạm vi một người làm được trong 3–4 tháng part-time, nếu giữ MVP gọn.
- **Nội dung:** **khả thi có điều kiện**. Cần (a) tận dụng tốt Wiktionary và Tatoeba, (b) AI viết nháp và AI kiểm định, (c) quy trình duyệt cho người làm một mình (file 05 §6). Chỉ một ngôn ngữ nên khả thi hơn đáng kể.
- **Toàn bộ lộ trình tiếng Đức** (A1 → B2, kèm tiếng Đức cho Ausbildung): khoảng 2 năm. Năm đầu nên đạt A1, A2 và bắt đầu B1.
