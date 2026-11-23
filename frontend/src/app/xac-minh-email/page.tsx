import type { Metadata } from "next";
import { Suspense } from "react";
import { ConfirmEmail } from "@/features/identity/components/ConfirmEmail";

export const metadata: Metadata = { title: "Xác minh email" };

async function Content({ searchParams }: Pick<PageProps<"/xac-minh-email">, "searchParams">) {
  const sp = await searchParams;
  return (
    <ConfirmEmail
      userId={typeof sp.userId === "string" ? sp.userId : undefined}
      token={typeof sp.token === "string" ? sp.token : undefined}
    />
  );
}

export default function Page({ searchParams }: PageProps<"/xac-minh-email">) {
  return (
    <Suspense>
      <Content searchParams={searchParams} />
    </Suspense>
  );
}
