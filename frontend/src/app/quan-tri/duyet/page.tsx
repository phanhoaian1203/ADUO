import type { Metadata } from "next";
import { ReviewQueueAdmin } from "@/features/content/components/ReviewQueueAdmin";

export const metadata: Metadata = { title: "Hàng đợi duyệt" };

export default function Page() {
  return <ReviewQueueAdmin />;
}
