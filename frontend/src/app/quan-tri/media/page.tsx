import type { Metadata } from "next";
import { MediaAdmin } from "@/features/content/components/MediaAdmin";

export const metadata: Metadata = { title: "Media" };

export default function Page() {
  return <MediaAdmin />;
}
