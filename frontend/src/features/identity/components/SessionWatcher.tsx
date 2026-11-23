"use client";

import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { onSessionExpired, setSignedInHint } from "@/lib/api/client";
import { meKey, type Me } from "../hooks";

/** Khi làm mới token thất bại (phiên hết hẳn hoặc bị thu hồi), đưa người đang đăng nhập về trang đăng nhập. */
export function SessionWatcher() {
  const qc = useQueryClient();
  const router = useRouter();

  useEffect(
    () =>
      onSessionExpired(() => {
        const wasSignedIn = qc.getQueryData<Me | null>(meKey) != null;
        setSignedInHint(false);
        qc.setQueryData(meKey, null);
        if (wasSignedIn) router.replace("/dang-nhap?het-phien=1");
      }),
    [qc, router],
  );

  return null;
}
