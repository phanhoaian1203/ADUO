# 03 – Business Rules

> **Phiên bản 2 (CR-001):** chỉ tiếng Đức. Bao gồm rule cho bài kiểm tra A1 và từ vựng tiếng Đức.

Quy ước: mỗi rule có ID cố định để code, test và tài liệu cùng tham chiếu. Các giá trị số có ghi *(cấu hình)* nằm trong bảng cấu hình hoặc appsettings, không viết cứng trong code.

## 1. Identity & Privacy (BR-AUTH, BR-PRIV)

| ID | Quy tắc |
|---|---|
| BR-AUTH-01 | Email là duy nhất (so sánh không phân biệt hoa thường) và là định danh đăng nhập |
| BR-AUTH-02 | Mật khẩu tối thiểu **8 ký tự**, có cả chữ và số. Không trùng email. Không nằm trong danh sách mật khẩu phổ biến *(cấu hình)* |
| BR-AUTH-03 | Tên hiển thị dài 2–50 ký tự. Lọc từ ngữ thô tục cơ bản |
| BR-AUTH-04 | **Không giới hạn tuổi** (Q-P2-04). Người đăng ký phải tick đồng ý Điều khoản và Chính sách quyền riêng tư. Câu đồng ý ghi rõ: người dưới 16 tuổi phải được cha mẹ hoặc người giám hộ đồng ý. **Không thu thập ngày sinh** (thu thập tối thiểu). Hệ thống lưu thời điểm và **phiên bản** văn bản đã đồng ý |
| BR-AUTH-05 | Tài khoản Unverified được đăng nhập, học bài và làm bài tập |
| BR-AUTH-06 | Đăng nhập sai **5 lần liên tiếp** thì khóa tạm **15 phút** *(cấu hình)*. Thông báo lỗi chung chung ("Email hoặc mật khẩu không đúng") |
| BR-AUTH-07 | **Thi thử** và **báo lỗi** yêu cầu đã xác minh email. Lý do: chống spam, và để có kênh liên hệ khi cần |
| BR-AUTH-08 | Đặt lại mật khẩu thành công thì vô hiệu hóa mọi phiên đăng nhập khác |
| BR-AUTH-09 | Nếu đăng nhập Google (khi có) mà trùng email với tài khoản đã có, chỉ liên kết sau khi người dùng nhập đúng mật khẩu của tài khoản đó |
| BR-PRIV-01 | Chỉ thu thập: email, tên hiển thị, ảnh đại diện (tùy chọn), mục đích học và cấp độ mục tiêu (tùy chọn). **Không** thu thập số điện thoại, CCCD, địa chỉ ở MVP |
| BR-PRIV-02 | Xóa tài khoản: tài khoản bị vô hiệu hóa ngay. Dữ liệu cá nhân bị xóa hoặc ẩn danh trong **≤ 30 ngày**. Dữ liệu thống kê đã ẩn danh (ví dụ lượt trả lời câu hỏi) được giữ lại để tính độ khó câu hỏi |
| BR-PRIV-03 | Không gửi bất kỳ dữ liệu người học nào tới API AI hay TTS. Các API này chỉ xử lý nội dung học |
| BR-PRIV-04 | Báo lỗi có lưu `UserId` để chống spam, nhưng Admin chỉ thấy tên hiển thị |

> ⚠️ Pháp lý (rủi ro còn lại, đã chấp nhận): NĐ 13/2023 có yêu cầu riêng khi xử lý dữ liệu của trẻ em dưới 16 tuổi (cần sự đồng ý của cha mẹ hoặc người giám hộ). Vì không chặn theo tuổi, nền tảng dựa vào câu xác nhận trong ô đồng ý. Ở Phase 5, trước khi soạn Chính sách quyền riêng tư, cần đọc lại văn bản hiện hành (NĐ 13/2023, Luật Bảo vệ dữ liệu cá nhân) để xem cách này đã đủ chưa. Nếu chưa đủ, có thể thêm một câu hỏi "Bạn dưới 16 tuổi?" kèm bước xác nhận của phụ huynh.

## 2. Catalog & Learning (BR-CAT)

| ID | Quy tắc |
|---|---|
| BR-CAT-01 | Cây phân cấp: **Language → Program → Level → Course → Unit → Lesson**. Ví dụ: Tiếng Đức → Tiếng Đức theo CEFR → A1 → "Tiếng Đức A1 từ con số 0" → Unit 3 "Gia đình" → Bài 3.2 "Mạo từ sở hữu mein/dein" |
| BR-CAT-02 | Một nút chỉ hiển thị với người học khi **chính nó và mọi nút cha** đều Published |
| BR-CAT-03 | Mỗi Program có `ProgramType` và metadata riêng dạng JSON có schema. MVP chỉ có `CEFR` (cấp A1–C2). Sau này có thể thêm `Vocational` (tiếng Đức cho Ausbildung, V2), hoặc loại khác khi mở ngôn ngữ mới (CR-001) |
| BR-CAT-04 | Bài học gồm **các khối có thứ tự**. Khối từ vựng và ngữ pháp **tham chiếu** tới mục từ và điểm ngữ pháp dùng chung, không sao chép nội dung |
| BR-CAT-05 | Mỗi khóa có tối thiểu 1 bài `IsPreview = true` cho Guest |
| BR-CAT-06 | Không bắt buộc học tuần tự. Hệ thống chỉ gợi ý "bài tiếp theo". Lý do: nhiều người học đã học trước ở trung tâm, hoặc chỉ muốn ôn một chủ điểm |
| BR-CAT-07 | Enroll là thao tác ngầm (lần đầu mở bài) hoặc chủ động (nút "Tham gia"). Rời khóa không xóa tiến độ |

### Từ vựng tiếng Đức (BR-LEX)

| ID | Quy tắc |
|---|---|
| BR-LEX-01 | **Danh từ** phải có giống (`der`/`die`/`das`) và dạng số nhiều. Danh từ không có số nhiều đánh dấu `NoPlural`, danh từ chỉ có số nhiều đánh dấu `PluralOnly`. Thiếu các thông tin này thì không được publish |
| BR-LEX-02 | **Động từ** phải có bảng chia thì hiện tại (Präsens) cho 6 ngôi trước khi publish ở cấp A1. Có cờ `Separable` (động từ tách, ví dụ an·rufen) và `Irregular`. Perfekt (Partizip II + trợ động từ haben/sein) bắt buộc từ cấp A2 |
| BR-LEX-03 | Giao diện tô **màu theo giống** (der: xanh dương, die: đỏ, das: xanh lá), luôn kèm **chữ** mạo từ để không phụ thuộc vào màu (hỗ trợ người mù màu) |
| BR-LEX-04 | Tìm kiếm chấp nhận cả `ä/ö/ü/ß` lẫn `ae/oe/ue/ss`, không phân biệt hoa thường. Tìm theo nghĩa tiếng Việt chấp nhận gõ không dấu |

## 3. Progress (BR-PROG)

| ID | Quy tắc |
|---|---|
| BR-PROG-01 | Bài học được tính **Completed** khi: người học đã tới khối cuối **và**, nếu bài có bài tập, có ít nhất 1 lượt làm bài tập đạt **≥ 60%** *(cấu hình)*. Bài không có bài tập: chỉ cần tới khối cuối |
| BR-PROG-02 | Đã Completed thì không quay lại InProgress, kể cả khi lần làm sau thấp điểm hơn |
| BR-PROG-03 | % khóa = số bài Completed / số bài **đang Published** của khóa. Bài bị gỡ (Archived) không còn được tính |
| BR-PROG-04 | Tỷ lệ đúng theo kỹ năng chỉ tính trên các lượt trả lời trong 90 ngày gần nhất *(cấu hình)*, để phản ánh trình độ hiện tại |

## 4. Assessment – câu hỏi & luyện tập (BR-ASM)

| ID | Quy tắc |
|---|---|
| BR-ASM-01 | **SingleChoice**: từ 2 đến 4 phương án, **đúng 1** phương án đúng. Thứ tự phương án được xáo trộn khi hiển thị, trừ khi câu hỏi có cờ `KeepOptionOrder` (ví dụ đáp án là "①②③④" gắn với hình) |
| BR-ASM-02 | **FillBlank**: có danh sách đáp án chấp nhận được. So khớp bỏ khoảng trắng thừa. Tiếng Đức: phân biệt hoa thường mặc định (vì danh từ viết hoa), tắt được theo từng câu. Có tùy chọn chấp nhận `ae/oe/ue/ss` thay cho `ä/ö/ü/ß` |
| BR-ASM-03 | **Matching**: 3–6 cặp. Chấm theo **tỷ lệ cặp đúng** ở chế độ luyện tập. Ở chế độ thi, chỉ tính đúng khi đúng toàn bộ |
| BR-ASM-04 | **Ordering**: đúng khi trùng với một trong các thứ tự chấp nhận được |
| BR-ASM-05 | **Kiểm tra tự động** trước khi cho publish: có đủ trường bắt buộc, đúng số đáp án đúng, không có phương án trùng nhau, file media tồn tại, có giải thích, từ vựng trong câu nằm trong cấp độ khai báo (chỉ cảnh báo, không chặn) |
| BR-ASM-06 | Mỗi câu hỏi thuộc đúng **1 ngôn ngữ**, có **1 kỹ năng** (Reading/Listening, sau này thêm Writing/Speaking) và **1 dạng bài** (QuestionCategory) thuộc blueprint của chương trình |
| BR-ASM-07 | Câu hỏi dùng được cho cả bài tập lẫn đề thi. Một câu đã nằm trong đề Fixed đang Published thì không thể Archive nếu chưa gỡ khỏi đề |
| BR-ASM-08 | Hệ thống thống kê theo từng câu: số lượt trả lời và tỷ lệ đúng. Nếu tỷ lệ đúng **< 10%** hoặc **> 98%** sau ≥ 30 lượt *(cấu hình)*, câu được tự gắn cờ "Cần xem lại" |

## 5. Exam – thi thử (BR-EXAM)

| ID | Quy tắc |
|---|---|
| BR-EXAM-01 | Mỗi đề tham chiếu đúng **1 Exam Blueprint**. Blueprint định nghĩa: các phần (Section), thời gian mỗi phần, số câu, điểm, phân bố dạng bài |
| BR-EXAM-02 | **Blueprint "Kiểm tra cuối A1" (mặc định, CR-001)**, do nền tảng tự thiết kế. Phần 1 **Hören**: 15 câu, 20 phút. Phần 2 **Lesen**: 15 câu, 25 phút. Mỗi câu **1 điểm**, tổng **30 điểm**, hiển thị thêm dạng %. Thời lượng mượn khung phần thi viết Goethe A1 (Hören khoảng 20 phút, Lesen 25 phút; [Phase 1 – file 02 §1.10](../phase-1/02-resource-discovery.md), 🟡). **Số câu và cách tính điểm là lựa chọn của nền tảng**, không mô phỏng thang điểm Goethe. Schreiben và Sprechen chưa có ở MVP |
| BR-EXAM-03 | **Thời gian theo phần**: hết thời gian của một phần thì phần đó tự khóa và chuyển sang phần tiếp theo. Người học được chủ động chuyển phần sớm, nhưng **không quay lại** phần đã khóa (Q-P2-02). Thứ tự phần theo blueprint (A1: Hören rồi Lesen). Đây là lựa chọn của nền tảng để mô phỏng áp lực thời gian |
| BR-EXAM-04 | Giờ thi do **server** tính (`DeadlineAt` của từng phần). Client chỉ hiển thị. Câu trả lời gửi lên sau `DeadlineAt + 30 giây` *(độ trễ cho phép, cấu hình)* sẽ bị từ chối. Tác vụ nền tự nộp các lượt quá hạn |
| BR-EXAM-05 | Điểm = tổng điểm các câu đúng. Câu bỏ trống được 0 điểm. Không trừ điểm câu sai |
| BR-EXAM-06 | Mỗi người học chỉ có **1 lượt đang làm** cho mỗi đề. Không giới hạn số lần thi lại. Các lượt sau cùng đề Fixed hiển thị nhắc "Bạn đã làm đề này N lần" |
| BR-EXAM-07 | Mỗi lượt thi lưu **snapshot** (nội dung câu hỏi, phương án, đáp án đúng, điểm) tại thời điểm bắt đầu. Sửa câu hỏi sau đó không làm thay đổi kết quả cũ |
| BR-EXAM-08 | **Audio phần Hören**: mỗi audio phát tối đa **2 lần** ở chế độ thi (Q-P2-03). Giá trị cấu hình **theo từng phần** của blueprint, có thể đặt 1 lần cho các phần khó hơn. 🟡 Số lần phát của từng Teil trong bài thi Goethe thật chưa xác minh |
| BR-EXAM-09 | Đề **Generated**: rút ngẫu nhiên các câu Published đúng phân bố của blueprint. Ưu tiên câu người học chưa gặp. Nếu ngân hàng không đủ câu thì **không** cho tạo đề, và báo cho Admin |
| BR-EXAM-10 | Trang giới thiệu đề và trang kết quả luôn hiển thị: *"Đây là bài kiểm tra do nền tảng biên soạn theo khung trình độ A1 (CEFR), không phải đề thi Goethe, telc hay ÖSD. Kết quả chỉ để bạn tự đánh giá, không thay thế chứng chỉ."* |
| BR-EXAM-11 | **Mức đánh giá** (ngưỡng do nền tảng đặt, *cấu hình*): **≥ 60%** "Sẵn sàng sang A2". **40–59%** "Cần ôn thêm", kèm gợi ý bài học theo kỹ năng và dạng bài yếu. **< 40%** "Nên học lại các unit". Ngoài ra còn hiển thị điểm riêng từng kỹ năng. **Không** quy đổi ra "điểm Goethe" hay dự đoán đỗ hoặc trượt. |
| BR-EXAM-12 | Blueprint mới (A2, B1…) được tạo bằng dữ liệu, không cần sửa code. Khi thêm Schreiben (V2), câu tự luận được chấm bằng AI và gắn nhãn "phản hồi tham khảo", không tính vào mức đánh giá chính trừ khi blueprint cấu hình như vậy |

## 6. Content Ops (BR-CNT)

| ID | Quy tắc |
|---|---|
| BR-CNT-01 | Mọi bản ghi nội dung phải có `SourceId` + `LicenseId`. Nội dung tự soạn dùng Source = "ADUO (tự biên soạn)", License = "Proprietary" |
| BR-CNT-02 | Nếu License yêu cầu ghi công (BY), UI phải hiển thị tên nguồn và tên giấy phép kèm link, cạnh nội dung đó hoặc ở chân khối nội dung |
| BR-CNT-03 | **Chặn** import và publish nội dung có License với `AllowsCommercial = false` (theo PD-05). Ví dụ: CC BY-NC |
| BR-CNT-04 | Nội dung phái sinh từ nguồn **SA** (ví dụ nghĩa tiếng Việt dịch từ định nghĩa tiếng Anh của Wiktionary) **giữ nguyên** License SA của nguồn. Nội dung tự soạn chỉ *tham chiếu* (link tới mục từ) thì không bị ảnh hưởng |
| BR-CNT-05 | Vòng đời: `Draft → InReview → Published`. `InReview → Draft` (bị trả về, phải kèm ghi chú). `Published → NeedsUpdate` (do báo lỗi hoặc thống kê; **vẫn hiển thị**, kèm nhãn nhỏ "đang kiểm tra lại"). `Published / NeedsUpdate → Archived` (ẩn đi). Do chỉ có 1 người làm, Admin được chuyển thẳng `Draft → Published` nhưng **bắt buộc đi qua màn hình checklist** |
| BR-CNT-06 | **Checklist duyệt** gồm: nguồn và license đúng; nội dung chính xác (với câu hỏi: đáp án đúng, không mơ hồ); phù hợp cấp độ; tiếng Việt tự nhiên; media phát được; đã xem các cờ cảnh báo AI |
| BR-CNT-07 | Nội dung `Origin = AIDraft` bắt buộc chạy **AI kiểm định** trước khi publish. Kết quả (OK / cảnh báo + lý do) được lưu cùng bản ghi |
| BR-CNT-08 | Có **trần chi phí** AI và TTS mỗi tháng *(cấu hình, mặc định 10 USD)*. Vượt trần thì chặn các lượt gọi mới, đến tháng sau mới mở lại |
| BR-CNT-09 | Sửa nội dung đã Published sẽ tăng `Version` và lưu bản cũ vào lịch sử. Đề thi đang có lượt làm dở vẫn dùng snapshot (BR-EXAM-07) |
| BR-CNT-10 | Trùng lặp mục từ được xác định theo khóa `(Language, Lemma, PartOfSpeech, Homograph#)`. Import gặp bản trùng thì **không** ghi đè, mà đánh dấu để Admin chọn: bỏ qua / gộp / tạo mới |
| BR-CNT-11 | Audio từ nguồn ngoài (Commons, Tatoeba) chỉ được import khi có license **cho từng file** và license đó cho phép thương mại. Audio TTS ghi Source = "Azure TTS" (⚖️ chờ xác minh điều khoản lưu trữ, file 04 Phase 1) |
| BR-CNT-12 | Hủy một ImportBatch chỉ xóa các bản ghi **chưa từng Published**. Bản ghi đã Published phải Archive riêng |

## 7. Info Hub (BR-INFO)

| ID | Quy tắc |
|---|---|
| BR-INFO-01 | Mỗi bài có ≥ 1 `OfficialSource` (URL + tên cơ quan + ngày truy cập). Không publish được nếu thiếu |
| BR-INFO-02 | Mỗi bài có `Classification`: **Official** (tóm tắt trực tiếp từ nguồn của cơ quan nhà nước) hoặc **Reference** (tổng hợp, kinh nghiệm) |
| BR-INFO-03 | Có `ReviewDueAt` = ngày rà soát gần nhất + **6 tháng** *(cấu hình theo chuyên mục; "Visa & thủ tục Ausbildung" mặc định 3 tháng vì điều kiện và mức tài chính hay thay đổi)*. Chuyên mục MVP: **Ausbildung**, **Công nhận văn bằng**, **Lộ trình học & chứng chỉ tiếng Đức** |
| BR-INFO-04 | Quá `ReviewDueAt` thì bài tự chuyển `NeedsUpdate` và hiện banner "Thông tin có thể đã cũ, vui lòng kiểm tra nguồn chính thức". Dashboard Admin liệt kê các bài này |
| BR-INFO-05 | **Cấm** đưa vào bài viết: con số về lương, chi phí môi giới, cam kết đỗ, tuyển hay visa. Các con số như mức chứng minh tài chính chỉ được **trích dẫn nguyên văn kèm nguồn và ngày** |
| BR-INFO-06 | Bài về Recognition phải phân biệt ba loại thủ tục (học thuật / nghề được quản lý / nghề đào tạo theo BQFG) và dẫn đúng công cụ: anabin, Anerkennungs-Finder, BQ-Portal |

## 8. Báo lỗi (BR-RPT)

| ID | Quy tắc |
|---|---|
| BR-RPT-01 | Mỗi người dùng tạo tối đa **10 báo lỗi/ngày** *(cấu hình)*. Mỗi người chỉ có 1 báo lỗi đang mở cho mỗi nội dung |
| BR-RPT-02 | Một nội dung nhận **≥ 3 báo lỗi đang mở** từ 3 người khác nhau thì tự chuyển `NeedsUpdate` |
| BR-RPT-03 | Báo lỗi được Resolved hoặc Rejected kèm ghi chú. Người báo lỗi thấy trạng thái trong trang "Báo lỗi của tôi" (S) |
