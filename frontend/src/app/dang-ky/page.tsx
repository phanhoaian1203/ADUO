import type { Metadata } from "next";
import { Suspense } from "react";
import { RegisterForm } from "@/features/identity/components/RegisterForm";

export const metadata: Metadata = { title: "Đăng ký" };

async function Content({ searchParams }: Pick<PageProps<"/dang-ky">, "searchParams">) {
  const sp = await searchParams;
  return <RegisterForm returnUrl={typeof sp.returnUrl === "string" ? sp.returnUrl : undefined} />;
}

export default function Page({ searchParams }: PageProps<"/dang-ky">) {
  return (
    <Suspense>
      <Content searchParams={searchParams} />
    </Suspense>
  );
}
