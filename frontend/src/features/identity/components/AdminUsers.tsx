"use client";

import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { api } from "@/lib/api/client";
import { asProblem, problemMessage } from "@/lib/api/problem";
import { strings } from "@/lib/strings";
import { useMe } from "../hooks";

const t = strings.admin;

/** Quản lý người dùng (FR-ID-08): tìm, khóa, mở khóa. Quyền thật do server kiểm tra; trang này chỉ ẩn/hiện. */
export function AdminUsers() {
  const router = useRouter();
  const qc = useQueryClient();
  const { data: me, isLoading: meLoading } = useMe();
  const [q, setQ] = useState("");
  const [term, setTerm] = useState("");

  useEffect(() => {
    if (!meLoading && me === null) router.replace("/dang-nhap?returnUrl=%2Fquan-tri%2Fnguoi-dung");
  }, [meLoading, me, router]);

  // Gõ xong 300ms mới tìm, tránh gọi API ở mỗi phím.
  useEffect(() => {
    const id = setTimeout(() => setTerm(q), 300);
    return () => clearTimeout(id);
  }, [q]);

  const { data, isLoading, error } = useQuery({
    queryKey: ["admin-users", term],
    enabled: !!me?.isAdmin,
    queryFn: async () => {
      const { data: list, response } = await api.GET("/api/v1/admin/users", { params: { query: { q: term || undefined } } });
      if (!list) throw new Error(String(response.status));
      return list;
    },
  });

  async function toggle(id: string, locked: boolean) {
    const path = locked ? "/api/v1/admin/users/{id}/unlock" : "/api/v1/admin/users/{id}/lock";
    const { response, error: err } = await api.POST(path, { params: { path: { id } } });
    if (response.ok) qc.invalidateQueries({ queryKey: ["admin-users"] });
    else toast.error(asProblem(err)?.code === "LAST_ADMIN" ? problemMessage(err) : problemMessage(err));
  }

  if (meLoading || !me) return <Skeleton className="mx-auto mt-10 h-40 w-full max-w-2xl" />;
  if (!me.isAdmin) return <p className="mx-auto mt-10 max-w-2xl px-4">{t.forbidden}</p>;

  return (
    <main className="mx-auto flex w-full max-w-2xl flex-1 flex-col gap-4 px-4 py-10">
      <h1 className="text-2xl font-semibold tracking-tight">{t.usersTitle}</h1>
      <Input value={q} onChange={(e) => setQ(e.target.value)} placeholder={t.search} aria-label={t.search} />
      {isLoading ? <Skeleton className="h-32 w-full" /> : null}
      {error ? <p className="text-sm text-destructive">{strings.common.genericError}</p> : null}
      {data && data.length === 0 ? <p className="text-sm text-muted-foreground">{t.noUsers}</p> : null}
      <ul className="flex flex-col gap-2">
        {data?.map((u) => (
          <li key={u.id} className="flex items-center justify-between gap-3 rounded-lg border border-border p-3">
            <div className="min-w-0">
              <p className="truncate text-sm font-medium">
                {u.displayName} {u.isAdmin ? <span className="text-primary">· {t.admin}</span> : null}
              </p>
              <p className="truncate text-xs text-muted-foreground">
                {u.email} {u.isLocked ? `· ${t.locked}` : ""}
              </p>
            </div>
            <Button variant={u.isLocked ? "outline" : "destructive"} size="sm" onClick={() => toggle(u.id, u.isLocked)}>
              {u.isLocked ? t.unlock : t.lock}
            </Button>
          </li>
        ))}
      </ul>
    </main>
  );
}
