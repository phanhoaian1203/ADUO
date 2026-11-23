"use client";

import { useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useEffect, useRef, useState } from "react";
import { Button, buttonVariants } from "@/components/ui/button";
import { api } from "@/lib/api/client";
import { problemMessage } from "@/lib/api/problem";
import { strings } from "@/lib/strings";
import { meKey } from "../hooks";
import { AuthCard, FormError, FormNotice } from "./AuthCard";

type State = { kind: "working" } | { kind: "ok" } | { kind: "fail"; message: string } | { kind: "missing" };

/** Mở link trong email: tự gửi mã xác minh lên server khi vào trang. */
export function ConfirmEmail({ userId, token }: { userId?: string; token?: string }) {
  const t = strings.auth.confirm;
  const qc = useQueryClient();
  const [state, setState] = useState<State>(userId && token ? { kind: "working" } : { kind: "missing" });
  const [resent, setResent] = useState(false);
  const started = useRef(false);

  useEffect(() => {
    if (!userId || !token || started.current) return;
    started.current = true; // Strict Mode chạy effect hai lần ở dev; mã chỉ nên gửi một lần
    api.POST("/api/v1/auth/confirm-email", { body: { userId, token } }).then(({ response, error }) => {
      if (response.ok) {
        setState({ kind: "ok" });
        qc.invalidateQueries({ queryKey: meKey });
      } else {
        setState({ kind: "fail", message: problemMessage(error, t.fail) });
      }
    });
  }, [userId, token, qc, t.fail]);

  async function resend() {
    const { response } = await api.POST("/api/v1/auth/resend-confirmation");
    if (response.ok) setResent(true);
  }

  return (
    <AuthCard title={t.title}>
      {state.kind === "working" ? <FormNotice message={t.working} /> : null}
      {state.kind === "missing" ? <FormError message={t.missingLink} /> : null}
      {state.kind === "ok" ? (
        <>
          <FormNotice message={t.ok} />
          <Link href="/" className={buttonVariants({ size: "lg" })}>
            {strings.common.back}
          </Link>
        </>
      ) : null}
      {state.kind === "fail" ? (
        <>
          <FormError message={state.message} />
          {resent ? <FormNotice message={t.resent} /> : null}
          <Button variant="outline" onClick={resend}>
            {t.resend}
          </Button>
        </>
      ) : null}
    </AuthCard>
  );
}
