import createClient, { type Middleware } from "openapi-fetch";
import type { paths } from "./schema";

/**
 * API client có kiểu, sinh từ OpenAPI của backend (`npm run gen:api` tạo ra ./schema.d.ts).
 * Đổi API mà quên cập nhật frontend thì TypeScript báo lỗi ngay lúc build, không phải lúc chạy.
 *
 * baseUrl để trống: luôn gọi cùng origin (/api/...). Dev thì Next.js proxy, prod thì Caddy proxy (AR-01).
 */
export const api = createClient<paths>({ baseUrl: "", credentials: "include" });

const UNSAFE_METHODS = new Set(["POST", "PUT", "PATCH", "DELETE"]);

/** Đọc cookie phía trình duyệt (cookie XSRF-TOKEN không HttpOnly, đúng thiết kế, để JS gửi lại qua header). */
function readCookie(name: string): string | undefined {
  if (typeof document === "undefined") return undefined;
  const match = document.cookie.split("; ").find((c) => c.startsWith(`${name}=`));
  return match ? decodeURIComponent(match.split("=")[1]) : undefined;
}

/**
 * Chống CSRF (Phase 3 – 01 §4.2): mọi request thay đổi dữ liệu gửi kèm header X-XSRF-TOKEN.
 * Cookie XSRF-TOKEN do API cấp qua GET /api/v1/auth/csrf (module Identity).
 */
const csrfMiddleware: Middleware = {
  onRequest({ request }) {
    if (UNSAFE_METHODS.has(request.method)) {
      const token = readCookie("XSRF-TOKEN");
      if (token) request.headers.set("X-XSRF-TOKEN", token);
    }
    return request;
  },
};

api.use(csrfMiddleware);

/** Dạng lỗi ProblemDetails mà API luôn trả (Phase 3 – 03 §1.4). */
export type ApiProblem = {
  title?: string;
  status?: number;
  code?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
};
