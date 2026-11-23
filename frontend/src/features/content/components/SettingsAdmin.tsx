"use client";

import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { FormField } from "@/features/identity/components/FormField";
import { api } from "@/lib/api/client";
import { asProblem, problemMessage } from "@/lib/api/problem";
import { strings } from "@/lib/strings";
import { AdminShell } from "./AdminShell";

const t = strings.content.settings;

export function SettingsAdmin() {
  const qc = useQueryClient();
  const { data, isLoading } = useQuery({
    queryKey: ["admin-settings"],
    queryFn: async () => (await api.GET("/api/v1/admin/settings")).data ?? [],
  });
  const [edits, setEdits] = useState<Record<string, string>>({});
  const [errors, setErrors] = useState<Record<string, string[]>>({});

  async function save(e: React.FormEvent) {
    e.preventDefault();
    const values = Object.fromEntries(Object.entries(edits).map(([k, v]) => [k, Number(v)]));
    if (Object.keys(values).length === 0) return;
    const { data: updated, error } = await api.PATCH("/api/v1/admin/settings", { body: { values } });
    if (!updated) {
      setErrors(asProblem(error)?.errors ?? {});
      toast.error(problemMessage(error));
      return;
    }
    setErrors({});
    setEdits({});
    toast.success(t.saved);
    qc.setQueryData(["admin-settings"], updated);
  }

  return (
    <AdminShell current="settings" title={t.title} intro={t.intro}>
      {isLoading ? <Skeleton className="h-40 w-full" /> : null}
      <form onSubmit={save} className="flex flex-col gap-4">
        {data?.map((s) => (
          <FormField
            key={s.key}
            label={s.key}
            type="number"
            step="any"
            min={Number(s.min)}
            max={Number(s.max)}
            value={edits[s.key] ?? String(s.value)}
            hint={`${s.description} (từ ${s.min} đến ${s.max}, mặc định ${s.default})`}
            error={errors[s.key]?.[0]}
            onChange={(e) => setEdits({ ...edits, [s.key]: e.target.value })}
          />
        ))}
        <Button type="submit" disabled={Object.keys(edits).length === 0} className="self-start">
          {t.save}
        </Button>
      </form>
    </AdminShell>
  );
}
