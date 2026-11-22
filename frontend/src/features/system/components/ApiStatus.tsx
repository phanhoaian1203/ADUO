"use client";

import { useEffect, useState } from "react";
import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/ui/button";
import { api } from "@/lib/api/client";
import { strings } from "@/lib/strings";

type State =
  | { kind: "loading" }
  | { kind: "ready"; database: boolean }
  | { kind: "down" };

/**
 * Trang thử nghiệm Foundation: gọi GET /api/health/ready qua proxy cùng origin và hiển thị kết quả.
 * Có đủ 3 trạng thái bắt buộc: đang tải (skeleton), thành công, lỗi (kèm nút thử lại) – Phase 3 – 05 §4.
 */
export function ApiStatus() {
  const [state, setState] = useState<State>({ kind: "loading" });
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    let cancelled = false;

    async function check() {
      try {
        // 503 cũng có body hợp lệ (database chưa sẵn sàng), nên dùng `data` hoặc `error` đều được.
        const { data, error } = await api.GET("/api/health/ready");
        if (cancelled) return;
        const body = data ?? error;
        setState(body ? { kind: "ready", database: body.status === "Healthy" } : { kind: "down" });
      } catch {
        if (!cancelled) setState({ kind: "down" });
      }
    }

    void check();
    return () => {
      cancelled = true;
    };
  }, [attempt]);

  if (state.kind === "loading") {
    return <Skeleton className="h-24 w-full max-w-sm" aria-label={strings.common.loading} />;
  }

  if (state.kind === "down") {
    return (
      <div role="alert" className="max-w-sm rounded-xl border border-danger/30 bg-danger/5 p-4">
        <p className="font-medium text-danger">{strings.system.apiDown}</p>
        <Button variant="outline" className="mt-3" onClick={() => { setState({ kind: "loading" }); setAttempt((n) => n + 1); }}>
          {strings.common.retry}
        </Button>
      </div>
    );
  }

  return (
    <div className="max-w-sm rounded-xl border bg-card p-4">
      <p className="font-medium text-success">✓ {strings.system.apiOk}</p>
      <p className={state.database ? "text-success" : "text-warning"}>
        {state.database ? "✓ " : "⚠ "}
        {state.database ? strings.system.databaseOk : strings.system.databaseDown}
      </p>
    </div>
  );
}
