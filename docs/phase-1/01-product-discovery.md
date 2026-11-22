# 01 – Product Discovery

> **Phiên bản 2: cập nhật theo [CR-001](../change-requests/CR-001-german-first.md) ngày 2026-11-22.** Dự án tập trung 100% vào tiếng Đức.

## 1. Project Overview

**Tên tạm:** ADUO (đặt theo thư mục dự án, có thể đổi sau).

**Mô tả:** Nền tảng web học **tiếng Đức** bằng tiếng Việt, theo lộ trình **A1 → A2 → B1** gắn với mục tiêu **Ausbildung (du học nghề)** và làm việc tại Đức. Người dùng chính là học sinh vừa tốt nghiệp THPT ở các tỉnh như Quảng Trị.

**Người phát triển:** 1 người (solo), làm toàn bộ vòng đời sản phẩm.

### Đánh giá ý tưởng (phản biện)

**Điểm mạnh:**
- Nhu cầu có thật. Ausbildung, trong đó có điều dưỡng, nhà hàng và khách sạn, cơ khí, là con đường được nhiều bạn trẻ miền Trung quan tâm. Visa Ausbildung thường yêu cầu tiếng Đức **B1** (theo [make-it-in-germany](https://www.make-it-in-germany.com/de/visum-aufenthalt/arten/berufsausbildung), 🟡), tức là mục tiêu học tập đo được rõ ràng.
- Tiếng Đức có nguồn dữ liệu mở tốt cho phần khó nhất: Wiktionary có giống danh từ, số nhiều, chia động từ (CC BY-SA). Tatoeba có câu ví dụ.
- Tập trung một ngôn ngữ giúp bạn, khi làm một mình, giữ được chất lượng và tự học sâu ngôn ngữ đó.

**Điểm yếu và rủi ro:**
1. **Hành trình dài.** Từ 0 lên B1 mất nhiều tháng. Nếu chỉ có A1, sản phẩm chưa đưa người học tới mục tiêu cuối, nên phải ra A2 và B1 theo lộ trình.
2. **Nội dung vẫn là nút thắt.** Không có ngân hàng câu hỏi tiếng Đức nào có giấy phép mở cho người Việt. Câu hỏi và bài giảng phải tự xây dựng, có AI hỗ trợ.
3. **Cạnh tranh** với các trung tâm và kênh online. Lợi thế của dự án phải đến từ: tiếng Việt dễ hiểu, lộ trình rõ ràng, theo dõi tiến độ, thông tin chính thống có ngày cập nhật, và **không môi giới**.
4. **Thông tin pháp lý thay đổi** (điều kiện visa, mức chứng minh tài chính). Sai thông tin sẽ gây hại thật cho người học.
5. **Kỹ năng Nói và Viết** cần cho B1 rất khó chấm tự động.

**Kết luận:** khả thi và tập trung hơn bản đầu. **MVP = German A1 đầy đủ + Info Hub (Ausbildung, Recognition)**. A2 ra ngay sau đó.

## 2. Product Vision

> Giúp người trẻ Việt Nam học tiếng Đức từ con số 0 đến trình độ cần cho Ausbildung và làm việc tại Đức, với lộ trình rõ ràng bằng tiếng Việt, nội dung hợp pháp và thông tin chính thống có ghi ngày cập nhật.

| Mục tiêu (12 tháng đầu) | Chỉ số đo |
|---|---|
| G1 – Ra mắt MVP dùng được thật | Lộ trình A1 hoàn chỉnh: tất cả bài học, bài tập và ít nhất 2 bài kiểm tra cuối A1 |
| G2 – Mở rộng lộ trình | A2 ra mắt (Release 1.1) |
| G3 – Có người dùng thật và giữ chân được | 100–300 người học đăng ký trong beta (giả định). Tỷ lệ quay lại sau 7 ngày ≥ 20% (tham khảo) |
| G4 – Portfolio | Có tài liệu kỹ thuật, CI/CD, test, chạy production |

## 3. Product Scope

**Trong phạm vi (toàn dự án, theo lộ trình):**
- Tiếng Đức A1 → B1, sau đó B2.
- Tiếng Đức cho Ausbildung (phỏng vấn, nơi làm việc, từ vựng nghề).
- Info Hub: Ausbildung, công nhận văn bằng (Recognition), lộ trình học và thi chứng chỉ.

**Ngoài phạm vi:**
- Môi giới, tuyển dụng, tư vấn visa cá nhân, cam kết đỗ.
- Lớp học trực tiếp, app native, phát hành chứng chỉ.
- Luyện đề "Goethe" hay "telc" mang tên chính thức. Nền tảng chỉ có bài kiểm tra do nền tảng tự thiết kế theo khung CEFR.

## 4. Target User Analysis

| Persona | Mục tiêu | Vấn đề chính | Ưu tiên |
|---|---|---|---|
| **P1 – Lan, 18 tuổi, muốn đi Ausbildung** (persona chính) | Học từ 0, đạt A1 rồi A2 rồi B1 để nộp hồ sơ Ausbildung | Không biết bắt đầu từ đâu. Tài liệu rời rạc. Sợ ngữ pháp (giống danh từ, chia động từ). Không rõ cần trình độ nào, thủ tục ra sao. Dễ bị "vẽ" lộ trình đắt đỏ | **MVP** |
| P2 – Hoa, 20 tuổi, muốn đi Ausbildung **điều dưỡng (Pflege)** | Như P1, thêm từ vựng chuyên ngành điều dưỡng | Cần hiểu khác biệt giữa học nghề điều dưỡng và công nhận bằng điều dưỡng (nghề được quản lý) | MVP (lộ trình chung + Info Hub). Từ vựng nghề ở V2 |
| P3 – Người đã có bằng hoặc nghề ở Việt Nam, muốn được công nhận tại Đức | B1–B2, hiểu thủ tục Recognition | Nhầm lẫn giữa công nhận học thuật, công nhận nghề được quản lý, và công nhận nghề theo BQFG | Info Hub ở MVP. Tiếng Đức B1/B2 ở V2/V3 |
| P4 – Người học tiếng Đức chung (sinh viên, người đi làm) | Học theo cấp độ | Cần lộ trình và luyện tập có hệ thống | MVP (cùng lộ trình) |
| P5 – Admin / Content Manager (chính bạn) | Nhập và duyệt nội dung nhanh, chính xác | Làm một mình, cần import hàng loạt, AI hỗ trợ, có checklist duyệt | **MVP** |

**Đặc điểm chung ảnh hưởng đến thiết kế:**
- Chủ yếu dùng **điện thoại Android tầm trung**, mạng 4G. Vì vậy cần mobile-first, nhẹ, audio nén.
- Giao diện **chỉ tiếng Việt**, văn phong gần gũi. Giải thích ngữ pháp bằng ví dụ, so sánh với tiếng Việt khi phù hợp.
- Cần **lộ trình từng bước** và mục tiêu nhỏ, có thể đạt được, để không bỏ cuộc giữa hành trình dài.
- Nhạy cảm với chi phí. MVP **miễn phí**.

## 5. Product Value Proposition

| Cho ai | Giá trị | Khác gì so với tự học rời rạc |
|---|---|---|
| P1, P2, P4 | Lộ trình A1 → B1 bằng tiếng Việt. Mỗi bài ngắn, học được trên điện thoại. Từ vựng có **màu theo giống** (der/die/das), số nhiều, audio. Luyện chia động từ. Bài kiểm tra cuối cấp có tính giờ và lưu kết quả | Có hệ thống, có tiến độ, có kiểm tra định kỳ, giải thích bằng tiếng Việt |
| P1, P2, P3 | Info Hub: Ausbildung, Recognition. Thông tin **có nguồn chính thức và ngày cập nhật**, phân biệt "chính thức" với "tham khảo" | Không tin đồn, không quảng cáo môi giới, không cam kết |

## 6. Proposed Modules

Thiết kế theo **Modular Monolith**. Các module dùng chung tài khoản và hạ tầng. Mô hình vẫn **đa ngôn ngữ về mặt dữ liệu** (có bảng Language), nhưng chỉ triển khai tiếng Đức.

```
┌─────────────────────── Platform (dùng chung) ───────────────────────┐
│ Identity & Access │ User Profile │ Media/Storage │ Notifications(V2) │
└──────────────────────────────────────────────────────────────────────┘
┌──────── Learning Core ─────────────────────┐  ┌──── Assessment ────┐
│ Catalog: Language→Program→Level→Course→    │  │ Question Bank      │
│          Unit→Lesson                        │  │ Exercise/Quiz      │
│ Lexicon: Vocabulary (giống, số nhiều,       │  │ Level Test (bài    │
│          chia động từ), Example sentences   │  │ kiểm tra cuối cấp) │
│ Grammar points                              │  │ Attempt & Scoring  │
│ Progress tracking                           │  └────────────────────┘
└─────────────────────────────────────────────┘
┌──── Program: German CEFR ─────────────────────┐  ┌── Content Ops ──┐
│ Cấp A1 (MVP), A2 (1.1), B1 (V2), B2 (V3)      │  │ CMS, Import,    │
│ Tiếng Đức cho Ausbildung (V2)                 │  │ Provenance,     │
│ Info Hub: Ausbildung, Recognition, Lộ trình   │  │ License, Review │
└───────────────────────────────────────────────┘  └─────────────────┘
```

**Đặc thù tiếng Đức cần hỗ trợ ngay từ đầu (không ép theo khuôn chung):**
- Mục từ danh từ có **giống** và **số nhiều**. Động từ có **bảng chia** (Präsens ở A1, Perfekt ở A2). Có thông tin động từ tách (trennbar).
- Dạng câu hỏi đặc thù **dùng lại các loại câu hỏi chung**, phân biệt bằng *dạng bài*:
  - Chọn mạo từ der/die/das: SingleChoice.
  - Chia động từ: FillBlank.
  - Sắp xếp trật tự câu (động từ ở vị trí 2): Ordering.
- **Bài kiểm tra cuối cấp** cấu hình bằng blueprint. Khung tham chiếu là thời lượng phần thi viết của Goethe A1 (Hören khoảng 20 phút, Lesen 25 phút, Schreiben 20 phút, theo [file 02 §1.10](02-resource-discovery.md)). Nhưng đề **do nền tảng tự thiết kế**, MVP chấm tự động Hören và Lesen.

## 7. MVP Feature List

| # | Chức năng | Ghi chú |
|---|---|---|
| 1 | Đăng ký, đăng nhập, quên mật khẩu (email) + đăng nhập Google | Q-P2-01 |
| 2 | Hồ sơ + **mục đích học** (Ausbildung / Điều dưỡng / Công nhận nghề / Du học / Khác) + **cấp độ mục tiêu** (A1, A2, B1) | Dùng để gợi ý và hiển thị Info Hub phù hợp |
| 3 | Catalog: Language → Program → Level → Course → Unit → Lesson | |
| 4 | Bài học: nội dung, từ vựng, ngữ pháp, hội thoại, audio | |
| 5 | Tra từ vựng: giống, số nhiều, chia động từ, IPA, audio, ví dụ | Dữ liệu import từ Wiktionary, có ghi công |
| 6 | Điểm ngữ pháp, giải thích tiếng Việt | Tự soạn, AI hỗ trợ, có duyệt |
| 7 | Bài tập: SingleChoice, FillBlank, Matching, Ordering | |
| 8 | **Bài kiểm tra cuối A1** (Hören + Lesen, tính giờ theo phần) | Blueprint cấu hình |
| 9 | Kết quả + giải thích đáp án | |
| 10 | Tiến độ: bài đã học, tỷ lệ đúng theo kỹ năng, điểm kiểm tra | |
| 11 | Info Hub: Ausbildung, Recognition, Lộ trình học tiếng Đức | Có nguồn và ngày rà soát |
| 12 | Báo lỗi nội dung | Bù cho việc không có người duyệt chuyên môn |
| 13 | Admin dashboard | |
| 14 | CMS + quy trình duyệt + import CSV/JSON + AI viết nháp | |
| 15 | **"Bàn học"**: màn hình chính một cửa (streak, 3 nhiệm vụ ngày, "Tiếp tục học", "Ôn hôm nay", lưới lối vào) | CR-002 A1 |
| 16 | **Streak + XP + 3 nhiệm vụ ngày** | CR-002 A2 |
| 17 | **Flashcard + SRS** (SM-2 giản lược) cho từ vựng | CR-002 A3 |
| 18 | **Khách vào học ngay**: ngữ pháp công khai, luyện tập thử, tiến độ tạm lưu trên máy rồi gộp khi đăng ký | CR-002 A4 |
| 19 | **Mini-game "Nối từ nhanh"** | CR-002 A5 |
| 20 | **PWA** (cài lên điện thoại) | CR-002 A6 |
| 21 | **6 chế độ học từ + "Trộn"** (Lặp lại, Flashcard, Trắc nghiệm, Trắc nghiệm đảo, Đoán, Artikel), chung một lịch SRS | CR-003 A1 |
| 22 | **Thẻ từ giàu thông tin**: đồng nghĩa, cụm đi kèm, họ từ, cách dùng, kèm nguồn và nhãn AI | CR-003 A2, A6 |
| 23 | **Cài đặt cá nhân hóa**: mục tiêu XP, thẻ mới/ngày, thời lượng học, độ nghiêm khi chấm | CR-003 A3 |
| 24 | **Lưu từ + "Từ của tôi"** (tự nhập, không AI), phím tắt | CR-003 A4, A5 |

**Vai trò MVP:** Guest, Student, Admin (kiêm Content Manager).

**Nội dung MVP tối thiểu (mục tiêu):**
- Phát âm và bảng chữ cái (khoảng 6 bài).
- Khoảng **650 từ** thuộc phạm vi A1.
- Khoảng **25 điểm ngữ pháp**.
- Khoảng **30 bài học**.
- **300–400 câu hỏi**.
- **2–3 bài kiểm tra cuối A1**.
- Khoảng **8–10 bài Info Hub**.

## 8. Future Feature List (kèm đánh giá khả thi)

| Tính năng | Phiên bản | Giá trị | Độ phức tạp | Chi phí vận hành | Nhận xét |
|---|---|---|---|---|---|
| **A2** | 1.1 | Rất cao | Thấp (code) / Cao (nội dung) | 0 | Dùng lại toàn bộ hệ thống |
| ~~Flashcards + Spaced Repetition~~ | ~~V2~~ **MVP (CR-002)** | Rất cao | Trung bình | 0 | Chuyển lên MVP, dùng SM-2 giản lược. FSRS để V2 sau khi xác minh giấy phép |
| **B1** | V2 | Rất cao (mục tiêu visa Ausbildung) | Cao (nội dung, có Schreiben/Sprechen) | Thấp | |
| Tiếng Đức cho Ausbildung (phỏng vấn, giới thiệu bản thân, từ vựng nghề) | V2 | Cao | Trung bình | 0 | Khác biệt so với app đại trà |
| Phản hồi Schreiben bằng AI | V2 | Cao | Trung bình | Token LLM | Ghi rõ là "phản hồi tham khảo". Có quota |
| ~~Streak~~ | ~~V2~~ **MVP (CR-002)** | Cao | Thấp | 0 | Chuyển lên MVP cùng 3 nhiệm vụ ngày. Nhắc học qua email vẫn để V2 |
| Speaking: ghi âm và tự nghe lại | V2 | Trung bình | Thấp | 0 | |
| Pronunciation feedback (Azure) | V3 | Cao | Cao | Tính theo giờ | Thử free tier trước |
| B2 | V3 | Trung bình | Cao | 0 | |
| AI Tutor / hội thoại | V3 | Cao | Trung bình | Biến đổi | Có quota |
| Payment / Premium | V3 | Phụ thuộc mô hình | Cao | Phí cổng thanh toán | Chỉ làm khi đã có người dùng |

### Bổ sung theo CR-002 (đã chốt, phương án 2)
| Tính năng | Phiên bản | Ghi chú |
|---|---|---|
| Lộ trình trực quan theo tuần/mốc, cụm từ mỗi ngày, mini-game Artikel Blitz, cụm bài SEO `StudyTips`, bộ từ theo nghề (3 nghề), trang hướng dẫn lịch thi | **1.1** (cùng A2) | CR-002 tầng B |
| Chép chính tả bằng audio TTS của ta, shadowing ghi âm, đọc song ngữ truyện cổ, bảng xếp hạng tuần (tự nguyện), đổi giao diện/ảnh nền, Pomodoro, Ctrl+K, tour, tường góp ý | **V2** | CR-002 tầng C |
| Đấu trường realtime, cửa hàng kim cương, "Đơn đang mở" (qua API chính thức), "La bàn nghề" | **V3** hoặc không làm | CR-002 tầng D |

### Bổ sung theo CR-003 (đã chốt, phương án 2)
| Tính năng | Phiên bản |
|---|---|
| Bộ từ theo nghề (Bộ → Phần → Chủ đề con, 3 nghề), trang Thống kê (heatmap, 14 ngày), hội thoại chat điền chỗ trống, dịch Việt → Đức che chữ, tạm nghỉ chuỗi, "Có gì mới" | **1.1** |
| "Từ của tôi" có AI (cần sửa BR-PRIV-03), chép chính tả bằng audio TTS, đọc song ngữ, luyện nói ghi âm, viết có AI nhận xét, bảng xếp hạng | **V2** |
| Quán café 3D, cửa hàng hình nền/khung, thư viện video bên thứ ba | **V3 hoặc không làm** |
