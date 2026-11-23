"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { Button } from "@/components/ui/button";
import { api } from "@/lib/api/client";
import { applyFieldErrors, asProblem, problemMessage, safeReturnUrl } from "@/lib/api/problem";
import { strings } from "@/lib/strings";
import { useSetMe } from "../hooks";
import { registerSchema, type RegisterValues } from "../schemas";
import { AuthCard, FormError } from "./AuthCard";
import { FormField } from "./FormField";
import { GoogleButton } from "./GoogleButton";

const t = strings.auth.register;

// Phiên bản văn bản pháp lý người dùng đồng ý. Phải khớp bản đang đăng trên trang Điều khoản/Quyền riêng tư (BR-AUTH-04).
const TERMS_VERSION = "2026-11";
const PRIVACY_VERSION = "2026-11";

export function RegisterForm({ returnUrl }: { returnUrl?: string }) {
  const router = useRouter();
  const setMe = useSetMe();
  const [formError, setFormError] = useState<string | null>(null);
  const [emailTaken, setEmailTaken] = useState(false);
  const {
    register,
    handleSubmit,
    setError,
    control,
    formState: { errors, isSubmitting },
  } = useForm<RegisterValues>({ resolver: zodResolver(registerSchema), defaultValues: { acceptTerms: false } });

  const accepted = useWatch({ control, name: "acceptTerms" });
  const target = safeReturnUrl(returnUrl);

  async function onSubmit(values: RegisterValues) {
    setFormError(null);
    setEmailTaken(false);
    const { data, error } = await api.POST("/api/v1/auth/register", {
      body: {
        displayName: values.displayName.trim(),
        email: values.email.trim(),
        password: values.password,
        acceptTerms: values.acceptTerms,
        termsVersion: TERMS_VERSION,
        privacyVersion: PRIVACY_VERSION,
        minorGuardianConfirmed: false,
      },
    });
    if (!data) {
      if (asProblem(error)?.code === "EMAIL_TAKEN") {
        setEmailTaken(true);
        setError("email", { message: t.emailTaken });
      } else if (!applyFieldErrors(error, setError)) {
        setFormError(problemMessage(error));
      }
      return;
    }
    setMe(data);
    // Người mới: chọn mục tiêu trước (bỏ qua được), rồi mới vào nơi họ định đến.
    router.replace(`/chon-muc-tieu?returnUrl=${encodeURIComponent(target)}`);
  }

  return (
    <AuthCard
      title={t.title}
      footer={
        <>
          {t.haveAccount}{" "}
          <Link href="/dang-nhap" className="font-medium text-primary underline-offset-4 hover:underline">
            {t.goLogin}
          </Link>
        </>
      }
    >
      <GoogleButton returnUrl={returnUrl} />
      <form onSubmit={handleSubmit(onSubmit)} noValidate className="flex flex-col gap-4">
        <FormField label={t.displayName} autoComplete="nickname" error={errors.displayName?.message} {...register("displayName")} />
        <FormField
          label={t.email}
          type="email"
          autoComplete="email"
          inputMode="email"
          error={errors.email?.message}
          {...register("email")}
        />
        {emailTaken ? (
          <p className="-mt-2 text-sm">
            <Link href="/dang-nhap" className="font-medium text-primary underline-offset-4 hover:underline">
              {strings.auth.login.title}
            </Link>
          </p>
        ) : null}
        <FormField
          label={t.password}
          type="password"
          autoComplete="new-password"
          hint={t.passwordHint}
          error={errors.password?.message}
          {...register("password")}
        />
        <FormField
          label={t.confirmPassword}
          type="password"
          autoComplete="new-password"
          error={errors.confirmPassword?.message}
          {...register("confirmPassword")}
        />
        <div className="flex flex-col gap-1">
          <label className="flex items-start gap-2 text-sm">
            <input type="checkbox" className="mt-0.5 size-4 shrink-0" {...register("acceptTerms")} />
            <span>{t.acceptTerms}</span>
          </label>
          {errors.acceptTerms ? (
            <p role="alert" className="text-xs text-destructive">
              {errors.acceptTerms.message}
            </p>
          ) : null}
        </div>
        <FormError message={formError} />
        <Button type="submit" size="lg" disabled={isSubmitting || !accepted}>
          {isSubmitting ? strings.common.loading : t.submit}
        </Button>
      </form>
    </AuthCard>
  );
}
