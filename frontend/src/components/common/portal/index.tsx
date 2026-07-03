import { useEffect, useRef, type ReactNode } from "react";
import { createPortal } from "react-dom";

interface PortalProps {
  children: ReactNode;
  selector?: string;
}

export default function Portal({ children, selector = "body" }: PortalProps) {
  const elementRef = useRef<HTMLElement | null>(null);

  useEffect(() => {
    elementRef.current = document.querySelector(selector);
  }, [selector]);

  if (!elementRef.current) {
    return null;
  }

  return createPortal(children, elementRef.current);
}
