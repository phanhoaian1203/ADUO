"use client";

import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { FormField } from "@/features/identity/components/FormField";
import { api, postForm } from "@/lib/api/client";
import { asProblem, problemMessage } from "@/lib/api/problem";
import { strings } from "@/lib/strings";
import { AdminShell } from "./AdminShell";

const t = strings.content.media;

export function MediaAdmin() {
  const qc = useQueryClient();
  const [sourceId, setSourceId] = useState("");
  const [attribution, setAttribution] = useState("");
  const [file, setFile] = useState<File | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { data: sources } = useQuery({
    queryKey: ["admin-sources"],
    queryFn: async () => (await api.GET("/api/v1/admin/sources")).data ?? [],
  });
  const { data: page, isLoading } = useQuery({
    queryKey: ["admin-media"],
    queryFn: async () => (await api.GET("/api/v1/admin/media", { params: { query: { pageSize: 48 } } })).data,
  });

  async function upload(e: React.FormEvent) {
    e.preventDefault();
    if (!file || !sourceId) return;
    setBusy(true);
    setError(null);
    const form = new FormData();
    form.set("file", file);
    form.set("sourceId", sourceId);
    if (attribution) form.set("attribution", attribution);
    const res = await postForm("/api/v1/admin/media", form);
    setBusy(false);
    if (!res.ok) {
      setError(problemMessage(asProblem(await res.json().catch(() => null))));
      return;
    }
    const body = await res.json();
    toast.success(body.reused ? t.reused : t.uploaded);
    setFile(null);
    qc.invalidateQueries({ queryKey: ["admin-media"] });
  }

  return (
    <AdminShell current="media" title={t.title} intro={t.intro}>
      <form onSubmit={upload} className="flex flex-col gap-3 rounded-lg border border-border p-4">
        <label className="flex flex-col gap-1.5 text-sm font-medium">
          {t.file}
          <input
            type="file"
            accept="audio/mpeg,audio/ogg,image/png,image/jpeg,image/webp"
            onChange={(e) => setFile(e.target.files?.[0] ?? null)}
            required
          />
        </label>
        <label className="flex flex-col gap-1.5 text-sm font-medium">
          {t.source}
          <select
            required
            value={sourceId}
            onChange={(e) => setSourceId(e.target.value)}
            className="h-8 rounded-lg border border-input bg-transparent px-2 text-sm"
          >
            <option value="">—</option>
            {sources?.map((s) => (
              <option key={s.id} value={s.id} disabled={!s.isActive || s.blocksPublishing}>
                {s.name} ({s.licenseCode})
              </option>
            ))}
          </select>
        </label>
        <FormField label={t.attribution} value={attribution} onChange={(e) => setAttribution(e.target.value)} />
        {error ? (
          <p role="alert" className="text-sm text-destructive">
            {error}
          </p>
        ) : null}
        <Button type="submit" disabled={busy || !file || !sourceId} className="self-start">
          {busy ? strings.common.loading : t.upload}
        </Button>
      </form>

      {isLoading ? <Skeleton className="h-32 w-full" /> : null}
      {page && page.items.length === 0 ? <p className="text-sm text-muted-foreground">{t.empty}</p> : null}
      <ul className="grid grid-cols-1 gap-3 sm:grid-cols-2">
        {page?.items.map((m) => (
          <li key={m.id} className="flex flex-col gap-2 rounded-lg border border-border p-3">
            <p className="truncate text-sm font-medium">{m.fileName ?? m.id}</p>
            <p className="text-xs text-muted-foreground">
              {m.contentType} · {(Number(m.sizeBytes) / 1024).toFixed(1)} KB
            </p>
            {m.kind === "Audio" ? <audio controls preload="none" src={`/api/v1/admin/media/${m.id}/file`} className="w-full" /> : null}
            {m.kind === "Image" ? (
              // eslint-disable-next-line @next/next/no-img-element -- ảnh admin nằm sau đăng nhập, không cần tối ưu bằng next/image
              <img src={`/api/v1/admin/media/${m.id}/file`} alt={m.fileName ?? ""} className="max-h-40 rounded-md object-contain" />
            ) : null}
          </li>
        ))}
      </ul>
    </AdminShell>
  );
}
