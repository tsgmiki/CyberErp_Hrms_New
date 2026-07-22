"use client";
import { Component, type ErrorInfo, type ReactNode } from "react";
import { useLocation } from "react-router-dom";
import { AlertTriangle } from "lucide-react";

export interface FallbackProps {
  error: Error | null;
  reset: () => void;
}

/** Default fallback — deliberately minimal (no i18n/store deps, since those may be what failed). */
function DefaultFallback({ error, reset }: FallbackProps) {
  return (
    <div className="flex min-h-[50vh] w-full flex-col items-center justify-center gap-4 p-6 text-center">
      <div className="flex h-14 w-14 items-center justify-center rounded-full bg-error/10 text-error">
        <AlertTriangle className="h-7 w-7" />
      </div>
      <div className="space-y-1">
        <h1 className="text-lg font-semibold text-foreground">Something went wrong</h1>
        <p className="mx-auto max-w-md text-sm text-muted">
          This section ran into an unexpected error. You can try again, or reload the page.
        </p>
      </div>
      {error?.message ? (
        <pre className="max-w-lg overflow-auto rounded-lg border border-border bg-card px-3 py-2 text-left text-xs text-muted">
          {error.message}
        </pre>
      ) : null}
      <div className="flex flex-wrap items-center justify-center gap-2">
        <button
          type="button"
          onClick={reset}
          className="rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-on-accent transition-opacity hover:opacity-90"
        >
          Try again
        </button>
        <button
          type="button"
          onClick={() => window.location.reload()}
          className="rounded-lg border border-border bg-card px-4 py-2 text-sm font-medium text-foreground transition-colors hover:bg-secondary"
        >
          Reload page
        </button>
      </div>
    </div>
  );
}

interface ErrorBoundaryProps {
  children: ReactNode;
  /** When any value here changes (e.g. the route), the boundary clears its error so a fresh render is attempted. */
  resetKeys?: unknown[];
  fallback?: (props: FallbackProps) => ReactNode;
}

interface ErrorBoundaryState {
  error: Error | null;
}

function shallowEqual(a?: unknown[], b?: unknown[]) {
  if (a === b) return true;
  if (!a || !b || a.length !== b.length) return false;
  return a.every((value, index) => Object.is(value, b[index]));
}

/**
 * Catches render/lifecycle errors in its subtree and shows a fallback instead of letting the error
 * unmount the whole React tree (a blank page). Error boundaries MUST be class components.
 */
export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  state: ErrorBoundaryState = { error: null };

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    // No external logging service is wired; surface the error + component stack to the console.
    console.error("Uncaught render error:", error, info.componentStack);
  }

  componentDidUpdate(prevProps: ErrorBoundaryProps) {
    if (this.state.error && !shallowEqual(prevProps.resetKeys, this.props.resetKeys)) {
      this.setState({ error: null });
    }
  }

  reset = () => this.setState({ error: null });

  render() {
    if (this.state.error) {
      const render = this.props.fallback ?? ((props: FallbackProps) => <DefaultFallback {...props} />);
      return render({ error: this.state.error, reset: this.reset });
    }
    return this.props.children;
  }
}

/**
 * Error boundary that also clears itself whenever the route changes — so if a page crashes, the
 * user can simply navigate elsewhere (via the still-rendered sidebar) and the error is cleared.
 * Must be rendered inside a Router.
 */
export function RouteErrorBoundary({
  children,
  fallback,
}: {
  children: ReactNode;
  fallback?: (props: FallbackProps) => ReactNode;
}) {
  const location = useLocation();
  return (
    <ErrorBoundary resetKeys={[location.pathname]} fallback={fallback}>
      {children}
    </ErrorBoundary>
  );
}

export default ErrorBoundary;
