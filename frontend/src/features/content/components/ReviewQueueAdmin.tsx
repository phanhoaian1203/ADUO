"use client";

import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { api } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";
import { asProblem, problemMessage } from "@/lib/api/problem";
import { strings } from "@/lib/strings";
import { cn } from "@/lib/utils";
import { AdminShell } from "./AdminShell";

const t = strings.content.review;
type Checklist = components["schemas"]["ChecklistRequest"];
type Item = components["schemas"]["ReviewQueueItem"];

const EMPTY: Checklist = {
  sourceAndLicenseCorrect: false,
  contentAccurate: false,
  levelAppropriate: false,
  vietnameseNatural: false,
  mediaPlayable: false,
  flagsReviewed: false,
};

const key = (i: Pick<Item, "resource" | "id">) => `${i.resource}:${i.id}`;

function reasonText(code: string): string {
  return (t.reasons as Record<string, string>)[code] ?? code;
}

/**
 * Hàng đợi duyệt gom từ mọi loại nội dung. Publish luôn đi kèm checklist 6 mục; publish hàng loạt chỉ cho mục không có cờ.
 * Mọi quy tắc thật (PublishGuard) nằm ở server, giao diện chỉ hiển thị lý do server trả về.
 */
export function ReviewQueueAdmin() {
  const qc = useQueryClient();
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [checklist, setChecklist] = useState<Checklist>(EMPTY);
  const [messages, setMessages] = useState<string[]>([]);

  const { data, isLoading } = useQuery({
    queryKey: ["admin-review-queue"],
    queryFn: async () => (await api.GET("/api/v1/admin/review-queue", { params: { query: { pageSize: 100 } } })).data,
  });
  const refresh = () => qc.invalidateQueries({ queryKey: ["admin-review-queue"] });
  const items = data?.items ?? [];
  const allChecked = Object.values(checklist).every(Boolean);

  function toggle(i: Item) {
    const next = new Set(selected);
    if (next.has(key(i))) next.delete(key(i));
    else next.add(key(i));
    setSelected(next);
  }

  async function publish(list: Item[]) {
    setMessages([]);
    const { data: outcomes, error } = await api.POST("/api/v1/admin/review-queue/publish-batch", {
      body: { items: list.map((i) => ({ resource: i.resource, id: i.id, version: Number(i.version) })), checklist },
    });
    if (!outcomes) {
      setMessages([problemMessage(error)]);
      return;
    }
    const failed = outcomes.filter((o) => !o.published);
    if (failed.length === 0) toast.success(t.published);
    setMessages(failed.map((o) => `${o.resource}/${o.id}: ${[o.code, ...(o.reasons ?? [])].filter(Boolean).map((c) => reasonText(String(c))).join(", ")}`));
    setSelected(new Set());
    refresh();
  }

  async function lifecycle(i: Item, action: "submit-review" | "return-to-draft" | "archive") {
    let note: string | null = null;
    if (action === "return-to-draft") {
      note = window.prompt(t.notePrompt);
      if (!note?.trim()) return;
    }
    const res = await fetch(`/api/v1/admin/${i.resource}/${i.id}/${action}`, {
      method: "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json", "X-XSRF-TOKEN": decodeURIComponent(document.cookie.split("; ").find((c) => c.startsWith("XSRF-TOKEN="))?.split("=")[1] ?? "") },
      body: JSON.stringify({ version: Number(i.version), note }),
    });
    if (res.ok) refresh();
    else toast.error(problemMessage(asProblem(await res.json().catch(() => null))));
  }

  return (
    <AdminShell current="review" title={t.title} intro={t.intro}>
      {isLoading ? <Skeleton className="h-32 w-full" /> : null}
      {data && items.length === 0 ? <p className="text-sm text-muted-foreground">{t.empty}</p> : null}

      <ul className="flex flex-col gap-2">
        {items.map((i) => (
          <li key={key(i)} className={cn("flex flex-col gap-2 rounded-lg border p-3", i.flagged ? "border-destructive/50" : "border-border")}>
            <label className="flex items-start gap-2 text-sm">
              <input type="checkbox" className="mt-0.5 size-4" disabled={i.flagged} checked={selected.has(key(i))} onChange={() => toggle(i)} />
              <span className="min-w-0">
                <span className="font-medium">{i.title}</span>
                <span className="text-muted-foreground">
                  {" "}
                  · {i.resource} · {i.status} · {i.origin}
                </span>
                {i.flagged ? <span className="ml-1 rounded-full bg-destructive/10 px-2 py-0.5 text-xs text-destructive">{t.flagged}</span> : null}
              </span>
            </label>
            {i.flags.length > 0 ? <p className="text-xs text-destructive">{i.flags.join(", ")}</p> : null}
            <div className="flex flex-wrap gap-2">
              {i.status === "Draft" ? (
                <Button size="sm" variant="outline" onClick={() => lifecycle(i, "submit-review")}>
                  {t.submit}
                </Button>
              ) : null}
              {i.status === "InReview" ? (
                <Button size="sm" variant="outline" onClick={() => lifecycle(i, "return-to-draft")}>
                  {t.returnToDraft}
                </Button>
              ) : null}
              {i.status === "NeedsUpdate" ? (
                <Button size="sm" variant="outline" onClick={() => lifecycle(i, "archive")}>
                  {t.archive}
                </Button>
              ) : null}
              <Button size="sm" disabled={!allChecked} onClick={() => publish([i])}>
                {t.publish}
              </Button>
            </div>
          </li>
        ))}
      </ul>

      {items.length > 0 ? (
        <fieldset className="flex flex-col gap-2 rounded-lg border border-border p-4">
          <legend className="px-1 text-sm font-medium">{t.checklistTitle}</legend>
          {(Object.keys(EMPTY) as (keyof Checklist)[]).map((k) => (
            <label key={k} className="flex items-center gap-2 text-sm">
              <input type="checkbox" className="size-4" checked={checklist[k]} onChange={(e) => setChecklist({ ...checklist, [k]: e.target.checked })} />
              {t.checklist[k]}
            </label>
          ))}
          <Button
            className="mt-2 self-start"
            disabled={!allChecked || selected.size === 0}
            onClick={() => publish(items.filter((i) => selected.has(key(i))))}
          >
            {t.publishSelected} ({selected.size})
          </Button>
        </fieldset>
      ) : null}

      {messages.length > 0 ? (
        <div role="alert" className="rounded-lg bg-destructive/10 px-3 py-2 text-sm text-destructive">
          <p className="font-medium">{t.failed}</p>
          <ul className="list-disc pl-5">
            {messages.map((m) => (
              <li key={m}>{m}</li>
            ))}
          </ul>
        </div>
      ) : null}
    </AdminShell>
  );
}
