import type { Metadata } from "next";
import { Attributions } from "@/features/content/components/Attributions";

export const metadata: Metadata = { title: "Nguồn dữ liệu và giấy phép" };

export default function Page() {
  return <Attributions />;
}
