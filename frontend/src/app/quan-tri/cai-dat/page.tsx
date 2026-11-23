import type { Metadata } from "next";
import { SettingsAdmin } from "@/features/content/components/SettingsAdmin";

export const metadata: Metadata = { title: "Cài đặt" };

export default function Page() {
  return <SettingsAdmin />;
}
