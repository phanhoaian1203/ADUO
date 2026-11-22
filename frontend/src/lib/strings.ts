/**
 * Toàn bộ chuỗi giao diện tiếng Việt (Q6: chỉ tiếng Việt, gom một chỗ để dễ sửa từ ngữ).
 * Văn phong: xưng "bạn", thân thiện, tránh thuật ngữ (Phase 3 – 06 §5).
 */
export const strings = {
  app: {
    name: "ADUO",
    tagline: "Học tiếng Đức từ con số 0 đến B1 – cho Ausbildung. Bằng tiếng Việt, miễn phí.",
  },
  common: {
    retry: "Thử lại",
    loading: "Đang tải…",
    genericError: "Đã có lỗi xảy ra. Vui lòng thử lại sau.",
    offline: "Mất kết nối mạng. Vui lòng kiểm tra lại.",
  },
  system: {
    apiStatusTitle: "Trạng thái hệ thống",
    apiOk: "API: OK",
    apiDown: "API: không kết nối được",
    databaseOk: "Cơ sở dữ liệu: OK",
    databaseDown: "Cơ sở dữ liệu: chưa sẵn sàng",
  },
} as const;
