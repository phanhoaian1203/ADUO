import type { Metadata } from "next";
import { AdminUsers } from "@/features/identity/components/AdminUsers";

export const metadata: Metadata = { title: "Quản lý người dùng" };

export default function Page() {
  return <AdminUsers />;
}
