import type { Metadata } from "next";
import { ProfileView } from "@/features/identity/components/ProfileView";

export const metadata: Metadata = { title: "Hồ sơ" };

export default function Page() {
  return <ProfileView />;
}
