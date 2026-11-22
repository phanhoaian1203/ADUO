import { ApiStatus } from "@/features/system/components/ApiStatus";
import { strings } from "@/lib/strings";

// Trang tạm của Foundation: chứng minh web → proxy /api → API → DB chạy thông suốt.
// Sẽ được thay bằng Landing page thật khi làm module Catalog (Phase 3 – 05 §3.1).
export default function Home() {
  return (
    <main className="mx-auto flex w-full max-w-2xl flex-1 flex-col gap-6 px-4 py-12">
      <header>
        <h1 className="text-3xl font-semibold tracking-tight">{strings.app.name}</h1>
        <p className="mt-2 text-muted-foreground">{strings.app.tagline}</p>
      </header>

      <section aria-labelledby="status-title" className="flex flex-col gap-3">
        <h2 id="status-title" className="text-lg font-semibold">
          {strings.system.apiStatusTitle}
        </h2>
        <ApiStatus />
      </section>
    </main>
  );
}
