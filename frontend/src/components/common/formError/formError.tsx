function FormError({ message }: { message: string }) {
  return (
    <p className="rounded-md border border-error/20 bg-error/10 px-2 py-1 text-xs text-error">
      {message}
    </p>
  );
}

export default FormError;
