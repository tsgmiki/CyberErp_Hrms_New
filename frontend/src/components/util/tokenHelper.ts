import { jwtDecode } from "jwt-decode";

export const isTokenValid = (token:any) => {
  if (!token) return false;
  try {
    const decoded = jwtDecode(token);
    const currentTime = Date.now() / 1000;
    return decoded?.exp && decoded?.exp > currentTime; // Returns true if not expired
  } catch (error) {
    return false;
  }
};