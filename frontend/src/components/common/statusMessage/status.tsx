import { useEffect } from "react";
import { toast } from "@/components/common/toast";

export function ZodErrors({ error }: { error?: string[] }) {
  if (!error || !Array.isArray(error)) return null;
  return error.map((err: string, index: number) => (
    <div
      key={index}
      className="mt-1.5 flex animate-in items-center gap-2 text-xs font-medium text-error duration-200 fade-in slide-in-from-top-1"
    >
      <div className="h-1 w-1 shrink-0 rounded-full bg-error" />
      {err}
    </div>
  ));
}

export function StatusMessage({
  status,
  message,
  formState,
  id,
}: {
  status: string;
  message: string;
  formState: unknown;
  id?: string;
}) {
  useEffect(() => {
    if (!status || !message) return;

    if (status === "success") {
      toast.success(message);
    } else if (status === "error") {
      toast.error(message);
    }
  }, [status, message, formState, id]);

  return null;
}
