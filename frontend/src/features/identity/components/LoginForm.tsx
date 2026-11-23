"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { Button } from "@/components/ui/button";
import { api } from "@/lib/api/client";
import { applyFieldErrors, problemMessage, safeReturnUrl } from "@/lib/api/problem";
import { strings } from "@/lib/strings";
import { useSetMe } from "../hooks";
import { loginSchema, type LoginValues } from "../schemas";
import { AuthCard, FormError, FormNotice } from "./AuthCard";
import { FormField } from "./FormField";
import { GoogleButton } from "./GoogleButton";

const t = strings.auth.login;

export function LoginForm({ returnUrl, sessionExpired, error }: { returnUrl?: string; sessionExpired?: boolean; error?: string }) {
  const router = useRouter();
  const setMe = useSetMe();
  const [formError, setFormError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<LoginValues>({ resolver: zodResolver(loginSchema), defaultValues: { rememberMe: false } });

  const googleError = error ? (strings.auth.google.errors as Record<string, string>)[error] : undefined;
  const target = safeReturnUrl(returnUrl);
  const withReturn = (path: string) => (returnUrl ? `${path}?returnUrl=${encodeURIComponent(target)}` : path);

  async function onSubmit(values: LoginValues) {
    setFormError(null);
    const { data, error } = await api.POST("/api/v1/auth/login", {
      body: { email: values.email.trim(), password: values.password, rememberMe: values.rememberMe },
    });
    if (!data) {
      if (!applyFieldErrors(error, setError)) setFormError(problemMessage(error));
      return;
    }
    setMe(data);
    router.replace(target);
  }

  return (
    <AuthCard
      title={t.title}
      footer={
        <>
          {t.noAccount}{" "}
          <Link href={withReturn("/dang-ky")} className="font-medium text-primary underline-offset-4 hover:underline">
            {t.goRegister}
          </Link>
        </>
      }
    >
      <GoogleButton returnUrl={returnUrl} />
      <form onSubmit={handleSubmit(onSubmit)} noValidate className="flex flex-col gap-4">
        <FormError message={googleError} />
        {sessionExpired ? <FormNotice message={t.sessionExpired} /> : null}
        <FormField
          label={t.email}
          type="email"
          autoComplete="email"
          inputMode="email"
          error={errors.email?.message}
          {...register("email")}
        />
        <FormField
          label={t.password}
          type="password"
          autoComplete="current-password"
          error={errors.password?.message}
          {...register("password")}
        />
        <label className="flex items-center gap-2 text-sm">
          <input type="checkbox" className="size-4" {...register("rememberMe")} />
          {t.remember}
        </label>
        <FormError message={formError} />
        <Button type="submit" size="lg" disabled={isSubmitting}>
          {isSubmitting ? strings.common.loading : t.submit}
        </Button>
        <Link href="/quen-mat-khau" className="text-sm text-primary underline-offset-4 hover:underline">
          {t.forgot}
        </Link>
      </form>
    </AuthCard>
  );
}
