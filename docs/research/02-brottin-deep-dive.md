# Phân tích sâu Brottin (sau đăng nhập) và câu hỏi "có dùng lại được tài nguyên từ vựng không?"

> Khảo sát ngày **2026-11-22**, bằng **phiên đăng nhập của chủ dự án** trong trình duyệt của Claude. Tôi chỉ **đọc và xem**: không điểm danh, không trả lời bài, không đánh dấu đã thuộc, không vào quán 3D (vì sẽ hiện nhân vật của chủ dự án cho người khác). Việc mở một phiên học và chuyển giữa các chế độ có thể được hệ thống của họ ghi là "đã xem", nhưng không có đáp án nào được gửi.
> Tài liệu này **không sao chép nội dung** của Brottin. Chỉ ghi tính năng, cấu trúc, số liệu thống kê và nhận định. Tên người dùng khác (bảng xếp hạng) không được ghi lại.
> Bổ sung cho [01-competitor-analysis.md](01-competitor-analysis.md) (phần trước đăng nhập).

## 1. Câu trả lời ngắn cho câu hỏi chính

**Không nên và không được lấy bộ từ vựng của Brottin đưa vào ADUO**, kể cả khi họ cho tải xuống file Excel/CSV. Có ba lý do độc lập, chỉ cần một lý do là đủ:

| # | Lý do | Căn cứ |
|---|---|---|
| 1 | **Điều khoản của họ cấm.** Điều 6 (cập nhật 2026-09-22) nói phần nội dung do Brottin biên soạn thuộc về Brottin, người dùng được dùng để **tự học**, và **không được sao chép hàng loạt, thu thập tự động hay phát hành lại**. Chức năng "Xuất danh sách / Tải về" phục vụ việc tự học, không phải một giấy phép cho phép tái phân phối | Trang `/vi/terms`, đọc trực tiếp |
| 2 | **Không đáp ứng nguyên tắc của dự án.** BR-CNT-01 bắt buộc mọi nội dung có **Source + License rõ ràng**. File xuất **không có cột nguồn hay giấy phép**, và điều khoản của họ không cấp quyền nào cho ta. Theo phân loại ở Phase 1 file 04, đây là **Group D (không đủ điều kiện sử dụng)** | Phase 1: 04-license-assessment |
| 3 | **Chất lượng chưa đạt chuẩn của ta.** Phân tích file CSV bạn gửi (mục 4) cho thấy 55% danh từ **thiếu số nhiều**, không có IPA, không có audio, không có bảng chia động từ. Ta sẽ phải làm lại phần lớn, nên sao chép gần như không tiết kiệm được công | Mục 4 |

Điều **làm được** (hợp pháp và có lợi hơn): học **cách tổ chức** của họ (ý tưởng, cấu trúc, trải nghiệm thì không bị bảo hộ như nội dung), rồi **tự xây bộ từ vựng nghề** bằng nguồn có giấy phép rõ. Chi tiết ở mục 6.

## 2. Phạm vi tính năng sau đăng nhập

| Khu | Đường dẫn | Tình trạng khảo sát |
|---|---|---|
| Bàn học (trang chủ) | `/vi` | Đã xem |
| Từ vựng (bộ từ, 5 chế độ học, thẻ chi tiết) | `/vi/vocabulary` | Đã xem, **không trả lời** |
| Ngữ pháp | `/vi/grammar` | Đã xem (phần trước) |
| Nghe & Shadowing | `/vi/dictation` | Thư viện video đã xem, **chưa vào bài** |
| Dịch tiếng Đức | `/vi/translate` | Đã xem |
| Luyện viết (AI chấm) | `/vi/writing` | Danh sách đề đã xem |
| Luyện nói / phát âm | `/vi/speaking` | Đã xem |
| Hội thoại theo cảnh ("Szene") | `/vi/szene/...` | Đã xem một cảnh |
| Đọc sách song ngữ | `/vi/reading/...` | Đã xem trang mở sách |
| Đọc báo (Brottin-Zeitung) | `/vi/news` | Đã xem |
| Luyện thi | `/vi/exam-prep` | Đã xem danh mục |
| Ôn tập (SRS) | `/vi/review` | Chỉ thấy trạng thái rỗng |
| Bảng điều khiển / thống kê | `/vi/dashboard` | Đã xem |
| Nhiệm vụ, đấu trường | `/vi/mission`, `/vi/arena` | Đã xem |
| Cửa hàng ngọc | `/vi/shop` | Đã xem |
| Cộng đồng, thông báo | `/vi/community`, `/vi/feedback` | Đã xem |
| Thị trấn Brottin (quán café 3D) | `/vi/cafe` | Chỉ thấy màn chọn nhân vật |
| Cài đặt cá nhân hóa | `/vi/settings` | Đã xem |
| Ghi chú | `/vi/notes` | Đã xem |
| Điều khoản, gói Pro | `/vi/terms`, `/vi/pro` | Đã đọc |
| Chưa xem được | Hồ sơ, "Từ của tôi" (thư viện cá nhân), nội dung bên trong bài nghe/viết/nói và đề thi | Không thấy đường dẫn trực tiếp hoặc cần thao tác làm thay đổi tài khoản |

## 3. Phân tích từng khu

### 3.1 Từ vựng: khu bạn ấn tượng nhất
**Tổ chức bộ từ**
- Bộ từ chia theo cấp (A1 cơ bản/bổ sung/mở rộng/nâng cao… đến C2) và theo **chủ đề**, cộng nhóm **"Du học nghề" 17 nghề**. Mỗi bộ có vòng tiến độ ngay trên danh sách.
- Mở một bộ hiện hộp thoại: số thẻ + số đã thuộc, **"Học tiếp · Phần 1"**, **"Luyện Artikel"**, danh sách **phần** (bộ Điều dưỡng có **499 thẻ chia 2 phần**, khoảng 250 thẻ mỗi phần, mỗi phần có tiến độ riêng), **"Thêm vào bộ của tôi"** và **"Xuất danh sách / Tải về"**.
- Bên trong một phần, thẻ lại chia theo **danh sách chủ đề con** (ví dụ "Cơ thể & giải phẫu", **50 thẻ**), có tiến độ từng chủ đề con (0/50).
- Học theo cỡ nhỏ (khoảng 50 thẻ) rồi mới sang chủ đề con khác là một thiết kế giảm quá tải rất tốt.

**5 chế độ học** (tôi suy ra tên từ hành vi giao diện):
| Chế độ | Cách hoạt động |
|---|---|
| **Đoán** | Hiện **định nghĩa** (tiếng Anh và tiếng Việt) và một câu ví dụ, người học **gõ từ tiếng Đức**. Số dấu chấm cho biết độ dài từ. Nút **Gợi ý (Tab)**, **Không biết (Space)**, chuyển sang kiểu **ô chữ** từng ký tự |
| **Flashcard** | Mặt trước là mạo từ có màu + từ + IPA + nút nghe. Bấm để lật xem nghĩa |
| **Trắc nghiệm** | Hiện nghĩa tiếng Việt, chọn 1 trong 4 từ Đức. Các đáp án gây nhiễu **cùng chủ đề nghề** |
| **Trắc nghiệm đảo** | Hiện từ Đức, chọn nghĩa tiếng Việt trong 4 lựa chọn |
| **Lặp lại** | Thẻ đầy đủ + 4 nút chấm **Học lại / Khó / Tốt / Dễ** (phím tắt Enter). Đây là chế độ SRS |

Ngoài ra có **"Luyện Artikel"** riêng cho mạo từ der/die/das, và hai nút trên mọi thẻ: **"Lưu từ"** và **"Báo lỗi"**.

**Thẻ từ đầy đủ**: mạo từ **có màu**, từ, **IPA**, nghĩa tiếng Việt, **định nghĩa tiếng Anh**, **định nghĩa tiếng Việt**, câu ví dụ (có bản Anh và Việt, và một **câu Đức có audio**), cộng **5 mục mở rộng**: *Từ đồng nghĩa · Cụm từ đi kèm · Họ từ · Cách dùng · Lưu ý cách dùng*. Mục "Họ từ" liệt kê từ liên quan kèm loại từ, nghĩa và audio từng từ.

**Nhận xét quan trọng**
1. **Câu ví dụ và bản dịch có dấu hiệu do AI tạo.** Điều khoản của họ nói rõ một phần nội dung luyện tập (câu ví dụ, hội thoại, bản dịch) do AI tạo và được ghi nhãn (mục 3.9). Ví dụ trong bộ Điều dưỡng đều xoay quanh bệnh nhân và bệnh viện, rất nhất quán với một prompt "viết ví dụ theo ngành".
2. **Dữ liệu trong file xuất nghèo hơn nhiều so với thẻ hiển thị** (mục 4). Phần giàu (họ từ, đồng nghĩa, cụm, cách dùng) **không** nằm trong file.
3. **Học theo "phần" và "chủ đề con" là mô hình tốt** cho người học ít thời gian.

### 3.2 Nghe & Shadowing
Thư viện **video YouTube** theo cấp CEFR (podcast "Easy German", series dạy tiếng của đài truyền hình Đức, video giải thích, cả đoạn phim điện ảnh). Có **"Tạo mới"** (người dùng tự thêm link YouTube), **"Luyện nghe theo câu"** (chép chính tả từng câu), tìm kiếm theo tên/cấp độ, trạng thái "Chưa học". Người dùng đã đề xuất phím tắt nghe lại câu ở phần góp ý.
**Rủi ro:** thư viện dựa trên **video và phụ đề của bên thứ ba** (đài truyền hình, kênh YouTube, phim). Nhúng video YouTube là cách làm được phép theo điều khoản của YouTube, nhưng **bản chép lời và phụ đề từng câu** có thể là tác phẩm có bản quyền. ADUO không đi theo hướng này (đã chốt ở CR-002).

### 3.3 Dịch tiếng Đức (Việt → Đức)
Hai kiểu: **"Theo từ"** và **"Đoạn văn dài"**, lọc theo cấp A1–C2 và chủ đề, ví dụ "1/104 đoạn · 21 từ". Hiện một đoạn tiếng Việt, người học viết bản dịch tiếng Đức. **Câu mẫu bị che** thành từng từ chỉ lộ chữ cái đầu (ví dụ "M****"), **gõ đúng thì từ mở ra**, có "Xem gợi ý", "Nghe cả đoạn", đếm "0/21 từ đúng". Phím: Enter kiểm tra, Shift+Enter xuống dòng, Ctrl+Space mở từ kế.
**Đánh giá:** cơ chế **che chữ + mở dần** rất hay để luyện **sản xuất ngôn ngữ**, thứ mà bài trắc nghiệm không làm được. Dữ liệu câu có thể lấy hợp pháp từ **Tatoeba (CC BY / CC0)** thay vì tự tạo bằng AI.

### 3.4 Luyện viết (AI chấm)
Chọn cấp độ, chọn **đề**, viết, AI chấm. Đề có hai loại: **"Đề thi"** gắn nhãn kỳ thi (Goethe, telc, DTZ) kèm khoảng số từ (20–40, 25–45) và **"Hằng ngày"** (giới thiệu gia đình, ngày của tôi…). Cho thấy: nhãn kỳ thi trên đề của một bên không phải tổ chức thi là một **rủi ro bản quyền và hiểu nhầm**.

### 3.5 Luyện nói / phát âm
Hiện một câu, tách từng từ, người học **bấm micro đọc to**, có "Nghe mẫu", trạng thái "Sẵn sàng", tiến độ "x/500 câu". Có hai tab **"Luyện phát âm"** và **"Đề thi nói"**. Điều khoản xác nhận **phát âm được chấm bằng mô hình AI của bên thứ ba**, và nội dung gửi vào sẽ được chuyển tới nhà cung cấp mô hình.

### 3.6 Hội thoại theo cảnh ("Szene")
**50 cảnh** A1–C1. Mỗi cảnh ghi cấp, thời lượng (3 phút) và số từ đã vững. Giao diện là **khung chat hai người** (nhân vật ví dụ "MA" và "Du"). Câu của người học có **chỗ trống** (5 chỗ cho một cảnh 13 câu), mỗi chỗ là một từ trong **"Túi từ · cảnh này"**. Có **"Nghe lại câu", "Gợi ý", "Không biết"**, Enter để kiểm tra, và **"Bản ghi đầy đủ"** (mở ra đọc lại sau khi luyện).
**Đánh giá:** đây là cách học từ **trong ngữ cảnh** rất tự nhiên, và **rất dễ làm bằng hệ thống của ta** (khối `Dialogue` + câu hỏi `FillBlank`).

### 3.7 Đọc sách song ngữ
120 cuốn, trang mở sách dạng **bìa sách 3D**, có tác giả và năm ấn bản (ví dụ 1921, một ấn bản cũ), tiêu đề tiếng Việt, **mục lục theo chương** ("2 trang"), mô tả ngắn, và **"từ vựng được chú giải sẵn"** (chạm vào từ khi đọc để xem lại).
**Lưu ý chất lượng:** tiêu đề tiếng Việt của một truyện (*Das Bürle im Himmel*) được dịch thành "Linh hồn nhỏ…", trong khi chú giải ngay bên dưới của chính nó giải thích "Bürle" là *người nghèo*. Đây là **dấu hiệu** (tôi không khẳng định chắc) của việc **dịch bằng AI chưa được rà**. Bài học cho ADUO: bản dịch AI **luôn cần người duyệt** và phải ghi nhãn.

### 3.8 Đọc báo ("Brottin-Zeitung")
**68 bài** B1–C1, 11 chuyên đề (Arbeit, Umwelt, Gesundheit, Bildung, Migration, Digitales, Wohnen, Wirtschaft, Kultur…), mỗi bài ghi cấp, thời gian đọc, ngày và nhãn **"KI-GENERIERT"**. Có dòng tuyên bố đây là **văn bản học do AI tạo**, "bình luận và phân tích, không phải tin tức", soạn **không chứa số liệu, trích dẫn hay nhận định về người thật**.
**Đánh giá:** một cách làm **có trách nhiệm** với nội dung AI (nhãn rõ + giới hạn nội dung rủi ro). Đáng học.

### 3.9 Điều khoản và cách họ nói về AI
- Mục 5: phần **chấm viết, chấm phát âm, tạo bộ từ, trò chuyện** dùng mô hình bên thứ ba; kết quả AI **có thể sai**, không thay giáo viên hay kỳ thi thật.
- Họ **nêu Điều 50 của Đạo luật AI của EU** làm căn cứ để ghi nhãn nội dung AI. Tôi không đánh giá việc áp dụng pháp luật này. Chỉ lưu ý rằng họ **chọn ghi nhãn minh bạch**.
- Mục 2 và trang `/vi/pro`: **hoàn toàn miễn phí, không có gói trả phí**, ủng hộ qua mã QR là quyên góp tự nguyện. Nhưng **trang luyện thi vẫn còn nhãn "Pro"** trên vài bộ đề (có thể là giao diện cũ chưa dọn). Từng có gói Pro, nay đã bỏ.
- Mục 8: người dùng **tự xóa tài khoản** và dữ liệu học trong trang Hồ sơ.

### 3.10 Luyện thi
Danh mục bộ đề: **ôn ngữ pháp** (18 bài, 2 miễn phí), **"Goethe"** (15 bài), **"telc"** (5 bài, ghi "chính thức"), **đề thi THPT Quốc gia môn tiếng Đức** (7 bài), **DTZ** (2 bài do g.a.s.t., kèm audio). Có "Lộ trình luyện thi" cho "Chứng chỉ Goethe A1" (từ vựng A1 12 bộ thẻ, nghe và chính tả 3 bài).
**Nhận xét:** việc **dùng nhãn Goethe, telc** và ghi "chính thức" cần được hỏi lại về bản quyền và nguồn. Điểm **đáng chú ý cho nhóm người học của bạn**: có đề **THPT Quốc gia môn tiếng Đức**. Đề thi của Bộ Giáo dục thường công khai, nhưng **điều kiện tái sử dụng chưa được xác minh** (⚖️), nên chưa nên đưa vào kế hoạch.

### 3.11 Bảng điều khiển và thống kê (rất đáng học)
- **Chuỗi ngày** theo tuần T2→CN, nút **"Điểm danh ngay"** (điểm danh thủ công).
- **Nhiệm vụ hằng ngày** (3 nhiệm vụ), "hoàn thành tất cả nhận **200 XP**".
- **Tiến trình học tập:** thời gian học **hôm nay** và **tuần này**, số **ngày hoạt động** trong tháng, **chuỗi hiện tại và kỷ lục**, **từ đã học (+ tuần này)**, **phân bổ theo kỹ năng**, biểu đồ **hoạt động 14 ngày** (phút mỗi ngày), **lịch học dạng heatmap nhiều tháng**, chi tiết từng ngày.
- **Mục tiêu theo kỹ năng** (ví dụ từ vựng 400, bài học 50, bài thi 20, chính tả 50, luyện nói 50) với % hoàn thành.
- **Bảng xếp hạng theo thời gian học** (giờ và phút). **Top 3 cuối tháng nhận huy hiệu + kim cương.**

### 3.12 Kinh tế "ngọc" và cửa hàng
Vật phẩm: **bảo vệ chuỗi** (50 ngọc, giữ chuỗi một ngày), **tăng tốc XP** (100 ngọc, gấp đôi XP 24 giờ), **sửa chuỗi ngày** (300 ngọc, khôi phục một ngày quên điểm danh), **danh hiệu** (100–750), **khung avatar** (1.500), **hơn 40 hình nền hồ sơ** (2.000, chủ đề như anime, cyberpunk, Halloween, Tết, và cả tên gọi gợi nhớ các trò chơi/phim có bản quyền).
**Đánh giá:** cơ chế **"bảo vệ chuỗi/sửa chuỗi"** rất thiết thực (giảm nản khi lỡ ngày). Phần **mỹ phẩm bán bằng ngọc** thì nặng về thiết kế và nội dung hình ảnh, có rủi ro sở hữu trí tuệ, không hợp với giai đoạn đầu của ta.

### 3.13 Cộng đồng, Thông báo, Thị trấn 3D
- **Mạng xã hội Brottin:** tab Thông báo, Chia sẻ cộng đồng, Hoạt động. Admin đăng **nhật ký cập nhật** ("ra mắt giao diện Pastel và Clay", "thêm trắc nghiệm ngữ pháp"). Đây là kênh giữ chân rất rẻ.
- **Thị trấn Brottin:** một **quán café 3D nhiều người chơi** (chọn 1 trong 8 nhân vật, di chuyển bằng WASD, thấy người khác trong quán), có lời chào bằng tiếng Đức trong từng nhân vật. Rất ấn tượng nhưng **rất nặng** (WebGL, realtime, kiểm duyệt).
- **Từ của tôi (thư viện cá nhân):** người dùng **tự nhập từ**, hệ thống tạo **câu ví dụ hoàn chỉnh theo cấp độ đã chọn**, từ đồng nghĩa và giải thích cách dùng (theo thông báo của admin, tính năng AI).
- **Ghi chú của tôi** (Ctrl+Enter để thêm), thanh tìm kiếm toàn cục, thông báo (chuông).

### 3.14 Cài đặt cá nhân hóa
Chỉ 4 thiết lập nhưng đúng chỗ: **mục tiêu XP hằng ngày**, **số thẻ mới mỗi ngày**, **ngôn ngữ dịch** (Việt / Anh / Đức), **độ nghiêm khi chấm** (Dễ / Thường / Khó). Thêm: giao diện (kiểu Pastel/Clay, ảnh nền), nút **"Cài đặt"** ở thanh trên (cài ứng dụng).

## 4. Phân tích file CSV bạn gửi (`brottin-de-beruf-pflege-part-1.csv`)

> Chỉ thống kê cấu trúc và chất lượng, **không** sao chép nội dung.

| Chỉ số | Kết quả |
|---|---|
| Định dạng | CSV, UTF-8 có BOM, dấu phân cách `;`, xuống dòng CRLF (mở trong Excel được) |
| Số dòng | **250 mục từ** (khớp "Phần 1: 1–250" của bộ Điều dưỡng 499 thẻ) |
| **8 cột** | `Từ`, `Số nhiều`, `Loại từ`, `Nghĩa`, `Ví dụ`, `Nghĩa ví dụ`, `CEFR`, `Trạng thái` |
| Loại từ | **249 danh từ + 1 dòng trống**. **Không có động từ hay tính từ** (bộ này toàn danh từ) |
| Mạo từ | 249/249 danh từ có mạo từ đứng trước (`der/die/das`) |
| **Số nhiều** | **Chỉ 113/249 danh từ có** (45%). 137 dòng bỏ trống, không phân biệt được "không có số nhiều" với "chưa có dữ liệu" |
| CEFR | A1: 20 · A2: 56 · B1: 92 · B2: 72 · C1: 8 · C2: 2. **Mỗi từ có nhãn cấp riêng**, phần lớn B1–B2 |
| Cột `Trạng thái` | Toàn bộ "Chưa thuộc". Đây là **trạng thái học của người dùng**, cho thấy file xuất kèm dữ liệu cá nhân |
| Trùng từ | 0 |
| Độ dài ví dụ trung bình | khoảng 82 ký tự, mỗi từ **đúng 1 câu ví dụ + bản dịch** |
| **Không có** | IPA, audio, nguồn, giấy phép, ghi chú cách dùng, đồng nghĩa, cụm đi kèm, họ từ, bảng chia động từ, định nghĩa tiếng Anh |
| Lỗi dữ liệu thấy được | Một dòng **không có mạo từ và không có loại từ** (một danh từ bị thiếu cả hai) |

**So với yêu cầu của ADUO (BR-LEX-01):** danh từ phải có giống và hoặc số nhiều hoặc đánh dấu `NoPlural`/`PluralOnly` trước khi publish. File này **vi phạm điều đó ở 55% số danh từ**, nên không publish được nếu không bổ sung.

**Bài học cấu trúc đáng giữ:** phân tách ví dụ (câu Đức + bản dịch), nhãn CEFR theo **từng từ**, đặt tên "phần" theo khoảng số thứ tự, chủ đề con khoảng 50 từ.

## 5. Điểm mạnh và điểm yếu của Brottin (sau khi thấy bên trong)
| Điểm mạnh | Điểm yếu / rủi ro |
|---|---|
| Độ phủ rất rộng (từ, ngữ pháp, nghe, viết, nói, đọc, thi) | **Nhiều nội dung do AI tạo, bản dịch có dấu hiệu chưa rà** |
| Thiết kế thẻ từ giàu thông tin | **Không ghi nguồn/giấy phép** của dữ liệu và video |
| Cá nhân hóa đúng chỗ (4 thiết lập) | Thư viện video, nhãn kỳ thi, hình nền theo thương hiệu: **rủi ro bản quyền** |
| Thống kê học chi tiết, heatmap | Kinh tế ngọc, cửa hàng, thị trấn 3D: **chi phí vận hành lớn** cho một dự án "phi lợi nhuận" |
| Cộng đồng, nhật ký cập nhật | **Thiếu thông tin Ausbildung/Recognition có nguồn chính thức** (khoảng trống của ADUO) |
| Tính minh bạch về AI (nhãn, điều khoản) | Mô hình **phụ thuộc vào một admin** và quyên góp |

## 6. Hướng đi hợp pháp để có "bộ từ vựng nghề" cho ADUO

| Con đường | Mô tả | Đánh giá |
|---|---|---|
| **A. Tự xây bằng nguồn có giấy phép** (khuyến nghị) | (1) Lập **khung chủ đề** cho từng nghề từ tài liệu chính thức (khung đào tạo/lehrplan, mô tả nghề). (2) Lấy **giống, số nhiều, chia động từ, IPA** từ Wiktionary (CC BY-SA), kiểm tra bắt buộc theo BR-LEX-01. (3) AI **viết nháp** nghĩa tiếng Việt và câu ví dụ theo ngữ cảnh nghề. (4) Duyệt theo quy trình đã chốt. (5) Ghi `Origin` và hiển thị nhãn "có hỗ trợ AI" | Hợp pháp, kiểm soát chất lượng, **tốt hơn** file của Brottin ở số nhiều, IPA, audio TTS |
| **B. Xin phép Brottin bằng văn bản** | Admin trả lời từng góp ý, nên có thể thương lượng. Nếu họ **cho phép bằng văn bản** kèm giấy phép (ví dụ CC BY-SA) thì mới được xem là Source hợp lệ. Cần lưu bằng chứng và ghi công | Khả thi nhưng **không chắc**, và nội dung vẫn cần duyệt vì có lỗ hổng chất lượng. Mẫu tin nhắn ở phụ lục |
| **C. Dùng làm tài liệu tham khảo, không nhập dữ liệu** | Đọc để hiểu cần những chủ đề nào, rồi tự xây (như A). **Không** để file vào repo, không đưa vào pipeline import | An toàn. Ý tưởng và cấu trúc không bị bảo hộ như nội dung cụ thể |

**Về nguồn dữ liệu nghề (cần xác minh, ⚖️ 🟡):**
- Wiktionary có **danh mục theo lĩnh vực** (ví dụ thuật ngữ y học). Dữ liệu `wiktextract` chứa trường chủ đề/danh mục. **Chưa kiểm tra** độ phủ thực tế cho thuật ngữ điều dưỡng, nên đây là bước khám phá đầu tiên.
- Các **văn bản chính thức** về đào tạo nghề (khung chương trình, quy chế) do cơ quan nhà nước Đức ban hành thường được xem là **tác phẩm chính thức không có bản quyền** theo luật Đức. **Tôi chưa xác minh** điều này cho từng tài liệu cụ thể. Cần đọc điều khoản của từng nguồn trước khi dùng (⚖️).
- **Tatoeba** (CC BY/CC0) cho các cặp câu Đức–Việt dùng trong bài dịch và ví dụ.

## 7. Phụ lục: mẫu tin nhắn xin phép (nếu bạn muốn thử con đường B)
*Bạn tự gửi (qua kênh liên hệ của họ). Tôi không gửi thay.*

> Chào Admin Brottin, mình đang tự xây một nền tảng học tiếng Đức bằng tiếng Việt (hướng tới người học Ausbildung, phi thương mại ở giai đoạn đầu) và rất ấn tượng với cách Brottin tổ chức bộ từ vựng theo nghề.
> Mình có hai câu hỏi: (1) Brottin có cho phép dùng lại danh sách từ vựng theo nghề trong sản phẩm của mình không, nếu có thì theo giấy phép nào (ví dụ CC BY-SA) và cần ghi công thế nào? (2) Nếu không, mình sẽ tự xây bộ từ riêng và chỉ tham khảo ý tưởng tổ chức chủ đề, mình mong được xác nhận như vậy là ổn.
> Cảm ơn bạn rất nhiều!

Nếu họ không trả lời hoặc từ chối, ta đi con đường A hoặc C. **Không** coi sự im lặng là đồng ý.
