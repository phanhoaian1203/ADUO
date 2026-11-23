import { useId, type ComponentProps, type ReactNode } from "react";
import { Input } from "@/components/ui/input";

type Props = Omit<ComponentProps<typeof Input>, "id"> & {
  label: ReactNode;
  error?: string;
  hint?: ReactNode;
};

/**
 * Ô nhập có nhãn + lỗi ngay tại trường (Phase 3 – 05 §4). Gắn aria-invalid/aria-describedby để trình đọc màn hình đọc được lỗi.
 * Nhận `ref` và các prop của react-hook-form qua `{...register("tên")}`.
 */
export function FormField({ label, error, hint, ...inputProps }: Props) {
  const id = useId();
  const describedBy = [error ? `${id}-error` : null, hint ? `${id}-hint` : null].filter(Boolean).join(" ") || undefined;

  return (
    <div className="flex flex-col gap-1.5">
      <label htmlFor={id} className="text-sm font-medium">
        {label}
      </label>
      <Input id={id} aria-invalid={error ? true : undefined} aria-describedby={describedBy} {...inputProps} />
      {hint && !error ? (
        <p id={`${id}-hint`} className="text-xs text-muted-foreground">
          {hint}
        </p>
      ) : null}
      {error ? (
        <p id={`${id}-error`} role="alert" className="text-xs text-destructive">
          {error}
        </p>
      ) : null}
    </div>
  );
}
