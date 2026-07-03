import { z } from "zod";

export const loginSchema = z.object({
  userName: z.string().min(1, "User name is required"),
  password: z.string().min(1, "Password is required"),
});

export type LoginFormData = z.infer<typeof loginSchema>;
