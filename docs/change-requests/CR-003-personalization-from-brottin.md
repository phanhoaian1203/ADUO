# CR-003 – Kế thừa và cải tiến cá nhân hóa học từ vựng (từ phân tích sâu Brottin)

| Mục | Nội dung |
|---|---|
| Mã | CR-003 |
| Ngày | 2026-11-22 |
| Người yêu cầu | Chủ dự án (ấn tượng với trang Từ vựng của Brottin, muốn kế thừa, cải tiến, cá nhân hóa, và hỏi có dùng được tài nguyên từ vựng của họ không) |
| Trạng thái | **Confirmed (phương án 2)** ngày 2026-11-22: toàn bộ tầng A vào MVP. Q-CR3-02 (xin phép Brottin) chưa trả lời, là việc của chủ dự án, không chặn tiến độ. Q-CR3-03 chỉ cần quyết ở V2 |
| Căn cứ | [02-brottin-deep-dive.md](../research/02-brottin-deep-dive.md) |
| Quan hệ | **Bổ sung** cho CR-002 (đã chốt phương án 2). Không đảo ngược quyết định nào của CR-002 |

## 1. Quyết định rút ra ngay (không cần chờ)
| # | Quyết định | Lý do |
|---|---|---|
| D1 | **Không dùng dữ liệu từ vựng của Brottin** (file CSV/Excel xuất ra, hay bất kỳ nội dung nào của họ) trong ADUO: không import, không đưa vào repo, không dùng làm dữ liệu huấn luyện/mẫu cho AI | Điều khoản của họ (Điều 6) cấm sao chép hàng loạt và phát hành lại. File không có nguồn/giấy phép nên vi phạm BR-CNT-01. Xếp **Group D** |
| D2 | **Được phép học ý tưởng, cấu trúc, trải nghiệm** và tự xây nội dung riêng | Ý tưởng và cách tổ chức không bị bảo hộ như nội dung cụ thể |
| D3 | File CSV đã tải về chỉ để **bạn tự học hoặc tham khảo**. Không đặt trong thư mục dự án | Tránh vô tình commit lên repo public |
| D4 | Nội dung do AI hỗ trợ **phải có nhãn hiển thị** cho người học, và bản dịch AI **luôn qua duyệt** | Bài học từ Brottin: bản dịch AI có dấu hiệu chưa rà |

## 2. Những gì đáng kế thừa từ Brottin và cách ADUO cải tiến

| Brottin làm | ADUO kế thừa | ADUO cải tiến |
|---|---|---|
| 5 chế độ học từ (Đoán, Flashcard, Trắc nghiệm, Trắc nghiệm đảo, Lặp lại) | Cùng bộ chế độ, người học tự chọn hoặc "Trộn" | Thêm chế độ **Artikel** riêng; mọi chế độ dùng **cùng một lịch SRS**, nên không phải ôn riêng từng chế độ |
| Thẻ giàu thông tin (IPA, định nghĩa, ví dụ có audio, đồng nghĩa, cụm đi kèm, họ từ, cách dùng) | Cùng các mục | **Ghi công nguồn** trên thẻ, **bắt buộc đủ giống và số nhiều** (BR-LEX-01), **bảng chia động từ**; dữ liệu từ Wiktionary |
| Bộ từ → Phần (~250) → Chủ đề con (50 thẻ) | Cấu trúc 3 tầng, học từng phần nhỏ | Chủ đề con khoảng **30–40 từ** cho người học ít thời gian; tiến độ theo từng tầng |
| Bộ từ theo 17 nghề | Ý tưởng bộ từ theo nghề | Bắt đầu **3 nghề** (điều dưỡng, nhà hàng–khách sạn, cơ điện tử), xây bằng **nguồn có giấy phép** (deep dive §6), nối với **Info Hub** của chính nghề đó (thông tin Ausbildung có nguồn chính thức) |
| Cài đặt: mục tiêu XP, thẻ mới/ngày, ngôn ngữ dịch, độ nghiêm khi chấm | 4 thiết lập này | Thêm **thời lượng học mỗi ngày** (5/10/15 phút) ở onboarding. "Độ nghiêm" gắn với quy tắc chấm đã có (ae/oe/ue/ss, hoa/thường) |
| "Lưu từ", "Thêm vào bộ của tôi", "Từ của tôi" | Bộ từ cá nhân | MVP không AI (người dùng tự nhập nghĩa); AI tạo ví dụ để V2, **chỉ gửi một từ + cấp độ** (xem Q-CR3-03) |
| Hội thoại dạng chat điền chỗ trống, "Túi từ" | Cơ chế này | Tái dùng khối `Dialogue` + `FillBlank` đã có. Cảnh theo **bối cảnh Ausbildung** (phỏng vấn, giao ca) ở V2 |
| Dịch Việt → Đức che chữ, mở dần khi gõ đúng | Cơ chế che chữ | Câu lấy từ **Tatoeba (CC BY/CC0)** đã kiểm duyệt, không để AI tự sinh toàn bộ |
| Bảng thống kê (thời gian, heatmap, mục tiêu theo kỹ năng) | Cùng loại số liệu | Chỉ ghi **thời gian học chủ động** (bỏ qua lúc để màn hình treo), nói rõ dữ liệu nào được thu thập |
| "Bảo vệ chuỗi", "sửa chuỗi" bằng ngọc | Ý tưởng tha thứ khi lỡ ngày | **Miễn phí, không cần ngọc**: mỗi tuần 1 lần tạm nghỉ tự động. Tránh kinh tế ảo ở giai đoạn đầu |
| Nhật ký cập nhật của admin | Trang "Có gì mới" | Cho thấy dự án sống, và ghi nguồn các thay đổi nội dung |
| Nhãn "KI-GENERIERT", điều khoản về AI | Nhãn minh bạch | Nhãn theo trường `Origin` đã có, kèm trang giải thích cách chúng ta dùng AI |

## 3. Quyết định đề xuất theo tầng

### Tầng A: vào **MVP** (≈ +12–14 ngày công)
| # | Tính năng | Module | Công sức |
|---|---|---|---|
| A1 | **Chế độ học từ**: Lặp lại (SRS), Flashcard, Trắc nghiệm hai chiều, **Đoán** (gõ từ, gợi ý chữ cái), **Artikel**, **Trộn** | Engagement + component của Assessment | 3–4 |
| A2 | **Thẻ từ giàu thông tin**: thêm `synonyms`, `collocations`, `word_family`, `usage_notes_vi`; UI 5 mục mở rộng; mapper lấy từ Wiktionary (🟡 phải kiểm tra dữ liệu thật trước) | Lexicon | 3–4 |
| A3 | **Cài đặt cá nhân hóa**: mục tiêu XP/ngày, thẻ mới/ngày, **độ nghiêm khi chấm** (Dễ/Thường/Khó), thời lượng học/ngày | Engagement (+ Identity onboarding) | 2 |
| A4 | **"Lưu từ" và "Từ của tôi"**: lưu từ vào danh sách riêng; thêm từ tự nhập (không AI) | Engagement | 2 |
| A5 | **Phím tắt** (Tab gợi ý, Space bỏ qua, Enter kiểm tra) và trạng thái rỗng thân thiện | Frontend | 0,5 |
| A6 | **Nhãn nội dung AI** theo `Origin` trên thẻ, ví dụ, bài viết | ContentOps + UI | 1 |
| A7 | Cột `active_seconds` trong `daily_activity` (chuẩn bị cho thống kê, không làm UI) | Engagement | 0,5 |

### Tầng B: **Release 1.1**
| # | Tính năng | Ghi chú |
|---|---|---|
| B1 | **Bộ từ theo nghề** Bộ → Phần → Chủ đề con, 3 nghề đầu, kèm "Luyện Artikel" theo bộ | Xây bằng con đường A của deep dive §6. ⚠️ Thuật ngữ chuyên ngành khó duyệt khi làm một mình. Chỉ nhập từ có nguồn tham chiếu |
| B2 | **Trang Thống kê**: thời gian hôm nay/tuần, ngày hoạt động, chuỗi + kỷ lục, từ đã học, phân bổ kỹ năng, biểu đồ 14 ngày, **heatmap lịch**, mục tiêu theo kỹ năng | Engagement + Progress |
| B3 | **Hội thoại chat điền chỗ trống** ("Szene"): 10 cảnh A1, "Túi từ", bản ghi đầy đủ | Catalog + Assessment |
| B4 | **Dịch Việt → Đức che chữ mở dần** với câu Tatoeba | Assessment |
| B5 | **Tạm nghỉ chuỗi** tự động 1 lần/tuần | Engagement (BR-ENG-10 mới) |
| B6 | **"Có gì mới"** (nhật ký cập nhật) và thông báo hệ thống | InfoHub nhẹ |

### Tầng C: **V2**
| # | Tính năng | Ghi chú |
|---|---|---|
| C1 | **"Từ của tôi" có AI**: nhập từ → AI tạo ví dụ theo CEFR, đồng nghĩa, cách dùng | Cần chỉnh BR-PRIV-03 (Q-CR3-03), nhãn AI, trần chi phí |
| C2 | Chép chính tả từng câu bằng audio TTS của ta, phím tắt nghe lại | Đã có trong CR-002 tầng C |
| C3 | Đọc song ngữ có **từ chú giải sẵn**; "Đọc hiểu Ausbildung" do AI soạn + biên tập, có nhãn | ⚖️ nguồn văn bản gốc phải xác minh |
| C4 | Luyện nói đọc to + ghi âm (chưa AI chấm) | AI chấm phát âm để V3 |
| C5 | Luyện viết có AI nhận xét (đề theo tình huống, **không gắn nhãn Goethe/telc**) | |
| C6 | Bảng xếp hạng theo thời gian học (tự nguyện, biệt danh), huy hiệu, "ngọc" chỉ dùng để sửa chuỗi | Không bán mỹ phẩm |

### Tầng D: **V3 hoặc không làm**
- Quán café 3D nhiều người chơi (WebGL, realtime, kiểm duyệt: quá nặng).
- Cửa hàng hình nền/khung avatar (nặng thiết kế, rủi ro sở hữu trí tuệ).
- Thư viện video/phụ đề của bên thứ ba, đề và nhãn của kỳ thi chưa có giấy phép.
- **Nhập dữ liệu của Brottin** (xem D1).
- Đề thi THPT Quốc gia môn tiếng Đức: **chờ xác minh điều kiện tái sử dụng** (⚖️). Nếu hợp pháp, có thể hợp với nhóm học sinh THPT, nhưng chưa vào kế hoạch.

## 4. Tác động lên thiết kế
| Hạng mục | Thay đổi |
|---|---|
| `lexicon.vocabulary_entries` | Thêm cột jsonb `synonyms`, `collocations`, `word_family` và `usage_notes_vi` text (A2) |
| `engagement.user_preferences` (bảng mới) | `user_id`, `daily_xp_goal`, `new_cards_per_day`, `grading_strictness` (`Easy`/`Normal`/`Hard`), `daily_minutes`, `meaning_language` |
| `engagement.saved_entries` và `engagement.user_words` (bảng mới) | Từ lưu và từ tự nhập của người học. **Chuyển** `saved_entries` từ Lexicon sang Engagement (dữ liệu của người học thuộc về Engagement, không thuộc kho tri thức chung) |
| `engagement.daily_activity` | Thêm `active_seconds` int |
| Quy tắc chấm (BR-ASM-02) | `grading_strictness` điều chỉnh: **Dễ** chấp nhận ae/oe/ue/ss và bỏ qua hoa/thường ở câu cho phép; **Thường** theo cấu hình từng câu; **Khó** đúng từng ký tự. Chỉ áp dụng ở **chế độ luyện tập**, **không** áp dụng cho bài kiểm tra có giờ |
| Business rule mới | **BR-ENG-10** (tạm nghỉ chuỗi 1 lần/tuần), **BR-ENG-11** (thiết lập cá nhân hóa có giới hạn hợp lý: thẻ mới/ngày 5–50, XP/ngày 20–500), **BR-LEX-05** (thẻ từ hiển thị nhãn nguồn và nhãn AI theo `Origin`) |
| API | `GET/PUT /me/preferences`; `POST/DELETE /me/saved-entries`; `POST/PATCH/DELETE /me/user-words` |

## 5. Rủi ro
| Rủi ro | Mức | Giảm thiểu |
|---|---|---|
| **Phình phạm vi liên tiếp**: MVP đã tăng từ 56–78 (ban đầu) lên 76–105 (CR-002), nay lên khoảng **88–119 ngày công** nếu chọn đủ tầng A | Cao | Đặt **điểm kiểm tra bắt buộc**: sau khi xong Identity + ContentOps 2a + Lexicon, đo tốc độ thực rồi quyết định giữ hay hoãn tầng A của CR-002 và CR-003 |
| Dữ liệu Wiktionary không đủ trường (đồng nghĩa, cụm, họ từ) cho A2 | Trung bình | Bước khám phá dữ liệu của Lexicon sẽ trả lời. Nếu thiếu, hoãn A2 hoặc để AI viết nháp rồi duyệt |
| Bộ từ nghề sai thuật ngữ chuyên ngành, người làm một mình không đủ chuyên môn duyệt | Cao | Chỉ nhập từ có nguồn tham chiếu. Nhãn "chưa được chuyên gia duyệt". Mời người trong ngành (điều dưỡng, đầu bếp…) góp ý khi có người dùng |
| Cá nhân hóa quá nhiều làm người học rối | Trung bình | Mặc định hợp lý, thiết lập nằm sau "Cài đặt", onboarding chỉ hỏi 2 câu |
| Nhầm lẫn "tự học từ file Brottin" với "dùng dữ liệu của Brottin" | Thấp | D1, D3 |

## 6. Cần bạn xác nhận
| # | Câu hỏi | Đề xuất của tôi |
|---|---|---|
| **Q-CR3-01** | **Phạm vi đưa vào MVP** | **Phương án 2 (khuyến nghị): toàn bộ tầng A (+12–14 ngày).** Hai phương án còn lại: **Phương án 1**: chỉ A1, A2, A3 (+8–10 ngày), A4–A7 để 1.1. **Phương án 3**: toàn bộ để 1.1, giữ MVP như sau CR-002. Dù chọn phương án nào, tôi đề xuất giữ **điểm kiểm tra** ở §5 |
| **Q-CR3-02** | Có thử **xin phép Brottin** (mẫu tin nhắn ở deep dive §7) không? | **Có, nhưng không phụ thuộc vào câu trả lời của họ.** Tôi vẫn làm theo con đường A song song. Bạn tự gửi, tôi không gửi thay |
| **Q-CR3-03** | Khi làm "Từ của tôi có AI" (V2): cho phép gửi **một từ + cấp độ CEFR** lên API AI (không gửi dữ liệu cá nhân nào) | **Đồng ý, nhưng chỉ ở V2.** Cần sửa BR-PRIV-03 thành "không gửi dữ liệu **cá nhân** của người học; được gửi từ vựng và cấp độ". Chưa cần quyết ngay |

**Kết quả:** chủ dự án chọn **phương án 2**. Tài liệu module, database, API, Phase 2 đã được cập nhật theo tầng A. **Điểm kiểm tra §5 vẫn giữ nguyên.**

## 7. Ảnh hưởng lên lịch
| | Ban đầu | Sau CR-002 | Sau CR-003 (phương án 2) |
|---|---|---|---|
| Code MVP (ngày công) | 56–78 | 76–105 | **≈ 88–119** |
| Ghi chú | | | Ước tính thô, chỉnh lại ở điểm kiểm tra §5 |
