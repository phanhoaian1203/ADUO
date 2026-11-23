import type { ReactNode } from "react";

/** Khung chung cho các trang xác thực: căn giữa, rộng vừa màn hình điện thoại. */
export function AuthCard({ title, children, footer }: { title: string; children: ReactNode; footer?: ReactNode }) {
  return (
    <main className="mx-auto flex w-full max-w-md flex-1 flex-col justify-center gap-6 px-4 py-10">
      <h1 className="text-2xl font-semibold tracking-tight">{title}</h1>
      {children}
      {footer ? <p className="text-sm text-muted-foreground">{footer}</p> : null}
    </main>
  );
}

/** Thông báo lỗi chung của cả form (lỗi không thuộc trường nào). */
export function FormError({ message }: { message?: string | null }) {
  if (!message) return null;
  return (
    <p role="alert" className="rounded-lg bg-destructive/10 px-3 py-2 text-sm text-destructive">
      {message}
    </p>
  );
}

export function FormNotice({ message }: { message?: string | null }) {
  if (!message) return null;
  return (
    <p role="status" className="rounded-lg bg-muted px-3 py-2 text-sm">
      {message}
    </p>
  );
}
