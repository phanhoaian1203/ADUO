import type { Metadata, Viewport } from "next";
import { Be_Vietnam_Pro } from "next/font/google";
import { Toaster } from "@/components/ui/sonner";
import { Providers } from "./providers";
import { strings } from "@/lib/strings";
import "./globals.css";

// Be Vietnam Pro: thiết kế cho tiếng Việt, hỗ trợ ä ö ü ß (Phase 3 – 06 §2). next/font tự host, không gọi Google lúc chạy.
const beVietnam = Be_Vietnam_Pro({
  variable: "--font-be-vietnam",
  subsets: ["latin", "vietnamese"],
  weight: ["400", "500", "600", "700"],
  display: "swap",
});

export const metadata: Metadata = {
  title: { default: strings.app.name, template: `%s · ${strings.app.name}` },
  description: strings.app.tagline,
};

export const viewport: Viewport = {
  width: "device-width",
  initialScale: 1,
  themeColor: "#1f4fd8",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="vi" className={`${beVietnam.variable} h-full antialiased`}>
      <body className="flex min-h-full flex-col">
        <Providers>{children}</Providers>
        <Toaster position="top-center" />
      </body>
    </html>
  );
}
