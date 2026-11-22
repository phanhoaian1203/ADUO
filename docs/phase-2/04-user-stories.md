# 04 – User Stories & Acceptance Criteria

> **Phiên bản 2 (CR-001):** chỉ tiếng Đức.

Mẫu viết: *Là [actor], tôi muốn [hành động], để [giá trị].* Acceptance Criteria (AC) viết theo dạng Given / When / Then và sẽ trở thành test case ở Phase 5.

Các story được nhóm theo **Epic**. Thứ tự Epic cũng là thứ tự triển khai ở Phase 4.

---

## Epic E1 – Tài khoản

**US-01 Đăng ký** (FR-ID-01, BR-AUTH-01..04)
*Là người mới, tôi muốn tạo tài khoản bằng email để lưu tiến độ học.*
- AC1: **Given** email chưa tồn tại, mật khẩu hợp lệ, đã tick ô đồng ý **When** bấm "Đăng ký" **Then** tài khoản được tạo ở trạng thái Unverified, email xác minh được gửi, tôi được đăng nhập và chuyển tới trang chọn mục tiêu.
- AC2: **Given** email đã tồn tại **When** đăng ký **Then** hiện "Email này đã được dùng" kèm link Đăng nhập / Quên mật khẩu, không tạo tài khoản.
- AC3: **Given** chưa tick ô đồng ý **Then** nút "Đăng ký" bị vô hiệu hóa.
- AC4: **Given** mật khẩu "12345678" **Then** hiện lỗi ngay tại trường: "Mật khẩu cần có cả chữ và số".
- AC5: Bản ghi đồng ý lưu `TermsVersion`, `PrivacyVersion`, `ConsentedAt`.

**US-02 Xác minh email** (FR-ID-02)
- AC1: **Given** link còn hạn **When** mở link **Then** tài khoản chuyển sang Verified, hiện "Xác minh thành công".
- AC2: **Given** link quá 24 giờ **Then** hiện "Link đã hết hạn" kèm nút "Gửi lại".
- AC3: **Given** đã gửi lại 3 lần trong 1 giờ **Then** nút "Gửi lại" bị khóa, hiện thời gian chờ.

**US-03 Đăng nhập** (FR-ID-03, BR-AUTH-06)
- AC1: Đúng thông tin thì vào được dashboard theo mục tiêu.
- AC2: Sai thông tin thì hiện "Email hoặc mật khẩu không đúng" (không nói rõ sai email hay sai mật khẩu).
- AC3: Sai 5 lần liên tiếp thì tài khoản bị khóa 15 phút, hiện thời gian còn lại. Lần thử thứ 6, dù đúng mật khẩu, vẫn bị từ chối cho tới khi hết thời gian khóa.

**US-04 Quên mật khẩu** (FR-ID-04, BR-AUTH-08)
- AC1: Nhập bất kỳ email nào cũng hiện cùng một thông báo "Nếu email tồn tại, chúng tôi đã gửi hướng dẫn" (chống dò email).
- AC2: Link chỉ dùng được 1 lần, có hiệu lực 1 giờ.
- AC3: Đặt lại thành công thì các phiên đăng nhập trên thiết bị khác bị đăng xuất.

**US-05 Chọn mục đích và mục tiêu học** (FR-ID-06, FR-CAT-07, PD-09)
*Là người học, tôi muốn cho nền tảng biết tôi học tiếng Đức để làm gì và muốn đạt trình độ nào, để thấy đúng lộ trình và thông tin phù hợp.*
- AC1: Lần đầu đăng nhập, nếu chưa có mục đích, tôi được đưa tới màn hình 2 bước. Bước 1 "Bạn học tiếng Đức để…" gồm các thẻ: Đi Ausbildung / Ausbildung điều dưỡng / Công nhận bằng nghề / Du học / Khác. Bước 2 "Mục tiêu của bạn" gồm: A1 / A2 / B1 (gợi ý B1 nếu chọn Ausbildung, kèm link Info Hub giải thích).
- AC2: Bỏ qua được cả hai bước. Khi đó mặc định là "Khác" và A1.
- AC3: Dashboard hiển thị lộ trình A1 → mục tiêu, nút "Tiếp tục học", bài kiểm tra cuối cấp, và 1–2 bài Info Hub phù hợp với mục đích đã chọn. Đổi được trong Hồ sơ.

**US-06 Xóa tài khoản** (FR-ID-08, BR-PRIV-02)
- AC1: Phải nhập lại mật khẩu và gõ "XÓA" để xác nhận.
- AC2: Sau khi xác nhận, bị đăng xuất ngay và không đăng nhập lại được.
- AC3: Tác vụ nền xóa hoặc ẩn danh dữ liệu cá nhân trong ≤ 30 ngày. Các lượt trả lời câu hỏi được giữ ở dạng ẩn danh.

---

## Epic E2 – Học bài

**US-10 Xem catalog** (FR-CAT-01, 02, BR-CAT-02)
- AC1: Guest thấy danh sách khóa Published, kèm mô tả, số bài, thời lượng ước tính.
- AC2: Khóa có một nút cha chưa Published thì không xuất hiện.
- AC3: Student đã enroll thì thấy thanh tiến độ % trên thẻ khóa.

**US-11 Học thử khi chưa đăng ký** (FR-CAT-05)
- AC1: Guest mở được bài `IsPreview`.
- AC2: Guest mở bài khác thì hiện hộp thoại "Đăng ký miễn phí để học tiếp". Đăng ký xong thì quay lại đúng bài đó.

**US-12 Học một bài** (FR-CAT-04, 06, BR-PROG-01)
*Là người học tiếng Đức A1, tôi muốn học bài theo từng khối ngắn (10–15 phút) trên điện thoại để học được lúc rảnh.*
- AC1: Các khối hiển thị đúng thứ tự Admin sắp xếp. Audio có nút phát, phát lại, chỉnh tốc độ 0,75x / 1x.
- AC2: Mở lại bài thì cuộn tới khối xem gần nhất.
- AC3: **Given** bài có bài tập **When** tôi tới khối cuối và làm bài tập đạt ≥ 60% **Then** bài chuyển Completed, hiện "Hoàn thành!" và nút "Bài tiếp theo".
- AC4: Làm bài tập đạt < 60% thì bài vẫn InProgress, hiện gợi ý "Ôn lại phần từ vựng".

**US-13 Tra từ vựng** (FR-LEX-01..04, 06)
- AC1: Gõ "Mädchen", "maedchen" hoặc "co gai" (không dấu) đều tìm ra mục từ *das Mädchen* (BR-LEX-04).
- AC2: Mục từ là danh từ thì hiển thị mạo từ có màu **kèm chữ** (der / die / das) và số nhiều, ví dụ "das Mädchen, die Mädchen" (BR-LEX-01, 03).
- AC3: Mục từ là động từ thì hiển thị bảng chia Präsens 6 ngôi, kèm nhãn "động từ tách" (ví dụ *anrufen → ich rufe an*) hoặc "bất quy tắc" nếu có (BR-LEX-02).
- AC4: Mục từ import từ Wiktionary hiển thị dòng "Nguồn: Wiktionary · CC BY-SA" kèm link tới trang gốc.
- AC5: Không có kết quả thì hiện trạng thái rỗng: "Chưa có từ này" kèm gợi ý kiểm tra chính tả.

**US-14 Xem điểm ngữ pháp** (FR-LEX-05)
- AC1: Hiển thị cấu trúc, giải thích tiếng Việt, ≥ 2 ví dụ có audio, phần "Lỗi thường gặp".
- AC2: Có link "Luyện tập ngữ pháp này" mở bộ câu hỏi gắn tag tương ứng.

---

## Epic E3 – Luyện tập

**US-20 Làm bài tập** (FR-ASM-01..04, BR-ASM-01..04)
- AC1: SingleChoice: chọn một phương án thì hiện ngay đúng hoặc sai, tô màu đáp án đúng và hiện giải thích.
- AC2: FillBlank tiếng Đức: đáp án đúng "Straße", tôi gõ "Strasse" thì được chấp nhận nếu câu bật tùy chọn ae/oe/ue/ss. Gõ "straße" thì bị tính sai nếu câu phân biệt hoa thường.
- AC3: Matching: ghép đúng 3/4 cặp thì hiện "3/4 cặp đúng" và tính 0,75 câu ở chế độ luyện tập.
- AC4: Hết bộ câu thì hiện tổng kết: số câu đúng, %, nút "Làm lại các câu sai".

---

## Epic E4 – Bài kiểm tra cuối cấp

**US-30 Bắt đầu bài kiểm tra cuối A1** (FR-ASM-05, 07, BR-EXAM-01..04, 06)
*Là người học đã học xong A1, tôi muốn làm một bài kiểm tra có tính giờ để biết mình đã sẵn sàng sang A2 chưa, và để quen với áp lực thời gian của bài thi chứng chỉ.*
- AC1: Trang giới thiệu hiển thị: "30 câu · Hören 20 phút → Lesen 25 phút · audio nghe tối đa 2 lần", kèm câu miễn trừ (BR-EXAM-10).
- AC2: **Given** email chưa xác minh **Then** nút "Bắt đầu" bị khóa, kèm link "Xác minh email".
- AC3: **When** bấm "Bắt đầu" **Then** server tạo lượt làm bài với `DeadlineAt` của phần Hören = bây giờ + 20 phút. Đồng hồ đếm ngược hiển thị theo giờ server.
- AC4: **Given** đang có lượt chưa nộp của đề này **When** bấm "Bắt đầu" **Then** hệ thống mở tiếp lượt cũ, không tạo lượt mới.

**US-31 Làm bài không mất dữ liệu** (FR-ASM-07, NFR-07)
- AC1: Mỗi lần chọn đáp án đều được lưu lên server trong ≤ 2 giây. Có chỉ báo "Đã lưu".
- AC2: **Given** mất mạng **When** chọn đáp án **Then** đáp án được giữ ở máy, hiện "Đang chờ mạng". Có mạng lại thì tự gửi.
- AC3: **Given** tôi đóng trình duyệt ở phút 10 và mở lại ở phút 15 **Then** các câu đã lưu vẫn còn, đồng hồ còn 10 phút (không dừng trong lúc tôi vắng).
- AC4: Có bảng điều hướng câu hỏi. Câu đã trả lời và câu được "đánh dấu xem lại" có màu khác nhau.

**US-32 Chuyển phần và hết giờ** (BR-EXAM-03, 04, 08)
- AC1: Hết 20 phút của phần Hören thì phần Hören tự khóa và chuyển sang phần Lesen với đồng hồ 25 phút mới.
- AC2: Tôi chủ động bấm "Sang phần Lesen" thì có hộp thoại xác nhận "Bạn sẽ không quay lại phần Hören".
- AC3: Mỗi audio phát được tối đa 2 lần (theo cấu hình của phần). Hết lượt thì nút phát bị khóa, hiện "Đã hết lượt nghe". Số lần đã nghe được lưu ở server, nên tải lại trang không được thêm lượt.
- AC4: Hết giờ phần Lesen thì bài tự nộp. Đáp án gửi tới sau hạn + 30 giây bị server từ chối.

**US-33 Xem kết quả** (FR-ASM-08, 10, BR-EXAM-05, 10, 11)
- AC1: Hiển thị tổng điểm /30 kèm %, điểm Hören /15, Lesen /15, số câu đúng theo từng dạng bài (ví dụ "Chọn mạo từ: 4/5", "Đọc hiểu quảng cáo: 2/4").
- AC2: Danh sách câu sai hiển thị đáp án của tôi, đáp án đúng, giải thích, và nút nghe lại audio.
- AC3: **Given** tổng ≥ 60% **Then** hiện "Sẵn sàng sang A2". **Given** 40–59% **Then** hiện "Cần ôn thêm" và gợi ý tối đa 3 bài học thuộc dạng bài sai nhiều nhất. **Given** < 40% **Then** hiện "Nên học lại các unit" (BR-EXAM-11). Không có dòng nào nói "tương đương điểm Goethe".
- AC4: Sửa câu hỏi sau khi tôi làm bài không làm thay đổi kết quả đã lưu (snapshot).

**US-34 Lịch sử thi** (FR-ASM-09)
- AC1: Danh sách các lượt thi: ngày, đề, điểm, thời gian làm. Có biểu đồ đường điểm theo thời gian khi có ≥ 2 lượt.

---

## Epic E5 – Tiến độ & Info Hub & Báo lỗi

**US-40 Xem tiến độ** (FR-PROG-01..03)
- AC1: Trang Tiến độ hiển thị: % từng khóa, số bài hoàn thành, tỷ lệ đúng Hören và Lesen (90 ngày gần nhất), điểm thi gần nhất và cao nhất.
- AC2: Người học mới (chưa có dữ liệu) thấy trạng thái rỗng kèm nút "Bắt đầu bài đầu tiên".

**US-41 Đọc Info Hub** (FR-INFO-01..04, BR-INFO-01..05)
- AC1: Mỗi bài hiển thị nhãn "Thông tin chính thức" hoặc "Tham khảo", danh sách nguồn kèm ngày truy cập, "Rà soát lần cuối: dd/mm/yyyy".
- AC2: Bài quá hạn rà soát hiển thị banner vàng cảnh báo.
- AC3: Cuối mỗi bài có tuyên bố: "Nền tảng không cam kết kết quả thi, tuyển dụng hay cấp visa".
- AC4: Bài render phía server, có thẻ meta title/description.

**US-42 Báo lỗi** (FR-RPT-01..03, BR-RPT-01..02)
- AC1: Bấm "Báo lỗi" trên câu hỏi thì mở form với loại lỗi và mô tả (tối đa 500 ký tự). Gửi xong hiện "Cảm ơn bạn!".
- AC2: Báo lỗi lần thứ 11 trong ngày thì hiện "Bạn đã gửi đủ số báo lỗi hôm nay".
- AC3: Có 3 người khác nhau cùng báo lỗi một câu thì câu đó chuyển NeedsUpdate và xuất hiện đầu hàng đợi của Admin.

---

## Epic E6 – Quản trị nội dung (Admin)

**US-50 Quản lý nguồn & giấy phép** (FR-CMS-05, BR-CNT-01, 03)
- AC1: Tạo Source gồm tên, nhà cung cấp, URL, License, ngày xác minh license, ghi chú.
- AC2: License có các cờ: AllowsCommercial, RequiresAttribution, ShareAlike, AllowsModification.
- AC3: Chọn một License có AllowsCommercial = false cho Source thì hệ thống cảnh báo "Nguồn này sẽ không import được".

**US-51 Import từ vựng CSV** (FR-CMS-04, BR-CNT-10, 12)
- AC1: Tải được file template mẫu (CSV có header) ngay trên màn hình import.
- AC2: Dry-run với file 500 dòng (480 hợp lệ, 15 lỗi, 5 trùng) hiển thị báo cáo đúng con số, kèm số dòng và lý do của từng lỗi.
- AC3: Xác nhận thì tạo 480 mục ở trạng thái Draft cùng `ImportBatchId`. 5 dòng trùng nằm trong danh sách "Cần quyết định".
- AC4: Hủy batch thì xóa các mục chưa từng Published của batch.

**US-52 Soạn bài học** (FR-CMS-01, 08)
- AC1: Thêm, sửa, xóa, kéo-thả thứ tự các khối.
- AC2: Khối từ vựng chọn mục từ có sẵn bằng ô tìm kiếm, không phải gõ lại.
- AC3: Nút "Xem trước" hiển thị bài đúng như người học sẽ thấy (cả trên khung mobile).

**US-53 Tạo câu hỏi bằng AI** (FR-CMS-06, BR-CNT-07, 08)
- AC1: Chọn mẫu prompt, chủ đề, cấp độ, số lượng (tối đa 20/lượt), bấm "Tạo" thì nhận về các câu Draft có `Origin = AIDraft`.
- AC2: Mỗi câu có kết quả kiểm tra tự động và kết quả AI kiểm định (OK / cảnh báo kèm lý do).
- AC3: Chi phí tháng đã vượt trần thì nút "Tạo" bị khóa, hiện số tiền đã dùng / trần.

**US-54 Duyệt & publish** (FR-CMS-03, BR-CNT-05, 06)
- AC1: Hàng đợi duyệt lọc được theo loại, nguồn, Origin, có hoặc không có cờ cảnh báo.
- AC2: Bấm "Publish" thì hiện checklist. Phải tick đủ mục mới publish được.
- AC3: Mục thiếu Source/License, hoặc câu hỏi chưa qua kiểm tra tự động, thì nút Publish bị khóa kèm lý do.

**US-55 Quản lý blueprint & đề** (FR-ASM-05, 06, BR-EXAM-01, 02, 09)
- AC1: Có sẵn blueprint "Kiểm tra cuối A1" (seed): Hören 15 câu / 20 phút / audio tối đa 2 lần, Lesen 15 câu / 25 phút, mỗi câu 1 điểm.
- AC2: Tạo đề Fixed: kéo câu hỏi vào từng phần. Hệ thống kiểm tra đủ số câu và đúng phân bố dạng bài trước khi cho publish.
- AC3: Tạo đề Generated khi ngân hàng thiếu câu thì báo "Thiếu N câu dạng [X] phần Hören".

**US-56 Quản lý Info Hub** (BR-INFO-01..04)
- AC1: Không publish được bài thiếu nguồn chính thức.
- AC2: Bấm "Đã rà soát" thì cập nhật `LastReviewedAt` và tính lại `ReviewDueAt`.
- AC3: Dashboard có thẻ "N bài quá hạn rà soát".

**US-57 Xử lý báo lỗi** (FR-RPT-03)
- AC1: Từ một báo lỗi, mở thẳng được nội dung để sửa.
- AC2: Resolve kèm ghi chú thì mọi báo lỗi đang mở của cùng nội dung cũng được đóng theo, và nội dung về lại Published nếu đang NeedsUpdate.

**US-58 Dashboard** (FR-ADM-01)
- AC1: Hiển thị đúng 6 chỉ số của FR-ADM-01. Số liệu cập nhật trong ≤ 5 phút (cache).
