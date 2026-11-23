"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { api } from "@/lib/api/client";
import { problemMessage, safeReturnUrl } from "@/lib/api/problem";
import { cn } from "@/lib/utils";
import { strings } from "@/lib/strings";
import { useMe, useSetMe, type LearningPurpose, type TargetLevel } from "../hooks";
import { AuthCard, FormError } from "./AuthCard";

const t = strings.onboarding;
const PURPOSES = Object.keys(t.purpose) as LearningPurpose[];
const LEVELS = Object.keys(t.levels) as TargetLevel[];

function Choice({ selected, onClick, children }: { selected: boolean; onClick: () => void; children: React.ReactNode }) {
  return (
    <button
      type="button"
      onClick={onClick}
      aria-pressed={selected}
      className={cn(
        "min-h-12 rounded-lg border px-3 py-2 text-left text-sm transition-colors",
        selected ? "border-primary bg-primary/10 font-medium" : "border-border hover:bg-muted",
      )}
    >
      {children}
    </button>
  );
}

/** Chọn mục tiêu học (UC-05). Cả hai bước đều bỏ qua được; không chọn thì dùng mặc định. */
export function OnboardingForm({ returnUrl }: { returnUrl?: string }) {
  const router = useRouter();
  const setMe = useSetMe();
  const { data: me, isLoading } = useMe();
  const [purpose, setPurpose] = useState<LearningPurpose | null>(null);
  const [level, setLevel] = useState<TargetLevel | null>(null);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const target = safeReturnUrl(returnUrl);

  useEffect(() => {
    if (!isLoading && me === null) router.replace("/dang-nhap");
  }, [isLoading, me, router]);

  async function save(skip: boolean) {
    if (skip || (!purpose && !level)) {
      router.replace(target);
      return;
    }
    setSaving(true);
    setError(null);
    const { data, error: err } = await api.PATCH("/api/v1/me", {
      body: { displayName: null, learningPurpose: purpose, targetLevel: level, avatarMediaId: null },
    });
    setSaving(false);
    if (!data) {
      setError(problemMessage(err));
      return;
    }
    setMe(data);
    router.replace(target);
  }

  return (
    <AuthCard title={t.title}>
      <p className="text-sm text-muted-foreground">{t.intro}</p>
      <div className="flex flex-col gap-2" role="group" aria-label={t.title}>
        {PURPOSES.map((p) => (
          <Choice key={p} selected={purpose === p} onClick={() => setPurpose(p)}>
            {t.purpose[p]}
          </Choice>
        ))}
      </div>
      <h2 className="text-lg font-semibold">{t.levelTitle}</h2>
      <div className="grid grid-cols-1 gap-2 sm:grid-cols-3" role="group" aria-label={t.levelTitle}>
        {LEVELS.map((l) => (
          <Choice key={l} selected={level === l} onClick={() => setLevel(l)}>
            {t.levels[l]}
          </Choice>
        ))}
      </div>
      <FormError message={error} />
      <div className="flex gap-2">
        <Button size="lg" disabled={saving} onClick={() => save(false)}>
          {saving ? strings.common.loading : t.submit}
        </Button>
        <Button size="lg" variant="ghost" disabled={saving} onClick={() => save(true)}>
          {strings.common.skip}
        </Button>
      </div>
    </AuthCard>
  );
}
