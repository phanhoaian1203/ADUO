"use client";

import { useQuery } from "@tanstack/react-query";
import { buttonVariants } from "@/components/ui/button";
import { api } from "@/lib/api/client";
import { safeReturnUrl } from "@/lib/api/problem";
import { strings } from "@/lib/strings";
import { cn } from "@/lib/utils";

/**
 * Nút đăng nhập Google. Chỉ hiện khi backend đã bật Google (có ClientId/Secret).
 * Là liên kết điều hướng cả trang (không phải fetch) vì luồng OAuth chuyển hướng qua trang của Google.
 */
export function GoogleButton({ returnUrl }: { returnUrl?: string }) {
  const { data } = useQuery({
    queryKey: ["auth-providers"],
    staleTime: Infinity,
    queryFn: async () => (await api.GET("/api/v1/auth/external/providers")).data ?? { google: false },
  });

  if (!data?.google) return null;

  const href = `/api/v1/auth/external/google?returnUrl=${encodeURIComponent(safeReturnUrl(returnUrl))}`;
  return (
    <div className="flex flex-col gap-3">
      <a href={href} className={cn(buttonVariants({ variant: "outline", size: "lg" }), "w-full")}>
        {strings.auth.google.button}
      </a>
      <p className="text-xs text-muted-foreground">{strings.auth.google.consent}</p>
      <p className="text-center text-xs text-muted-foreground">{strings.auth.google.or}</p>
    </div>
  );
}
