"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { Skeleton } from "@/components/ui/skeleton";
import { useMe } from "@/features/identity/hooks";
import { strings } from "@/lib/strings";
import { cn } from "@/lib/utils";

const NAV = [
  { key: "users", href: "/quan-tri/nguoi-dung" },
  { key: "sources", href: "/quan-tri/nguon" },
  { key: "media", href: "/quan-tri/media" },
  { key: "imports", href: "/quan-tri/import" },
  { key: "review", href: "/quan-tri/duyet" },
  { key: "settings", href: "/quan-tri/cai-dat" },
] as const;

/**
 * Khung chung của các trang quản trị: kiểm tra đăng nhập + quyền Admin (chỉ để ẩn/hiện, server luôn kiểm tra lại)
 * và thanh điều hướng giữa các trang.
 */
export function AdminShell({
  current,
  title,
  intro,
  children,
}: {
  current: (typeof NAV)[number]["key"];
  title: string;
  intro?: string;
  children: React.ReactNode;
}) {
  const router = useRouter();
  const { data: me, isLoading } = useMe();

  useEffect(() => {
    if (!isLoading && me === null) router.replace(`/dang-nhap?returnUrl=${encodeURIComponent(NAV.find((n) => n.key === current)!.href)}`);
  }, [isLoading, me, router, current]);

  if (isLoading || !me) return <Skeleton className="mx-auto mt-10 h-40 w-full max-w-3xl" />;
  if (!me.isAdmin) return <p className="mx-auto mt-10 max-w-3xl px-4">{strings.admin.forbidden}</p>;

  return (
    <main className="mx-auto flex w-full max-w-3xl flex-1 flex-col gap-4 px-4 py-8">
      <nav aria-label="Quản trị" className="flex flex-wrap gap-1 border-b border-border pb-2">
        {NAV.map((n) => (
          <Link
            key={n.key}
            href={n.href}
            aria-current={n.key === current ? "page" : undefined}
            className={cn(
              "rounded-lg px-3 py-1.5 text-sm",
              n.key === current ? "bg-primary text-primary-foreground" : "hover:bg-muted",
            )}
          >
            {strings.admin.nav[n.key]}
          </Link>
        ))}
      </nav>
      <h1 className="text-2xl font-semibold tracking-tight">{title}</h1>
      {intro ? <p className="text-sm text-muted-foreground">{intro}</p> : null}
      {children}
    </main>
  );
}
