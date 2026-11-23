"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useState } from "react";
import { SessionWatcher } from "@/features/identity/components/SessionWatcher";

export function Providers({ children }: { children: React.ReactNode }) {
  // Tạo QueryClient trong state để mỗi lần render không tạo mới, và không dùng chung giữa các request phía server.
  const [client] = useState(
    () => new QueryClient({ defaultOptions: { queries: { staleTime: 30_000, refetchOnWindowFocus: false } } }),
  );

  return (
    <QueryClientProvider client={client}>
      <SessionWatcher />
      {children}
    </QueryClientProvider>
  );
}
