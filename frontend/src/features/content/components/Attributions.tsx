"use client";

import { useQuery } from "@tanstack/react-query";
import { Skeleton } from "@/components/ui/skeleton";
import { api } from "@/lib/api/client";
import { strings } from "@/lib/strings";

const t = strings.content.attributions;

/**
 * Trang công khai /nguon-du-lieu: nghĩa vụ ghi công của CC BY và CC BY-SA (BR-CNT-04).
 * Chỉ liệt kê nguồn đang có nội dung đã xuất bản.
 */
export function Attributions() {
  const { data, isLoading } = useQuery({
    queryKey: ["attributions"],
    queryFn: async () => (await api.GET("/api/v1/attributions")).data ?? [],
  });

  return (
    <main className="mx-auto flex w-full max-w-2xl flex-1 flex-col gap-4 px-4 py-10">
      <h1 className="text-2xl font-semibold tracking-tight">{t.title}</h1>
      <p className="text-sm text-muted-foreground">{t.intro}</p>
      {isLoading ? <Skeleton className="h-24 w-full" /> : null}
      {data && data.length === 0 ? <p className="text-sm">{t.empty}</p> : null}
      <ul className="flex flex-col gap-3">
        {data?.map((a) => (
          <li key={a.sourceId} className="rounded-lg border border-border p-4">
            <p className="font-medium">
              {a.url ? (
                <a href={a.url} target="_blank" rel="noopener noreferrer" className="text-primary underline-offset-4 hover:underline">
                  {a.name}
                </a>
              ) : (
                a.name
              )}
            </p>
            <p className="text-sm text-muted-foreground">
              {t.license}:{" "}
              {a.licenseUrl ? (
                <a href={a.licenseUrl} target="_blank" rel="noopener noreferrer" className="underline-offset-4 hover:underline">
                  {a.licenseName}
                </a>
              ) : (
                a.licenseName
              )}
              {a.shareAlike ? ` · ${t.shareAlike}` : ""}
            </p>
            <p className="text-xs text-muted-foreground">
              {String(a.publishedCount)} {t.count}
            </p>
          </li>
        ))}
      </ul>
    </main>
  );
}
