"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import Link from "next/link";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { Button, buttonVariants } from "@/components/ui/button";
import { api } from "@/lib/api/client";
import { applyFieldErrors, asProblem, problemMessage } from "@/lib/api/problem";
import { strings } from "@/lib/strings";
import { forgotSchema, resetSchema } from "../schemas";
import { AuthCard, FormError, FormNotice } from "./AuthCard";
import { FormField } from "./FormField";

export function ForgotPasswordForm() {
  const t = strings.auth.forgot;
  const [sent, setSent] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<{ email: string }>({ resolver: zodResolver(forgotSchema) });

  async function onSubmit({ email }: { email: string }) {
    setFormError(null);
    const { response, error } = await api.POST("/api/v1/auth/forgot-password", { body: { email: email.trim() } });
    if (response.ok) setSent(true);
    else setFormError(problemMessage(error));
  }

  return (
    <AuthCard title={t.title}>
      {sent ? (
        <FormNotice message={t.sent} />
      ) : (
        <form onSubmit={handleSubmit(onSubmit)} noValidate className="flex flex-col gap-4">
          <p className="text-sm text-muted-foreground">{t.intro}</p>
          <FormField
            label={strings.auth.login.email}
            type="email"
            autoComplete="email"
            inputMode="email"
            error={errors.email?.message}
            {...register("email")}
          />
          <FormError message={formError} />
          <Button type="submit" size="lg" disabled={isSubmitting}>
            {isSubmitting ? strings.common.loading : t.submit}
          </Button>
        </form>
      )}
      <Link href="/dang-nhap" className="text-sm text-primary underline-offset-4 hover:underline">
        {strings.auth.login.title}
      </Link>
    </AuthCard>
  );
}

export function ResetPasswordForm({ email, token }: { email?: string; token?: string }) {
  const t = strings.auth.reset;
  const [done, setDone] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<{ newPassword: string }>({ resolver: zodResolver(resetSchema) });

  if (!email || !token) {
    return (
      <AuthCard title={t.title}>
        <FormError message={t.missingLink} />
        <Link href="/quen-mat-khau" className="text-sm text-primary underline-offset-4 hover:underline">
          {strings.auth.forgot.title}
        </Link>
      </AuthCard>
    );
  }

  async function onSubmit({ newPassword }: { newPassword: string }) {
    setFormError(null);
    const { response, error } = await api.POST("/api/v1/auth/reset-password", { body: { email: email!, token: token!, newPassword } });
    if (response.ok) {
      setDone(true);
      return;
    }
    if (asProblem(error)?.code === "TOKEN_EXPIRED" || asProblem(error)?.code === "TOKEN_INVALID") {
      setFormError(problemMessage(error));
    } else if (!applyFieldErrors(error, setError)) {
      setFormError(problemMessage(error));
    }
  }

  return (
    <AuthCard title={t.title}>
      {done ? (
        <>
          <FormNotice message={t.done} />
          <Link href="/dang-nhap" className={buttonVariants({ size: "lg" })}>
            {strings.auth.login.submit}
          </Link>
        </>
      ) : (
        <form onSubmit={handleSubmit(onSubmit)} noValidate className="flex flex-col gap-4">
          <FormField
            label={t.newPassword}
            type="password"
            autoComplete="new-password"
            hint={strings.auth.register.passwordHint}
            error={errors.newPassword?.message}
            {...register("newPassword")}
          />
          <FormError message={formError} />
          <Button type="submit" size="lg" disabled={isSubmitting}>
            {isSubmitting ? strings.common.loading : t.submit}
          </Button>
        </form>
      )}
    </AuthCard>
  );
}
