import type { FieldValues, Path, UseFormSetError } from "react-hook-form";
import { strings } from "@/lib/strings";
import type { ApiProblem } from "./client";

/** Lấy ApiProblem từ phần `error` của openapi-fetch (nếu có). */
export function asProblem(error: unknown): ApiProblem | undefined {
  return error && typeof error === "object" ? (error as ApiProblem) : undefined;
}

/** Thông báo tiếng Việt cho người dùng. Backend luôn trả `title` tiếng Việt cho lỗi nghiệp vụ. */
export function problemMessage(error: unknown, fallback: string = strings.common.genericError): string {
  return asProblem(error)?.title || fallback;
}

/** Gán lỗi theo từng trường của backend (`errors`) vào react-hook-form. Trả về true nếu đã gán được ít nhất một lỗi. */
export function applyFieldErrors<T extends FieldValues>(error: unknown, setError: UseFormSetError<T>): boolean {
  const errors = asProblem(error)?.errors;
  if (!errors) return false;
  let applied = false;
  for (const [field, messages] of Object.entries(errors)) {
    if (messages.length > 0 && field !== "retryAfter") {
      setError(field as Path<T>, { message: messages[0] });
      applied = true;
    }
  }
  return applied;
}

/** Chỉ chấp nhận đường dẫn nội bộ làm returnUrl (chống open redirect, ID-T14). */
export function safeReturnUrl(url: string | null | undefined, fallback = "/"): string {
  if (!url || !url.startsWith("/") || url.startsWith("//") || url.includes("\\")) return fallback;
  return url;
}
