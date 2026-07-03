import { BookA } from "lucide-react";

function Manual() {
  return (
    <a
      href="/manual"
      className="w-8 h-8 flex items-center justify-center rounded-lg text-muted-foreground hover:text-foreground hover:bg-muted transition-colors"
      title="Manual"
      aria-label="Manual"
    >
      <BookA className="w-4 h-4" />
    </a>
  );
}

export default Manual;
