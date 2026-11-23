import type { Metadata } from "next";
import { Suspense } from "react";
import { ResetPasswordForm } from "@/features/identity/components/PasswordFlowForms";

export const metadata: Metadata = { title: "Đặt lại mật khẩu" };

async function Content({ searchParams }: Pick<PageProps<"/dat-lai-mat-khau">, "searchParams">) {
  const sp = await searchParams;
  return (
    <ResetPasswordForm
      email={typeof sp.email === "string" ? sp.email : undefined}
      token={typeof sp.token === "string" ? sp.token : undefined}
    />
  );
}

export default function Page({ searchParams }: PageProps<"/dat-lai-mat-khau">) {
  return (
    <Suspense>
      <Content searchParams={searchParams} />
    </Suspense>
  );
}
