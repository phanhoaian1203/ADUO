# CR-001 – Tập trung 100% vào tiếng Đức

| Mục | Nội dung |
|---|---|
| Mã | CR-001 |
| Ngày | 2026-11-22 |
| Người yêu cầu | Chủ dự án |
| Trạng thái | **Approved & Applied**. Tài liệu Phase 1 và Phase 2 đã được cập nhật. Toàn bộ nội dung về ngôn ngữ khác đã bị xóa khỏi dự án |

## 1. Yêu cầu
Thu hẹp phạm vi dự án: **chỉ phát triển nền tảng học tiếng Đức**, để giữ chất lượng khi phát triển một mình.

## 2. Lý do
1. **Nội dung là nút thắt.** Một ngôn ngữ giúp giảm khối lượng nội dung và giúp bạn duyệt chính xác hơn khi làm một mình.
2. **Tiếng Đức có nguồn dữ liệu mở tốt** cho phần khó nhất: Wiktionary có giống danh từ, số nhiều, chia động từ, IPA (CC BY-SA). Tatoeba có câu ví dụ.
3. **Hành trình rõ ràng:** từ con số 0, qua A1, A2, tới B1, mức thường được yêu cầu cho visa Ausbildung ([make-it-in-germany](https://www.make-it-in-germany.com/de/visum-aufenthalt/arten/berufsausbildung), 🟡). Lộ trình này dễ truyền thông và dễ đo lường.

## 3. Rủi ro cần quản lý
| Rủi ro | Cách giảm thiểu |
|---|---|
| Hành trình A1 → B1 dài, người học dễ bỏ cuộc | Hiển thị lộ trình và mốc rõ ràng. Ra A2 sớm. Bài ngắn. Có tiến độ |
| Cạnh tranh với các trung tâm và kênh online | Miễn phí giai đoạn đầu, tiếng Việt dễ hiểu, thông tin chính thống có nguồn và ngày cập nhật, không môi giới |
| B1 cần Schreiben và Sprechen, khó chấm tự động | MVP chỉ chấm tự động Hören và Lesen. Phản hồi Schreiben bằng AI ở V2 (ghi rõ "tham khảo") |

## 4. Phạm vi sau thay đổi
| Phiên bản | Nội dung |
|---|---|
| **MVP (Release 1.0)** | Tiếng Đức **A1 đầy đủ** + Info Hub (Ausbildung, Công nhận văn bằng, Lộ trình học và chứng chỉ). Đầy đủ CMS, import, quy trình duyệt |
| Release 1.1 | A2 |
| V2 | B1, Flashcards + SRS, tiếng Đức cho Ausbildung, phản hồi Schreiben bằng AI, streak và nhắc học, Info Hub về du học đại học Đức |
| V3 | B2, luyện phát âm, Payment / Premium |

## 5. Quyết định kèm theo
- PD-08 (chỉ tiếng Đức) và AD-05 (giữ thực thể `Language` để dữ liệu rõ nghĩa, nhưng không xây tính năng cho ngôn ngữ khác) đều Confirmed.
- Q-CR1-01 (MVP chỉ gồm A1, A2 ở Release 1.1) và Q-CR1-02 (Info Hub MVP chưa có du học đại học) được Confirmed khi chủ dự án yêu cầu bắt đầu Phase 3.
