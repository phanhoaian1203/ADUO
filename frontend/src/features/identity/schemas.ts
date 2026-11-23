import { z } from "zod";
import { strings } from "@/lib/strings";

const s = strings.auth;

// Quy tắc phải khớp backend (BR-AUTH-02): backend vẫn kiểm tra lại, đây chỉ để báo lỗi sớm ngay tại trường nhập.
export const passwordRule = z
  .string()
  .min(8, s.passwordWeak)
  .refine((v) => /\p{L}/u.test(v) && /\d/.test(v), s.passwordWeak);

const email = z.string().trim().min(1, s.fieldRequired).email(s.emailInvalid).max(254, s.emailInvalid);

export const loginSchema = z.object({
  email,
  password: z.string().min(1, s.fieldRequired),
  rememberMe: z.boolean(),
});

export const registerSchema = z
  .object({
    displayName: z.string().trim().min(2, s.displayNameLength).max(50, s.displayNameLength),
    email,
    password: passwordRule,
    confirmPassword: z.string().min(1, s.fieldRequired),
    acceptTerms: z.boolean().refine((v) => v, s.termsRequired),
  })
  .refine((v) => v.password === v.confirmPassword, { path: ["confirmPassword"], message: s.passwordMismatch });

export const forgotSchema = z.object({ email });

export const resetSchema = z.object({ newPassword: passwordRule });

export const changePasswordSchema = z.object({
  currentPassword: z.string().min(1, s.fieldRequired),
  newPassword: passwordRule,
});

export const profileSchema = z.object({
  displayName: z.string().trim().min(2, s.displayNameLength).max(50, s.displayNameLength),
});

export const deleteSchema = z.object({
  password: z.string(),
  confirmText: z.string().refine((v) => v.trim().toUpperCase() === "XÓA", strings.profile.deleteWrongConfirm),
});

export type LoginValues = z.infer<typeof loginSchema>;
export type RegisterValues = z.infer<typeof registerSchema>;
