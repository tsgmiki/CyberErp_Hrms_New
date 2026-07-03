import { type FormHTMLAttributes, type ReactNode, forwardRef } from "react";

interface Props extends Omit<FormHTMLAttributes<HTMLFormElement>, "action"> {
  className?: string;
  children: ReactNode;
  submit?: (formData: FormData) => void;
}

const Form = forwardRef<HTMLFormElement, Props>(function Form(
  { children, className = "flex flex-col gap-4", submit, ...rest },
  ref,
) {
  return (
    <form
      ref={ref}
      className={className}
      action={(formData) => {
        submit?.(formData);
      }}
      {...rest}
    >
      {children}
    </form>
  );
});

export default Form;
