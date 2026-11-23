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

/** Bảo đảm có cookie XSRF-TOKEN. API chỉ cấp khi chưa có, nên gọi nhiều lần không sao. */
async function ensureCsrf(): Promise<void> {
  if (readCookie("XSRF-TOKEN")) return;
  await fetch("/api/v1/auth/csrf", { credentials: "include" });
}

/**
 * Chống CSRF (docs 01-identity §14.7): mọi request thay đổi dữ liệu gửi kèm header X-XSRF-TOKEN.
 * Cookie XSRF-TOKEN do API cấp qua GET /api/v1/auth/csrf.
 */
const csrfMiddleware: Middleware = {
  async onRequest({ request }) {
    if (UNSAFE_METHODS.has(request.method)) {
      await ensureCsrf();
      const token = readCookie("XSRF-TOKEN");
      if (token) request.headers.set("X-XSRF-TOKEN", token);
    }
    return request;
  },
};

// ───────────── Tự làm mới access token (docs 01-identity §14.10) ─────────────

const SESSION_EXPIRED_EVENT = "aduo:session-expired";
const LAST_REFRESH_KEY = "aduo:last-refresh";
const RECENT_REFRESH_MS = 5_000;

/** Các endpoint xác thực không được tự thử làm mới (tránh vòng lặp, và đăng nhập sai không phải "hết phiên"). */
const NO_RETRY_PREFIXES = ["/api/v1/auth/"];

/** Chỉ làm mới khi access token hết hạn hoặc chưa có. Phiên bị thu hồi/đăng nhập lại thì không. */
const RETRYABLE_CODES = new Set(["TOKEN_EXPIRED", "UNAUTHENTICATED"]);

const SIGNED_IN_HINT_KEY = "aduo:signed-in";

function hasSignedInHint(): boolean {
  return typeof localStorage !== "undefined" && localStorage.getItem(SIGNED_IN_HINT_KEY) === "1";
}

/**
 * Cookie phiên là HttpOnly nên JS không biết mình có đang đăng nhập hay không. Lưu một "gợi ý" (không chứa bí mật)
 * để khách chưa đăng nhập không gọi /auth/refresh vô ích ở mỗi trang (và không chạm giới hạn tốc độ).
 */
export function setSignedInHint(signedIn: boolean): void {
  if (typeof localStorage === "undefined") return;
  if (signedIn) localStorage.setItem(SIGNED_IN_HINT_KEY, "1");
  else localStorage.removeItem(SIGNED_IN_HINT_KEY);
}

let inflight: Promise<boolean> | null = null;
const retryCopies = new WeakMap<Request, Request>();

async function doRefresh(): Promise<boolean> {
  await ensureCsrf();
  const token = readCookie("XSRF-TOKEN");
  const res = await fetch("/api/v1/auth/refresh", {
    method: "POST",
    credentials: "include",
    headers: token ? { "X-XSRF-TOKEN": token } : {},
  });
  if (res.ok) localStorage.setItem(LAST_REFRESH_KEY, String(Date.now()));
  return res.ok;
}

/**
 * Chỉ MỘT lần làm mới tại một thời điểm, kể cả khi nhiều tab cùng hết hạn:
 * trong một tab dùng chung promise, giữa các tab dùng Web Locks. Tab vào sau nếu thấy tab khác vừa làm mới
 * (cookie đã mới) thì bỏ qua và thử lại luôn. Nhờ vậy gần như không bao giờ gặp tình huống "dùng lại token cũ".
 */
function refreshOnce(): Promise<boolean> {
  inflight ??= (async () => {
    try {
      const run = async () => {
        const last = Number(localStorage.getItem(LAST_REFRESH_KEY) ?? 0);
        if (Date.now() - last < RECENT_REFRESH_MS) return true;
        return doRefresh();
      };
      return navigator.locks ? await navigator.locks.request("aduo-refresh", run) : await run();
    } finally {
      inflight = null;
    }
  })();
  return inflight;
}

const refreshMiddleware: Middleware = {
  onRequest({ request }) {
    retryCopies.set(request, request.clone());
    return request;
  },
  async onResponse({ request, response }) {
    if (response.status !== 401 || !hasSignedInHint()) return response;
    const path = new URL(request.url).pathname;
    if (NO_RETRY_PREFIXES.some((p) => path.startsWith(p))) return response;

    const problem = await response.clone().json().catch(() => null);
    if (!problem || !RETRYABLE_CODES.has(problem.code)) return response;

    if (!(await refreshOnce())) {
      window.dispatchEvent(new Event(SESSION_EXPIRED_EVENT));
      return response;
    }

    const copy = retryCopies.get(request);
    return copy ? fetch(copy) : response;
  },
};

api.use(csrfMiddleware);
api.use(refreshMiddleware);

/** Lắng nghe khi phiên hết hẳn (refresh thất bại) để chuyển về trang đăng nhập. */
export function onSessionExpired(handler: () => void): () => void {
  window.addEventListener(SESSION_EXPIRED_EVENT, handler);
  return () => window.removeEventListener(SESSION_EXPIRED_EVENT, handler);
}

/** Dạng lỗi ProblemDetails mà API luôn trả (Phase 3 – 03 §1.4). */
export type ApiProblem = {
  title?: string;
  status?: number;
  code?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
};

/**
 * Gửi form multipart (upload file). openapi-fetch chưa mô tả tốt multipart nên dùng fetch trực tiếp,
 * vẫn gắn X-XSRF-TOKEN và tự làm mới token một lần nếu access token hết hạn (giống các request khác).
 * Trình duyệt tự đặt Content-Type kèm boundary, nên KHÔNG tự đặt header này.
 */
export async function postForm(url: string, form: FormData): Promise<Response> {
  const send = async () => {
    await ensureCsrf();
    const token = readCookie("XSRF-TOKEN");
    return fetch(url, { method: "POST", body: form, credentials: "include", headers: token ? { "X-XSRF-TOKEN": token } : {} });
  };
  let res = await send();
  if (res.status === 401 && hasSignedInHint()) {
    const problem = await res.clone().json().catch(() => null);
    if (problem && RETRYABLE_CODES.has(problem.code) && (await refreshOnce())) res = await send();
  }
  return res;
}
