import type { Metadata } from "next";
import { SourcesAdmin } from "@/features/content/components/SourcesAdmin";

export const metadata: Metadata = { title: "Nguồn và giấy phép" };

export default function Page() {
  return <SourcesAdmin />;
}
