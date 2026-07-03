import Spinner, { type SpinnerProps } from "../spinner/spinner";

type LoadingProps = Omit<SpinnerProps, "block" | "fullPage">;

/** Full-area loader (tables, forms) — dual-ring spinner with animated label. */
function Loading({
  size = "lg",
  variant = "ring",
  ...props
}: LoadingProps) {
  return <Spinner block size={size} variant={variant} {...props} />;
}

export default Loading;
