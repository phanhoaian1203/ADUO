# CR-002 – Tích hợp bài học từ khảo sát Brottin, TPT Glanz, Deutsch Tiger

| Mục | Nội dung |
|---|---|
| Mã | CR-002 |
| Ngày | 2026-11-22 |
| Người yêu cầu | Chủ dự án (muốn trải nghiệm tương tự Brottin, rồi mở rộng) |
| Trạng thái | **Confirmed (phương án 2)** ngày 2026-11-22. Riêng Q-CR2-03 (hướng hình ảnh) chưa xác nhận riêng, tạm theo đề xuất |
| Căn cứ | [docs/research/01-competitor-analysis.md](../research/01-competitor-analysis.md) |
| Ảnh hưởng | Thêm 1 module (Engagement), sửa 5 module hiện có, đổi thứ tự một số tính năng V2 lên MVP |

## 1. Vấn đề
MVP hiện tại (Phase 1–3) có **hệ thống học đúng** nhưng thiếu 3 thứ mà cả ba trang đối thủ đều có, và là lý do người học quay lại:
1. **Ghi nhớ từ vựng theo lịch** (flashcard + lặp lại ngắt quãng).
2. **Vòng thói quen** (streak, mục tiêu ngày nhỏ).
3. **Vào học ngay**, ít rào cản đăng nhập.

Đồng thời MVP đang đặt người dùng vào một **dashboard khô** thay vì một "không gian học" có cảm xúc như Brottin.

## 2. Nguyên tắc chọn lọc
Không chép "mọi thứ họ có". Mỗi tính năng phải qua 4 câu hỏi:

| Câu hỏi | Nếu "không" thì |
|---|---|
| Có tái dùng được hệ thống đã thiết kế (Lexicon, Assessment, Progress) không? | Chi phí cao, hoãn |
| Một người làm và duyệt được không? | Hoãn |
| Nội dung/hình ảnh có hợp pháp (giấy phép rõ) không? | **Loại bỏ** |
| Có đúng với mục tiêu "học để đi Ausbildung" và cam kết không môi giới không? | Loại bỏ |

## 3. Quyết định đề xuất theo tầng

### Tầng A – Đưa vào **MVP** (rẻ, tác động lớn)
| # | Tính năng | Học từ | Tái dùng | Công sức (ngày công, ước tính) |
|---|---|---|---|---|
| A1 | **"Bàn học"**: màn hình chính một cửa thay dashboard. Lối vào: Lộ trình · Từ vựng · Ôn tập · Ngữ pháp · Hội thoại · Kiểm tra · Thông tin Ausbildung. Có thẻ streak + 3 nhiệm vụ ngày | Brottin | Progress, Catalog | 3–4 |
| A2 | **Streak + 3 nhiệm vụ mỗi ngày** (ví dụ: ôn 10 thẻ, làm 1 bài tập, nghe 1 hội thoại). Có XP nhẹ. **Chưa có** kim cương, cửa hàng | Brottin, TPT | Progress (`answer_events`) | 4–5 |
| A3 | **Flashcard + lặp lại ngắt quãng (SRS)** cho từ vựng, có mạo từ der/die/das, audio. Thuật toán SM-2 giản lược | Cả ba | Lexicon, audio TTS | 6–8 |
| A4 | **Khách vào học ngay**: ngữ pháp công khai (SEO), luyện tập chơi được khi chưa đăng nhập, tiến độ tạm lưu trên máy, **gộp vào tài khoản khi đăng ký** | Brottin, Tiger | Lexicon, Assessment, Next.js SSR | 4–5 |
| A5 | **Mini-game "Nối từ nhanh"** (khách chơi thử ngay trang chủ) | Tiger | Component Matching của Assessment | 2–3 |
| A6 | **PWA**: cài lên màn hình điện thoại, vỏ ứng dụng cache được | TPT, Tiger | Next.js | 1–2 |
| | **Tổng tầng A** | | | **≈ 20–27 ngày** (tăng khoảng 30–35% so với 56–78 ngày hiện có) |

### Tầng B – **Release 1.1** (cùng với A2)
| # | Tính năng | Ghi chú |
|---|---|---|
| B1 | Lộ trình **trực quan theo tuần/mốc** (có minh họa địa danh Đức do ta tự thiết kế) | Học từ Brottin. Chỉ là giao diện cho Catalog |
| B2 | **Cụm từ/tục ngữ mỗi ngày** | Brottin. Dùng khoảng 100 câu tục ngữ Đức, tự dịch, ⚖️ xác minh nguồn |
| B3 | **Mini-game "Artikel Blitz"** (der/die/das) | Tiger. Biến thể SingleChoice |
| B4 | **Cụm bài SEO** theo mẫu Tiger: tiêu đề dạng câu hỏi → quy tắc → ngoại lệ → bảng "từ người Việt hay nhầm" → FAQ → CTA vào bài tập. Ví dụ: mẹo der/die/das theo đuôi từ, so sánh Goethe/telc/ÖSD (chỉ dẫn tới nguồn chính thức) | InfoHub chuyên mục mới `StudyTips` |
| B5 | **Bộ từ vựng theo nghề Ausbildung**, bắt đầu 3 nghề (điều dưỡng, nhà hàng–khách sạn, cơ điện tử) | Cả ba đều có (Brottin 17 nghề, TPT 19). Nội dung tạo bằng AI nháp + duyệt, ⚠️ chất lượng chuyên ngành khó duyệt khi làm một mình |
| B6 | **Trang hướng dẫn lịch thi**: không chép lịch, chỉ chỉ dẫn tra trên cổng chính thức, ghi ngày rà soát | Mẫu trung thực của Tiger |

### Tầng C – **V2**
| # | Tính năng | Ghi chú |
|---|---|---|
| C1 | **Chép chính tả (Diktat)** dùng **audio TTS của ta** (không dùng video bản quyền), phím tắt nghe lại câu (người dùng Brottin đã đề xuất) | Tránh rủi ro bản quyền của video truyền hình |
| C2 | **Shadowing**: ghi âm và nghe lại | MediaRecorder, chưa cần AI |
| C3 | **Đọc song ngữ** truyện cổ (văn bản gốc thuộc phạm vi công cộng) | ⚖️ cần xác minh ấn bản và nguồn; bản dịch là của ta |
| C4 | **Bảng xếp hạng tuần** (tự nguyện, có biệt danh), huy hiệu | Có yếu tố quyền riêng tư |
| C5 | Đổi giao diện/ảnh nền, Pomodoro, Ctrl+K, tour lần đầu | Cải thiện trải nghiệm |
| C6 | Tường góp ý (đánh giá sao, admin trả lời) | Chỉ hiển thị phản hồi thật |
| C7 | AI nhận xét Viết (đã có trong kế hoạch V2) | |

### Tầng D – **V3 hoặc không làm**
| Tính năng | Quyết định |
|---|---|
| Đấu trường trực tuyến realtime, mạng xã hội, cửa hàng kim cương | V3 nếu có đủ người dùng. Chi phí vận hành và kiểm duyệt lớn |
| "Đơn đang mở" (danh sách vị trí Ausbildung) | V3. Cần xác minh API Ausbildungssuche của Bundesagentur (Phase 1: 🟡 chưa đọc được). **Không** tự nhập tay vì sẽ cũ như TPT |
| "La bàn nghề" (trắc nghiệm chọn ngành) | V3. Chỉ làm nếu dùng dữ liệu nghề chính thức (BERUFENET) |
| "Máy tính chi phí / lương" | **Không làm** cho tới khi có nguồn số liệu chính thức và cập nhật được (BR-INFO-05) |
| Kho đề từ nhà xuất bản, video/phụ đề chương trình truyền hình, ảnh nền của hãng phim | **Loại bỏ** (bản quyền) |
| Phễu thu số điện thoại, ẩn tên doanh nghiệp, "bao đậu" | **Loại bỏ** (ngoài phạm vi, trái nguyên tắc) |

## 4. Tác động lên kiến trúc và module

### 4.1 Module mới: **Engagement** (schema `engagement`)
Đảm nhận: SRS (thẻ, lịch ôn), streak, nhiệm vụ ngày, XP, mini-game, phiên khách và việc gộp tiến độ khách vào tài khoản. Chi tiết: [modules/08-engagement.md](../phase-4/modules/08-engagement.md).

**Vì sao tách module mà không nhét vào Progress hay Lexicon?**
- Lexicon là kho tri thức dùng chung, không nên biết gì về "người học nào ôn thẻ nào".
- Progress chỉ ghi nhận "đã học gì", còn Engagement điều khiển "cần ôn gì và hôm nay đã đạt mục tiêu chưa". Hai việc có vòng đời và tần suất thay đổi khác nhau.
- Tách thêm một schema chỉ tốn một migration. Không tốn gì về vận hành (vẫn một monolith).

### 4.2 Module hiện có bị ảnh hưởng
| Module | Thay đổi |
|---|---|
| **Identity** | Thêm luồng **gộp dữ liệu khách** khi đăng ký/đăng nhập lần đầu (nhận gói dữ liệu từ localStorage) |
| **Lexicon** | Ngữ pháp và một phần từ vựng **công khai cho khách** (SSR, SEO). Thêm cờ `IsPublic` cho điểm ngữ pháp. Chủ đề "cụm từ mỗi ngày" (tục ngữ) |
| **Catalog** | Trang **Lộ trình** trực quan (1.1). Bài học và hội thoại tự cho khách vào được khi `IsPreview` hoặc `IsPublic` |
| **Assessment** | Component Matching, SingleChoice dùng lại cho mini-game. Luyện tập chấm được **ở phía khách** (không cần server) với bộ câu hỏi công khai |
| **Progress** | Dashboard được thay bằng **"Bàn học"**. `answer_events` thêm nguồn `Game`, `Srs`. Streak lấy từ Engagement |
| **InfoHub** | Chuyên mục mới `StudyTips` (cụm bài SEO), trang hướng dẫn lịch thi (1.1) |
| **ContentOps** | Không đổi lõi. Thêm loại nội dung `DailyPhrase` vào `IContentRegistry` |

### 4.3 Những điểm cần quyết định kỹ thuật mới
- **Múi giờ của streak:** tính theo `Asia/Ho_Chi_Minh`, ngày học tính từ 00:00 đến 23:59 giờ Việt Nam. Người ở nước khác (ít) chấp nhận.
- **Chống gian lận XP/streak:** XP chỉ lấy từ sự kiện server ghi nhận (`answer_events`, thẻ SRS đã ôn), không tin số liệu từ client. Với khách, số liệu chỉ để trải nghiệm, không tạo XP thật cho tới khi gộp.
- **Chấm ở phía khách cho luyện tập công khai:** đáp án đúng phải nằm trên client, nên chỉ áp dụng cho **bộ câu hỏi công khai dùng để thử**. Bài kiểm tra có giờ thì **không** (vẫn chấm ở server, BR-EXAM).

## 5. Rủi ro và cách giảm thiểu
| Rủi ro | Mức | Giảm thiểu |
|---|---|---|
| **Phình phạm vi** (R1 đã nêu: khối lượng quá sức) | Cao | Chỉ tầng A vào MVP. Có mốc kiểm soát: nếu trễ hơn 30% thì chuyển A5, A6 sang 1.1 |
| Hiệu ứng "game hóa" lấn át việc học thật | Trung bình | Nhiệm vụ ngày đo **việc học có ý nghĩa** (ôn thẻ, làm bài đúng), không đo "đăng nhập". Không có phần thưởng bằng tiền ảo ở MVP |
| Nội dung công khai làm lộ đáp án bài kiểm tra | Thấp | Bộ câu hỏi công khai tách riêng khỏi ngân hàng đề thi (cờ `IsPublicPractice`) |
| Thiết kế ảnh nền/mascot bị nhầm với tài sản của người khác | Trung bình | Chỉ dùng hình tự vẽ, CC0 hoặc đã mua giấy phép. Ghi nguồn |
| SRS: thuật toán chọn sai làm người học nản | Trung bình | Bắt đầu bằng SM-2 giản lược (đã được hiểu rõ). Có số liệu để chỉnh sau |
| Chi phí vận hành tăng (người dùng khách nhiều hơn) | Thấp | Khách chỉ đọc nội dung tĩnh/cache. Không gọi AI hay TTS trong luồng người học (AD-04) |

## 6. Cần bạn xác nhận
| # | Câu hỏi | Đề xuất của tôi |
|---|---|---|
| **Q-CR2-01** | **Phạm vi đưa vào MVP** | **Phương án 2 (khuyến nghị): đưa toàn bộ tầng A vào MVP (≈ +20–27 ngày).** Tầng B vào Release 1.1 cùng A2. Hai phương án còn lại: **Phương án 1 (tinh gọn)**: chỉ A1 + A2 + A3 (Bàn học, streak/nhiệm vụ, SRS), khoảng +13–17 ngày, A4–A6 để 1.1. **Phương án 3**: giữ MVP như cũ, đưa toàn bộ vào 1.1 |
| **Q-CR2-02** | Thêm **module Engagement** (schema `engagement`) | Đồng ý |
| **Q-CR2-03** | **Hướng hình ảnh "Bàn học"** | Giai đoạn đầu dùng **gradient + hình minh họa vector tự thiết kế** (địa danh Đức dạng đơn giản). Không dùng ảnh nền phong cách hãng phim. Có thể nâng cấp bằng tranh thuê vẽ khi có người dùng |

**Kết quả:** chủ dự án chọn **phương án 2**. Q-CR2-01 và Q-CR2-02 được coi là đã chốt (module Engagement nằm trong phương án 2). Q-CR2-03 chưa được trả lời riêng, nên theo đề xuất mặc định và sẽ hỏi lại trước khi làm giao diện "Bàn học".

## 7. Ảnh hưởng lên lịch
| | Trước CR-002 | Sau CR-002 (phương án 2) |
|---|---|---|
| Phần code MVP | 56–78 ngày công | **≈ 76–105 ngày công** |
| Ra beta (làm khoảng 15–20 giờ/tuần) | 6–8 tháng | khoảng 8–10 tháng. Nếu bạn làm gần full-time như hiện nay thì rút ngắn nhiều |
| Giá trị | Hệ thống học đúng nhưng "khô" | Có vòng thói quen, vào học ngay, ghi nhớ từ vựng, và khác biệt rõ nhờ thông tin Ausbildung có nguồn |

*Đây là ước tính thô, giống các ước tính ở Phase 4. Sẽ chỉnh khi hoàn thành module đầu tiên và đo được tốc độ thực.*
