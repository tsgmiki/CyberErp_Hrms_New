import Spinner from "../spinner/spinner";

/** Compact inline loader (dropdowns, buttons) — animated icon with orbit ring. */
function SmallLoading() {
  return (
    <div className="flex items-center justify-center py-6">
      <Spinner size="sm" variant="icon" showLabel={false} />
    </div>
  );
}

export default SmallLoading;
