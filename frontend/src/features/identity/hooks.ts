"use client";

import { useQuery, useQueryClient } from "@tanstack/react-query";
import { api, setSignedInHint } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

export type Me = components["schemas"]["MeResponse"];
export type LearningPurpose = NonNullable<components["schemas"]["LearningPurpose"]>;
export type TargetLevel = NonNullable<components["schemas"]["TargetLevel"]>;

export const meKey = ["me"] as const;

/**
 * Người dùng hiện tại. `data === null` nghĩa là chưa đăng nhập (không phải lỗi). Dùng chung cho mọi trang cần biết trạng thái đăng nhập.
 * Quyền trong `permissions` chỉ để ẩn/hiện giao diện; server luôn kiểm tra lại ở mọi request.
 */
export function useMe() {
  return useQuery({
    queryKey: meKey,
    retry: false,
    queryFn: async (): Promise<Me | null> => {
      const { data, response } = await api.GET("/api/v1/me");
      if (response.status === 401) {
        setSignedInHint(false);
        return null;
      }
      if (!data) throw new Error("me failed");
      setSignedInHint(true);
      return data;
    },
  });
}

/** Cập nhật cache `me` sau khi đăng nhập/đăng ký/sửa hồ sơ. */
export function useSetMe() {
  const qc = useQueryClient();
  return (me: Me | null) => {
    setSignedInHint(me !== null);
    qc.setQueryData(meKey, me);
  };
}
