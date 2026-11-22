# 06 – Bổ sung Phase 2 theo CR-002 (phương án 2)

Đã chốt ngày 2026-11-22. Phân tích: [docs/research/01-competitor-analysis.md](../research/01-competitor-analysis.md). Thiết kế module: [08-engagement.md](../phase-4/modules/08-engagement.md).

## 1. Actor thay đổi
**Guest** nay được: xem ngữ pháp công khai, **chơi mini-game và làm bộ luyện tập công khai**, xem bài học thử, ôn thử vài thẻ. Tiến độ của Guest lưu **trên máy** (`localStorage`) và được gộp vào tài khoản khi đăng ký (UC-34).

## 2. Use case mới
| ID | Tên | Actor | Ưu tiên |
|---|---|---|---|
| UC-34 | Gộp dữ liệu khách vào tài khoản | Student (vừa đăng ký/đăng nhập) | Must |
| UC-35 | Ôn thẻ từ vựng theo lịch (SRS) | Student | Must |
| UC-36 | Xem "Bàn học" (streak, nhiệm vụ, ôn hôm nay, lối vào) | Student, Guest | Must |
| UC-37 | Hoàn thành nhiệm vụ ngày, tích XP | Student | Must |
| UC-38 | Chơi mini-game "Nối từ nhanh" | Guest, Student | Must |
| UC-39 | Cài ứng dụng lên điện thoại (PWA) | Guest, Student | Should |
| UC-40 | Đọc ngữ pháp công khai và luyện tập thử | Guest, Student | Must |

## 3. Yêu cầu chức năng mới
| ID | Yêu cầu | Ưu tiên | UC |
|---|---|---|---|
| FR-ENG-01 | Thẻ SRS gắn với mục từ Lexicon; hai kiểu hỏi: nhận biết nghĩa, chọn mạo từ (danh từ) | M | UC-35 |
| FR-ENG-02 | Lịch ôn tính bằng SM-2 giản lược; 4 mức Again/Hard/Good/Easy; tối đa 20 thẻ mới/ngày, 20 thẻ/phiên | M | UC-35 |
| FR-ENG-03 | Tự thêm thẻ khi mở bài có từ vựng; thêm/tạm dừng thủ công | M | UC-35 |
| FR-ENG-04 | Streak theo ngày giờ Việt Nam, chỉ tính hoạt động có ý nghĩa | M | UC-36 |
| FR-ENG-05 | 3 nhiệm vụ mỗi ngày, XP, cấp độ | M | UC-37 |
| FR-ENG-06 | "Bàn học": streak, nhiệm vụ, "Tiếp tục học", "Ôn hôm nay", lưới lối vào. Có cả cho Guest | M | UC-36 |
| FR-ENG-07 | Mini-game "Nối từ nhanh" 60 giây, chơi được khi chưa đăng nhập | M | UC-38 |
| FR-ENG-08 | Gộp dữ liệu khách (idempotent, có giới hạn), không mất dữ liệu của tài khoản | M | UC-34 |
| FR-ENG-09 | PWA: manifest, biểu tượng, vỏ ứng dụng cache được, gợi ý cài đặt | S | UC-39 |
| FR-LEX-08 | Điểm ngữ pháp công khai render phía server, có SEO (title, description, JSON-LD) | M | UC-40 |
| FR-ASM-11 | Bộ luyện tập công khai (`is_public_practice`) chấm được ở client, tách khỏi ngân hàng đề kiểm tra | M | UC-40 |
| FR-ASM-12 | Component SingleChoice và Matching dùng chung giữa luyện tập, kiểm tra, mini-game | M | UC-38 |

## 4. Yêu cầu phi chức năng mới
| ID | Yêu cầu | Cách đo |
|---|---|---|
| NFR-19 | Lần mở đầu tiên của trang chủ khách cho chơi được mini-game trong **< 3 giây** trên Android tầm trung, 4G | Lighthouse mobile |
| NFR-20 | PWA đạt tiêu chí "có thể cài đặt" và mở lại khi mất mạng (vỏ ứng dụng) | Lighthouse PWA |
| NFR-21 | Phiên ôn thẻ phản hồi **< 200 ms** mỗi lần trả lời (p95) | Log thời gian |
| NFR-22 | Trang ngữ pháp công khai: Lighthouse SEO ≥ 90 | Lighthouse |

## 5. Business rules mới
**BR-ENG-01..09** được định nghĩa tại [08-engagement.md §8](../phase-4/modules/08-engagement.md). Tóm tắt: ngày học theo giờ VN; streak cần hoạt động có ý nghĩa; 3 nhiệm vụ/ngày; XP chỉ từ sự kiện server (game tối đa 5 XP/ngày); tối đa 20 thẻ mới/ngày; giới hạn khi gộp dữ liệu khách (200 thẻ, 500 sự kiện, 50 XP); mỗi lần trả lời có `clientEventId` (idempotent); thẻ của từ đã Archive bị loại; xóa dữ liệu khi xóa tài khoản.

**Sửa các rule cũ:**
| Rule cũ | Thay đổi |
|---|---|
| BR-CAT-05 / US-11 (Guest chỉ học bài preview) | Guest còn được xem **ngữ pháp công khai**, làm **bộ luyện tập công khai**, chơi **mini-game** |
| BR-INFO-01 (mọi bài Info Hub cần nguồn chính thức) | Chuyên mục `StudyTips` (1.1) không bắt buộc; bài nhắc quy định thi vẫn phải dẫn nguồn |
| BR-PRIV-01 (thu thập tối thiểu) | **Không đổi.** Dữ liệu khách chỉ nằm trên máy, không gửi lên server cho tới khi đăng ký và chọn gộp |

## 6. User stories mới
**US-60 Ôn thẻ** (FR-ENG-01..03, BR-ENG-05, 07, 08)
*Là người học, tôi muốn ôn từ vựng đúng lúc sắp quên để nhớ lâu mà chỉ mất vài phút mỗi ngày.*
- AC1: Trang "Ôn hôm nay" hiện số thẻ đến hạn. Bấm vào bắt đầu phiên tối đa 20 thẻ, thẻ quên (Lapsed) được ưu tiên.
- AC2: Với một thẻ, lật mặt sau rồi chọn Again/Hard/Good/Easy. Mỗi nút hiển thị gợi ý "ôn lại sau X" (ví dụ "1 ngày", "3 ngày").
- AC3: Với danh từ, có kiểu hỏi chọn der/die/das. Chọn đúng tính là Good, sai là Again.
- AC4: Mất mạng giữa phiên: đáp án được giữ lại và gửi sau; gửi trùng không bị tính hai lần.
- AC5: Cuối phiên thấy: số thẻ, tỷ lệ nhớ, XP, tiến độ nhiệm vụ.

**US-61 Streak và nhiệm vụ** (FR-ENG-04, 05, BR-ENG-01..04)
- AC1: "Bàn học" hiển thị streak và 3 nhiệm vụ kèm tiến độ (ví dụ 7/10).
- AC2: Ôn 4 thẻ thì streak chưa tăng; ôn thẻ thứ 5 thì tăng 1 và nhiệm vụ "Ôn 10 thẻ" hiển thị 5/10.
- AC3: Hoàn thành cả 3 nhiệm vụ nhận 15 XP một lần trong ngày.
- AC4: Bỏ một ngày: lần mở app tiếp theo thấy chuỗi về 0 cùng thông điệp nhẹ nhàng, "chuỗi dài nhất" vẫn được giữ.
- AC5: Hoạt động lúc 23:59 và 00:01 (giờ VN) tính cho hai ngày khác nhau.

**US-62 Khách vào học ngay và gộp dữ liệu** (FR-ENG-06..08, FR-LEX-08, FR-ASM-11, BR-ENG-06)
- AC1: Khách mở trang chủ thấy khung "Chơi thử ngay" và chơi được mini-game không cần đăng ký.
- AC2: Khách mở một điểm ngữ pháp công khai, đọc và làm bài luyện tập thử, thấy đúng/sai ngay.
- AC3: Khách có chuỗi và vài thẻ tạm lưu trên máy. Sau khi đăng ký, các dữ liệu đó xuất hiện trong tài khoản, dữ liệu trên máy được xóa.
- AC4: Gộp cùng một gói hai lần không nhân đôi. Tài khoản đã có thẻ trùng thì giữ lịch của tài khoản.
- AC5: Gói dữ liệu vượt giới hạn bị từ chối, không ghi gì.

**US-63 Mini-game Nối từ nhanh** (FR-ENG-07, BR-ENG-04)
- AC1: Hiển thị 4 từ Đức và 4 nghĩa, chạm để nối cặp, có đồng hồ 60 giây và điểm.
- AC2: Chạm sai thì cặp lắc nhẹ và không nối. Nối đúng thì cặp biến mất kèm âm thanh (có thể tắt).
- AC3: Người đã đăng nhập chơi 10 ván: XP từ game không quá 5 trong ngày.

**US-64 Cài ứng dụng** (FR-ENG-09)
- AC1: Trên Chrome Android thấy gợi ý "Thêm vào màn hình chính". Sau khi cài, mở từ biểu tượng và chạy toàn màn hình.
- AC2: Mất mạng khi mở app: hiện vỏ ứng dụng và thông báo "Cần mạng để học", không trắng trang.

## 7. Truy vết
UC-34..40 → FR-ENG-01..09, FR-LEX-08, FR-ASM-11..12 → BR-ENG-01..09 → US-60..64 → test case EN-T01..22, LX-T16..17, AS-T38, ID-T21..22, PR-T14, CA-T15.

---

# Bổ sung theo CR-003 (phương án 2, đã chốt 2026-11-22)

Nguồn: [CR-003](../change-requests/CR-003-personalization-from-brottin.md), [phân tích sâu Brottin](../research/02-brottin-deep-dive.md).

## 8. Use case mới
| ID | Tên | Actor | Ưu tiên |
|---|---|---|---|
| UC-41 | Chọn chế độ học từ (Lặp lại, Flashcard, Trắc nghiệm, Trắc nghiệm đảo, Đoán, Artikel, Trộn) | Student | Must |
| UC-42 | Xem thẻ từ giàu thông tin (đồng nghĩa, cụm, họ từ, cách dùng) | Guest, Student | Must |
| UC-43 | Tùy chỉnh thiết lập học (XP/ngày, thẻ mới/ngày, độ nghiêm khi chấm, thời lượng) | Student | Must |
| UC-44 | Lưu từ và quản lý "Từ của tôi" | Student | Must |

## 9. Yêu cầu chức năng mới
| ID | Yêu cầu | Ưu tiên | UC |
|---|---|---|---|
| FR-ENG-10 | 6 chế độ học từ + "Trộn", cùng cập nhật một thẻ SRS | M | UC-41 |
| FR-ENG-11 | Thiết lập cá nhân hóa với giá trị mặc định và giới hạn | M | UC-43 |
| FR-ENG-12 | Lưu từ, bỏ lưu từ; tự tạo thẻ SRS khi lưu | M | UC-44 |
| FR-ENG-13 | "Từ của tôi": thêm, sửa, xóa từ tự nhập (tối đa 500), riêng tư | M | UC-44 |
| FR-ENG-14 | Phím tắt trong phiên học (Tab, Space, Enter, 1–4) | S | UC-41 |
| FR-ENG-15 | Ghi nhận thời gian học chủ động (`active_seconds`), chưa có UI | S | UC-41 |
| FR-LEX-09 | Thẻ từ có `synonyms`, `collocations`, `word_family`, `usage_notes_vi`; các trường không bắt buộc | M | UC-42 |
| FR-LEX-10 | Thẻ từ hiển thị nguồn, giấy phép và nhãn "Có hỗ trợ AI" | M | UC-42 |
| FR-ASM-13 | Độ nghiêm khi chấm chỉ áp dụng ở luyện tập, không áp dụng ở bài kiểm tra | M | UC-43 |
| FR-CMS-11 | Trang công khai "Cách chúng tôi dùng AI" | S | — |

## 10. User stories mới
**US-65 Chọn chế độ học từ** (FR-ENG-10, BR-ENG-12)
*Là người học, tôi muốn đổi cách ôn từ (gõ, chọn, lật thẻ) để không thấy nhàm mà vẫn ôn đúng lịch.*
- AC1: Trong phiên ôn có thanh chọn chế độ: Lặp lại, Flashcard, Trắc nghiệm, Trắc nghiệm đảo, Đoán, Artikel, Trộn. Mặc định là chế độ tôi đã chọn trong Cài đặt.
- AC2: Chế độ Trắc nghiệm có 4 đáp án cùng chủ đề. Trả lời đúng thì lịch ôn của thẻ tăng như "Tốt".
- AC3: Chế độ Đoán: nhập từ Đức, bấm Tab để gợi ý một chữ cái. Dùng gợi ý rồi đúng vẫn được, nhưng lịch ôn ngắn hơn.
- AC4: Chế độ Artikel chỉ ra danh từ, chọn der/die/das; chọn sai hiển thị quy tắc đuôi từ nếu có.
- AC5: Đổi chế độ giữa chừng không làm mất tiến độ phiên.

**US-66 Thẻ từ giàu thông tin** (FR-LEX-09, 10)
- AC1: Thẻ hiển thị mạo từ có màu kèm chữ, IPA, audio, nghĩa, ví dụ có audio.
- AC2: Có các chip Đồng nghĩa, Cụm đi kèm, Họ từ, Cách dùng. Chip nào không có dữ liệu thì không hiển thị.
- AC3: Chạm vào một từ trong "Họ từ" mà từ đó có trong hệ thống thì mở thẻ của từ đó.
- AC4: Cuối thẻ có dòng "Nguồn: … · giấy phép …" và, nếu nội dung có hỗ trợ AI, nhãn "Có hỗ trợ AI".
- AC5: Có nút "Báo lỗi" và "Lưu từ".

**US-67 Thiết lập cá nhân hóa** (FR-ENG-11, FR-ASM-13)
- AC1: Trong Cài đặt đổi được mục tiêu XP/ngày, thẻ mới/ngày, thời lượng học/ngày, chế độ học mặc định và độ nghiêm khi chấm. Giá trị ngoài giới hạn bị chặn kèm thông báo.
- AC2: Độ nghiêm "Dễ": gõ "strasse" cho "Straße" được tính đúng, sai một ký tự ở từ dài thì hiện "Gần đúng" và vẫn cho tiếp.
- AC3: Trong bài kiểm tra có giờ, độ nghiêm khi chấm **không** thay đổi cách chấm.
- AC4: Bỏ qua bước hỏi thời lượng ở onboarding thì dùng mặc định 10 phút.

**US-68 Lưu từ và Từ của tôi** (FR-ENG-12, 13, BR-ENG-13)
- AC1: Bấm "Lưu từ" trên một thẻ thì từ vào danh sách của tôi và có thẻ SRS. Bỏ lưu không xóa thẻ SRS.
- AC2: Trong "Từ của tôi" tôi thêm một từ tự nhập (từ Đức, nghĩa, ví dụ tùy chọn). Từ đó xuất hiện trong phiên ôn của tôi.
- AC3: Thêm quá 500 từ thì báo giới hạn.
- AC4: Người khác không thể thấy từ của tôi. Xóa tài khoản thì mọi từ này bị xóa.

## 11. Truy vết bổ sung
UC-41..44 → FR-ENG-10..15, FR-LEX-09..10, FR-ASM-13, FR-CMS-11 → BR-ENG-10..13, BR-LEX-05 → US-65..68 → test EN-T23..35, LX-T18..20, AS-T39..41, CO-T21..22, ID-T23.

---

# Bổ sung theo AD-09 (xác thực JWT hybrid, đã chốt 2026-11-22)

Thiết kế: [01-identity.md §14](../phase-4/modules/01-identity.md).

## 12. Use case và yêu cầu chức năng mới
| ID | Tên / Yêu cầu | Ưu tiên |
|---|---|---|
| UC-45 | Làm mới phiên tự động (người dùng không nhận ra) | Must |
| UC-46 | Xem và thu hồi các thiết bị đăng nhập | Must |
| UC-47 | Đăng xuất mọi thiết bị | Must |
| FR-ID-10 | Phiên gồm access token ngắn hạn và refresh token xoay vòng, truyền bằng cookie HttpOnly | M |
| FR-ID-11 | Trang "Thiết bị đăng nhập": danh sách, thu hồi từng thiết bị | M |
| FR-ID-12 | "Đăng xuất mọi thiết bị" | M |
| FR-ID-13 | Phiên Admin ngắn (tối đa 12 giờ) và yêu cầu đăng nhập gần đây cho thao tác Admin | M |
| FR-ID-14 | Nhật ký sự kiện bảo mật, IP rút gọn, giữ 180 ngày | M |
| NFR-23 | Thu hồi phiên có hiệu lực tối đa 60 giây (học viên) và tức thì (Admin, hành động nhạy cảm) | Kiểm tra bằng test ID-T34, T37 |

## 13. User stories mới
**US-69 Phiên đăng nhập an toàn** (FR-ID-10, BR-AUTH-10..12)
- AC1: Đăng nhập xong, tôi không phải nhập lại trong lúc dùng bình thường dù access token chỉ sống 10 phút (phiên tự làm mới).
- AC2: Nếu tôi không chọn "Ghi nhớ", đóng trình duyệt rồi mở lại thì phải đăng nhập lại.
- AC3: Dù dùng đều đặn, sau hạn tuyệt đối (7 ngày, hoặc 90 ngày nếu chọn "Ghi nhớ") tôi phải đăng nhập lại.
- AC4: Nếu ai đó dùng lại một refresh token cũ của tôi, mọi phiên của chuỗi đó bị thu hồi và tôi cần đăng nhập lại.

**US-70 Thiết bị đăng nhập** (FR-ID-11, 12, BR-AUTH-14, 16)
- AC1: Trong Hồ sơ tôi thấy danh sách thiết bị: tên thiết bị, lần dùng gần nhất, vị trí gần đúng (IP rút gọn), thiết bị hiện tại được đánh dấu.
- AC2: Bấm "Đăng xuất thiết bị này" thì thiết bị đó không dùng được nữa trong tối đa 60 giây.
- AC3: Bấm "Đăng xuất mọi thiết bị" thì mọi thiết bị đều phải đăng nhập lại.
- AC4: Đổi mật khẩu cũng đăng xuất các thiết bị khác.

**US-71 Phiên Admin** (FR-ID-13, BR-AUTH-13, 15)
- AC1: Admin không có tùy chọn "Ghi nhớ" và bị đăng xuất sau tối đa 12 giờ.
- AC2: Khi một Admin bị khóa, mọi thao tác Admin của người đó bị từ chối ngay lập tức.
- AC3: Thao tác Admin khi lần nhập mật khẩu gần nhất đã quá 12 giờ yêu cầu đăng nhập lại.

Test tương ứng: ID-T24..ID-T45 (01-identity §14.14).
