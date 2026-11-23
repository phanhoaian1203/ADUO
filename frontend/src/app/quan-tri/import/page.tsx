import type { Metadata } from "next";
import { ImportAdmin } from "@/features/content/components/ImportAdmin";

export const metadata: Metadata = { title: "Import nội dung" };

export default function Page() {
  return <ImportAdmin />;
}
