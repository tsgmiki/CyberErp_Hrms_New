import { z } from "zod";

export const registerSchema = z
  .object({
    fullName: z.string().min(1, "Full name is required"),
    email: z.string().email("Invalid email format"),
    phoneNumber: z
      .string()
      .min(1, "Phone number is required")
      .regex(
        /^(\+?[1-9]\d{1,14}|0\d{9,10})$/,
        "Phone number must be valid (e.g., +1234567890 or 0912063320)",
      ),
    userName: z.string().min(1, "Username is required"),
    password: z.string().min(6, "Password must be at least 6 characters"),
    confirmPassword: z.string().min(1, "Confirm Password is required"),
    tenantName: z.string().min(1, "Tenant name is required"),
    tenantAddress: z.string().optional(),
    tenantPhoneNumber: z
      .string()
      .regex(
        /^(\+?[1-9]\d{1,14}|0\d{9,10})$/,
        "Company phone must be valid (e.g., +1234567890 or 0912063320)",
      )
      .optional()
      .or(z.literal("")),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords don't match",
    path: ["confirmPassword"],
  });

export type RegisterFormData = z.infer<typeof registerSchema>;
