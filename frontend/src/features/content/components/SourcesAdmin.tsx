"use client";

import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { FormField } from "@/features/identity/components/FormField";
import { api } from "@/lib/api/client";
import { problemMessage } from "@/lib/api/problem";
import { strings } from "@/lib/strings";
import { cn } from "@/lib/utils";
import { AdminShell } from "./AdminShell";

const t = strings.content.sources;

function CreateSourceForm({ onCreated }: { onCreated: () => void }) {
  const { data: licenses } = useQuery({
    queryKey: ["admin-licenses"],
    staleTime: Infinity,
    queryFn: async () => (await api.GET("/api/v1/admin/licenses")).data ?? [],
  });
  const [form, setForm] = useState({ name: "", provider: "", url: "", notes: "", licenseId: "" });
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    const { data, error: err } = await api.POST("/api/v1/admin/sources", {
      body: {
        name: form.name,
        provider: form.provider || null,
        url: form.url || null,
        notes: form.notes || null,
        defaultLicenseId: Number(form.licenseId),
      },
    });
    setBusy(false);
    if (!data) {
      setError(problemMessage(err));
      return;
    }
    toast.success(t.created);
    setForm({ name: "", provider: "", url: "", notes: "", licenseId: "" });
    onCreated();
  }

  return (
    <form onSubmit={submit} className="flex flex-col gap-3 rounded-lg border border-border p-4">
      <FormField label={t.name} value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
      <FormField label={t.provider} value={form.provider} onChange={(e) => setForm({ ...form, provider: e.target.value })} />
      <FormField label={t.url} type="url" value={form.url} onChange={(e) => setForm({ ...form, url: e.target.value })} />
      <label className="flex flex-col gap-1.5 text-sm font-medium">
        {t.license}
        <select
          required
          value={form.licenseId}
          onChange={(e) => setForm({ ...form, licenseId: e.target.value })}
          className="h-8 rounded-lg border border-input bg-transparent px-2 text-sm"
        >
          <option value="">—</option>
          {licenses?.map((l) => (
            <option key={l.id} value={l.id}>
              {l.code}
              {l.allowsCommercial ? "" : " (không thương mại)"}
            </option>
          ))}
        </select>
      </label>
      <FormField label={t.notes} value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
      {error ? (
        <p role="alert" className="text-sm text-destructive">
          {error}
        </p>
      ) : null}
      <Button type="submit" disabled={busy} className="self-start">
        {t.create}
      </Button>
    </form>
  );
}

export function SourcesAdmin() {
  const qc = useQueryClient();
  const { data, isLoading } = useQuery({
    queryKey: ["admin-sources"],
    queryFn: async () => (await api.GET("/api/v1/admin/sources")).data ?? [],
  });
  const refresh = () => qc.invalidateQueries({ queryKey: ["admin-sources"] });

  async function patch(id: string, body: { isActive?: boolean; licenseVerified?: boolean }) {
    const { response, error } = await api.PATCH("/api/v1/admin/sources/{id}", {
      params: { path: { id } },
      body: { name: null, provider: null, url: null, defaultLicenseId: null, notes: null, isActive: null, licenseVerified: null, ...body },
    });
    if (response.ok) refresh();
    else toast.error(problemMessage(error));
  }

  return (
    <AdminShell current="sources" title={t.title} intro={t.intro}>
      {isLoading ? <Skeleton className="h-32 w-full" /> : null}
      <ul className="flex flex-col gap-2">
        {data?.map((s) => (
          <li key={s.id} className={cn("flex flex-col gap-2 rounded-lg border p-3", s.blocksPublishing ? "border-destructive/50" : "border-border")}>
            <div className="flex flex-wrap items-center justify-between gap-2">
              <p className="text-sm font-medium">
                {s.name} <span className="text-muted-foreground">· {s.licenseCode}</span>
              </p>
              <span className={cn("rounded-full px-2 py-0.5 text-xs", s.licenseAllowsCommercial ? "bg-primary/10 text-primary" : "bg-destructive/10 text-destructive")}>
                {s.licenseAllowsCommercial ? t.allowed : t.blocked}
              </span>
            </div>
            <p className="text-xs text-muted-foreground">
              {s.licenseVerifiedAt ? `${t.verified} (${new Date(s.licenseVerifiedAt).toLocaleDateString("vi-VN")})` : `⚖️ ${t.unverified}`}
              {" · "}
              {s.isActive ? t.active : t.inactive}
            </p>
            {s.notes ? <p className="text-xs text-muted-foreground">{s.notes}</p> : null}
            <div className="flex flex-wrap gap-2">
              <Button size="sm" variant="outline" onClick={() => patch(s.id, { licenseVerified: !s.licenseVerifiedAt })}>
                {s.licenseVerifiedAt ? t.unmarkVerified : t.markVerified}
              </Button>
              <Button size="sm" variant="outline" onClick={() => patch(s.id, { isActive: !s.isActive })}>
                {t.toggle}
              </Button>
            </div>
          </li>
        ))}
      </ul>
      <CreateSourceForm onCreated={refresh} />
    </AdminShell>
  );
}
