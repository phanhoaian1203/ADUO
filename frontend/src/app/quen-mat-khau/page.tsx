import type { Metadata } from "next";
import { ForgotPasswordForm } from "@/features/identity/components/PasswordFlowForms";

export const metadata: Metadata = { title: "Quên mật khẩu" };

export default function Page() {
  return <ForgotPasswordForm />;
}
