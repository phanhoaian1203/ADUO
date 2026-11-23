"use client";

import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { api, postForm } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";
import { asProblem, problemMessage } from "@/lib/api/problem";
import { strings } from "@/lib/strings";
import { AdminShell } from "./AdminShell";

const t = strings.content.imports;
type Batch = components["schemas"]["ImportBatchResponse"];

function Stat({ label, value }: { label: string; value: number | string | null | undefined }) {
  if (value === null || value === undefined) return null;
  return (
    <div className="rounded-lg bg-muted px-3 py-2">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className="text-lg font-semibold">{value}</p>
    </div>
  );
}

/** Báo cáo dry-run / kết quả batch: số liệu + danh sách dòng lỗi và dòng trùng. */
function BatchReport({ batch, onCommit, onCancel, busy }: { batch: Batch; onCommit?: () => void; onCancel?: () => void; busy: boolean }) {
  const r = batch.report;
  const errors = r.errors ?? [];
  const duplicates = r.duplicates ?? [];
  return (
    <div className="flex flex-col gap-3 rounded-lg border border-border p-4">
      <p className="text-sm">
        <span className="font-medium">{batch.contentType}</span> · {batch.status}
      </p>
      <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
        <Stat label={t.total} value={r.total} />
        <Stat label={t.valid} value={r.valid} />
        <Stat label={t.errors} value={errors.length} />
        <Stat label={t.duplicates} value={duplicates.length} />
        <Stat label={t.inserted} value={r.inserted} />
        <Stat label={t.deleted} value={r.deleted} />
        <Stat label={t.kept} value={r.keptPublished} />
      </div>
      {errors.length > 0 ? (
        <details>
          <summary className="cursor-pointer text-sm font-medium">
            {t.errors} ({errors.length})
          </summary>
          <ul className="mt-2 max-h-48 overflow-auto text-xs">
            {errors.slice(0, 200).map((e, i) => (
              <li key={i}>
                {t.row} {String(e.row)} · {e.field}: {e.message}
              </li>
            ))}
          </ul>
        </details>
      ) : null}
      {duplicates.length > 0 ? (
        <details>
          <summary className="cursor-pointer text-sm font-medium">
            {t.duplicates} ({duplicates.length})
          </summary>
          <ul className="mt-2 max-h-48 overflow-auto text-xs">
            {duplicates.slice(0, 200).map((d, i) => (
              <li key={i}>
                {t.row} {String(d.row)} · {d.key} ({d.where})
              </li>
            ))}
          </ul>
        </details>
      ) : null}
      <div className="flex gap-2">
        {onCommit ? (
          <Button disabled={busy || Number(r.valid ?? 0) === 0} onClick={onCommit}>
            {t.commit}
          </Button>
        ) : null}
        {onCancel ? (
          <Button variant="outline" disabled={busy} onClick={onCancel}>
            {t.cancel}
          </Button>
        ) : null}
      </div>
    </div>
  );
}

export function ImportAdmin() {
  const qc = useQueryClient();
  const [contentType, setContentType] = useState("");
  const [sourceId, setSourceId] = useState("");
  const [file, setFile] = useState<File | null>(null);
  const [current, setCurrent] = useState<Batch | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { data: types, isLoading } = useQuery({
    queryKey: ["admin-import-types"],
    queryFn: async () => (await api.GET("/api/v1/admin/imports/content-types")).data ?? [],
  });
  const { data: sources } = useQuery({
    queryKey: ["admin-sources"],
    queryFn: async () => (await api.GET("/api/v1/admin/sources")).data ?? [],
  });
  const { data: recent } = useQuery({
    queryKey: ["admin-imports"],
    queryFn: async () => (await api.GET("/api/v1/admin/imports")).data ?? [],
  });

  async function dryRun(e: React.FormEvent) {
    e.preventDefault();
    if (!file) return;
    setBusy(true);
    setError(null);
    const form = new FormData();
    form.set("file", file);
    form.set("sourceId", sourceId);
    form.set("contentType", contentType);
    const res = await postForm("/api/v1/admin/imports", form);
    setBusy(false);
    if (!res.ok) {
      setError(problemMessage(asProblem(await res.json().catch(() => null))));
      return;
    }
    setCurrent(await res.json());
    qc.invalidateQueries({ queryKey: ["admin-imports"] });
  }

  async function act(kind: "commit" | "cancel", id: string) {
    setBusy(true);
    const { data, error: err } = await api.POST(kind === "commit" ? "/api/v1/admin/imports/{id}/commit" : "/api/v1/admin/imports/{id}/cancel", {
      params: { path: { id } },
    });
    setBusy(false);
    if (!data) {
      toast.error(problemMessage(err));
      return;
    }
    toast.success(kind === "commit" ? t.committed : t.cancelled);
    setCurrent(data);
    qc.invalidateQueries({ queryKey: ["admin-imports"] });
  }

  const noTypes = !isLoading && (types?.length ?? 0) === 0;

  return (
    <AdminShell current="imports" title={t.title} intro={t.intro}>
      {isLoading ? <Skeleton className="h-24 w-full" /> : null}
      {noTypes ? <p className="rounded-lg bg-muted px-3 py-2 text-sm">{t.noTypes}</p> : null}

      {!noTypes && types ? (
        <form onSubmit={dryRun} className="flex flex-col gap-3 rounded-lg border border-border p-4">
          <label className="flex flex-col gap-1.5 text-sm font-medium">
            {t.contentType}
            <select
              required
              value={contentType}
              onChange={(e) => setContentType(e.target.value)}
              className="h-8 rounded-lg border border-input bg-transparent px-2 text-sm"
            >
              <option value="">—</option>
              {types.map((c) => (
                <option key={c} value={c}>
                  {c}
                </option>
              ))}
            </select>
          </label>
          {contentType ? (
            <a className="text-sm text-primary underline-offset-4 hover:underline" href={`/api/v1/admin/imports/templates/${contentType}`} download>
              {t.template}
            </a>
          ) : null}
          <label className="flex flex-col gap-1.5 text-sm font-medium">
            {strings.content.media.source}
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
          <input type="file" accept=".csv,.json" required onChange={(e) => setFile(e.target.files?.[0] ?? null)} />
          {error ? (
            <p role="alert" className="text-sm text-destructive">
              {error}
            </p>
          ) : null}
          <Button type="submit" disabled={busy || !file} className="self-start">
            {busy ? strings.common.loading : t.dryRun}
          </Button>
        </form>
      ) : null}

      {current ? (
        <BatchReport
          batch={current}
          busy={busy}
          onCommit={current.status === "DryRun" ? () => act("commit", current.id) : undefined}
          onCancel={current.status !== "Cancelled" ? () => act("cancel", current.id) : undefined}
        />
      ) : null}

      {recent && recent.length > 0 ? (
        <section className="flex flex-col gap-2">
          <h2 className="text-lg font-semibold">{t.recent}</h2>
          <ul className="flex flex-col gap-1 text-sm">
            {recent.map((b) => (
              <li key={b.id}>
                <button type="button" className="text-left underline-offset-4 hover:underline" onClick={() => setCurrent(b)}>
                  {new Date(b.createdAt).toLocaleString("vi-VN")} · {b.contentType} · {b.status}
                </button>
              </li>
            ))}
          </ul>
        </section>
      ) : null}
    </AdminShell>
  );
}
