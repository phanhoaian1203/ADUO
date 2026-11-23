"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { Button, buttonVariants } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { api } from "@/lib/api/client";
import { strings } from "@/lib/strings";
import { useMe, useSetMe } from "../hooks";

/** Thanh đăng nhập nhỏ: khách thấy Đăng nhập/Đăng ký, người đã đăng nhập thấy tên + Đăng xuất. Kèm cảnh báo chưa xác minh email. */
export function AuthNav() {
  const router = useRouter();
  const setMe = useSetMe();
  const { data: me, isLoading } = useMe();

  async function logout() {
    await api.POST("/api/v1/auth/logout");
    setMe(null);
    router.refresh();
  }

  if (isLoading) return <Skeleton className="h-8 w-40" />;

  if (!me) {
    return (
      <div className="flex items-center gap-2">
        <Link href="/dang-nhap" className={buttonVariants({ variant: "outline" })}>
          {strings.auth.login.submit}
        </Link>
        <Link href="/dang-ky" className={buttonVariants()}>
          {strings.auth.register.submit}
        </Link>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-2">
      <div className="flex flex-wrap items-center gap-2">
        <Link href="/ho-so" className="text-sm font-medium underline-offset-4 hover:underline">
          {me.displayName}
        </Link>
        {me.isAdmin ? (
          <Link href="/quan-tri/nguoi-dung" className={buttonVariants({ variant: "ghost", size: "sm" })}>
            {strings.admin.usersTitle}
          </Link>
        ) : null}
        <Button variant="outline" size="sm" onClick={logout}>
          {strings.auth.logout}
        </Button>
      </div>
      {me.emailConfirmed ? null : (
        <p role="status" className="rounded-lg bg-muted px-3 py-2 text-xs">
          {strings.auth.banner.unverified}
        </p>
      )}
    </div>
  );
}
