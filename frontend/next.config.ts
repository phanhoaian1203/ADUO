import type { NextConfig } from "next";

/**
 * Cùng origin cho web và API (AR-01):
 * - Production: Caddy chuyển /api/* sang ASP.NET Core trước khi request tới Next.js.
 * - Development: Next.js tự chuyển tiếp /api/* sang API_INTERNAL_URL (mặc định http://localhost:5000),
 *   nên cookie và CSRF ở máy dev hoạt động y như production.
 */
const apiInternalUrl = process.env.API_INTERNAL_URL ?? "http://localhost:5000";

const nextConfig: NextConfig = {
  output: "standalone", // đóng gói Docker gọn (Phase 3 – 01 §3)
  cacheComponents: true,
  partialPrefetching: true,
  poweredByHeader: false,
  turbopack: {
    rules: {
      "*.css": {
        loaders: ["@tailwindcss/turbopack"],
        as: "*.css",
      },
    },
  },
  async rewrites() {
    return [{ source: "/api/:path*", destination: `${apiInternalUrl}/api/:path*` }];
  },
};

export default nextConfig;
