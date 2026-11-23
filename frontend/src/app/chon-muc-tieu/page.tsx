import type { Metadata } from "next";
import { Suspense } from "react";
import { OnboardingForm } from "@/features/identity/components/OnboardingForm";

export const metadata: Metadata = { title: "Chọn mục tiêu học" };

async function Content({ searchParams }: Pick<PageProps<"/chon-muc-tieu">, "searchParams">) {
  const sp = await searchParams;
  return <OnboardingForm returnUrl={typeof sp.returnUrl === "string" ? sp.returnUrl : undefined} />;
}

export default function Page({ searchParams }: PageProps<"/chon-muc-tieu">) {
  return (
    <Suspense>
      <Content searchParams={searchParams} />
    </Suspense>
  );
}
