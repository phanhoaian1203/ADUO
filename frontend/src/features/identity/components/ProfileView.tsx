"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { api } from "@/lib/api/client";
import { applyFieldErrors, asProblem, problemMessage } from "@/lib/api/problem";
import { strings } from "@/lib/strings";
import { useMe, useSetMe, type Me } from "../hooks";
import { changePasswordSchema, deleteSchema, profileSchema } from "../schemas";
import { FormError } from "./AuthCard";
import { FormField } from "./FormField";

const t = strings.profile;

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section className="flex flex-col gap-3 border-t border-border pt-6">
      <h2 className="text-lg font-semibold">{title}</h2>
      {children}
    </section>
  );
}

function ProfileForm({ me }: { me: Me }) {
  const setMe = useSetMe();
  const [formError, setFormError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting, isDirty },
  } = useForm<{ displayName: string }>({ resolver: zodResolver(profileSchema), defaultValues: { displayName: me.displayName } });

  async function onSubmit({ displayName }: { displayName: string }) {
    setFormError(null);
    const { data, error } = await api.PATCH("/api/v1/me", {
      body: { displayName: displayName.trim(), learningPurpose: null, targetLevel: null, avatarMediaId: null },
    });
    if (!data) {
      if (!applyFieldErrors(error, setError)) setFormError(problemMessage(error));
      return;
    }
    setMe(data);
    toast.success(strings.common.saved);
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate className="flex flex-col gap-4">
      <FormField label={t.displayName} error={errors.displayName?.message} {...register("displayName")} />
      <FormField label={t.email} value={me.email} readOnly disabled hint={t.emailNote} />
      <p className="text-sm text-muted-foreground">{me.emailConfirmed ? t.verified : t.unverified}</p>
      <FormError message={formError} />
      <Button type="submit" disabled={isSubmitting || !isDirty} className="self-start">
        {strings.common.save}
      </Button>
    </form>
  );
}

function ChangePasswordForm({ hasPassword }: { hasPassword: boolean }) {
  const qc = useQueryClient();
  const [formError, setFormError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    setError,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<{ currentPassword: string; newPassword: string }>({ resolver: zodResolver(changePasswordSchema) });

  async function onSubmit(v: { currentPassword: string; newPassword: string }) {
    setFormError(null);
    const { response, error } = await api.POST("/api/v1/me/change-password", { body: v });
    if (response.ok) {
      reset();
      toast.success(t.passwordChanged);
      qc.invalidateQueries({ queryKey: ["sessions"] });
      return;
    }
    if (asProblem(error)?.code === "WRONG_PASSWORD") setError("currentPassword", { message: problemMessage(error) });
    else if (!applyFieldErrors(error, setError)) setFormError(problemMessage(error));
  }

  if (!hasPassword) return null; // tài khoản chỉ đăng nhập bằng Google: chưa hỗ trợ đặt mật khẩu ở giao diện

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate className="flex flex-col gap-4">
      <FormField
        label={t.currentPassword}
        type="password"
        autoComplete="current-password"
        error={errors.currentPassword?.message}
        {...register("currentPassword")}
      />
      <FormField
        label={t.newPassword}
        type="password"
        autoComplete="new-password"
        hint={strings.auth.register.passwordHint}
        error={errors.newPassword?.message}
        {...register("newPassword")}
      />
      <FormError message={formError} />
      <Button type="submit" disabled={isSubmitting} className="self-start">
        {t.changePassword}
      </Button>
    </form>
  );
}

function SessionsList() {
  const qc = useQueryClient();
  const router = useRouter();
  const setMe = useSetMe();
  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ["sessions"],
    queryFn: async () => {
      const { data: list } = await api.GET("/api/v1/me/sessions");
      if (!list) throw new Error("sessions failed");
      return list;
    },
  });

  async function revoke(id: string) {
    const { response } = await api.DELETE("/api/v1/me/sessions/{sessionId}", { params: { path: { sessionId: id } } });
    if (response.ok) {
      toast.success(t.revoked);
      qc.invalidateQueries({ queryKey: ["sessions"] });
    }
  }

  async function logoutAll() {
    await api.POST("/api/v1/auth/logout-all");
    setMe(null);
    router.replace("/dang-nhap");
  }

  if (isLoading) return <Skeleton className="h-20 w-full" />;
  if (isError || !data)
    return (
      <Button variant="outline" onClick={() => refetch()} className="self-start">
        {strings.common.retry}
      </Button>
    );

  return (
    <div className="flex flex-col gap-3">
      <p className="text-sm text-muted-foreground">{t.sessionsIntro}</p>
      <ul className="flex flex-col gap-2">
        {data.map((s) => (
          <li key={s.id} className="flex items-center justify-between gap-3 rounded-lg border border-border p-3">
            <div className="min-w-0">
              <p className="truncate text-sm font-medium">
                {s.device ?? "?"} {s.current ? <span className="text-primary">· {t.currentDevice}</span> : null}
              </p>
              <p className="text-xs text-muted-foreground">
                {t.lastUsed}: {new Date(s.lastUsedAt).toLocaleString("vi-VN")}
              </p>
            </div>
            {s.current ? null : (
              <Button variant="outline" size="sm" onClick={() => revoke(s.id)}>
                {t.revoke}
              </Button>
            )}
          </li>
        ))}
      </ul>
      <Button variant="destructive" onClick={logoutAll} className="self-start">
        {t.logoutAll}
      </Button>
    </div>
  );
}

function DeleteAccountForm({ hasPassword }: { hasPassword: boolean }) {
  const router = useRouter();
  const setMe = useSetMe();
  const [formError, setFormError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<{ password: string; confirmText: string }>({ resolver: zodResolver(deleteSchema) });

  async function onSubmit(v: { password: string; confirmText: string }) {
    setFormError(null);
    const { response, error } = await api.POST("/api/v1/me/delete", { body: v });
    if (response.ok) {
      setMe(null);
      router.replace("/");
      return;
    }
    if (asProblem(error)?.code === "WRONG_PASSWORD") setError("password", { message: problemMessage(error) });
    else setFormError(problemMessage(error));
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate className="flex flex-col gap-4">
      <p className="text-sm text-muted-foreground">{t.dangerIntro}</p>
      <FormField label={t.confirmText} error={errors.confirmText?.message} autoComplete="off" {...register("confirmText")} />
      {hasPassword ? (
        <FormField
          label={strings.auth.login.password}
          type="password"
          autoComplete="current-password"
          error={errors.password?.message}
          {...register("password")}
        />
      ) : null}
      <FormError message={formError} />
      <Button type="submit" variant="destructive" disabled={isSubmitting} className="self-start">
        {t.deleteSubmit}
      </Button>
    </form>
  );
}

export function ProfileView() {
  const router = useRouter();
  const { data: me, isLoading } = useMe();

  useEffect(() => {
    if (!isLoading && me === null) router.replace("/dang-nhap?returnUrl=%2Fho-so");
  }, [isLoading, me, router]);

  if (isLoading || !me) return <Skeleton className="mx-auto mt-10 h-64 w-full max-w-md" />;

  return (
    <main className="mx-auto flex w-full max-w-md flex-1 flex-col gap-6 px-4 py-10">
      <h1 className="text-2xl font-semibold tracking-tight">{t.title}</h1>
      <ProfileForm me={me} />
      <Section title={t.changePassword}>
        <ChangePasswordForm hasPassword={me.hasPassword} />
      </Section>
      <Section title={t.sessionsTitle}>
        <SessionsList />
      </Section>
      <Section title={t.dangerTitle}>
        <DeleteAccountForm hasPassword={me.hasPassword} />
      </Section>
    </main>
  );
}
