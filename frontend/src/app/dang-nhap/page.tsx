import type { Metadata } from "next";
import { Suspense } from "react";
import { LoginForm } from "@/features/identity/components/LoginForm";

export const metadata: Metadata = { title: "Đăng nhập" };

// searchParams là dữ liệu theo từng request, nên phải đọc bên trong <Suspense> (Cache Components của Next.js 16).
async function Content({ searchParams }: Pick<PageProps<"/dang-nhap">, "searchParams">) {
  const sp = await searchParams;
  const returnUrl = typeof sp.returnUrl === "string" ? sp.returnUrl : undefined;
  return <LoginForm returnUrl={returnUrl} sessionExpired={sp["het-phien"] === "1"} error={typeof sp.loi === "string" ? sp.loi : undefined} />;
}

export default function Page({ searchParams }: PageProps<"/dang-nhap">) {
  return (
    <Suspense>
      <Content searchParams={searchParams} />
    </Suspense>
  );
}
