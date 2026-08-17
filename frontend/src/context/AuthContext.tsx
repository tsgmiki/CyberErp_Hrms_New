import React, {
  createContext,
  useState,
  useEffect,
  useContext,
  useCallback,
  useRef,
  type ReactNode,
} from "react";
import { AUTH_ERROR_EVENT } from "@/utils/apiClient";
import getLoginStatus from "@/services/auth/loginStatus";

interface User {
  id: string;
  fullName: string;
  userName: string;
  email: string;
  isPublicUser: boolean;
  accessToken: string;
}

interface AuthContextType {
  user: User | null;
  loading: boolean;
  login: (userData: User) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

// Session storage key for user data
const SESSION_USER_KEY = "sessionUser";

/**
 * Normalises whatever the API returned into the `User` shape above.
 *
 * ⚠️ THE TWO ENDPOINTS DISAGREE. `auth/login` returns `id` / `fullName`, but the session probe
 * `auth/loginStatus` returns **`userId` / `name`** (CurrentUserResult). Both were cast straight to
 * `User`, so on any session RESTORED from the cookie — which is every deep-link in from the Home
 * portal, and every reload without sessionStorage — `user.id` and `user.fullName` were `undefined`.
 *
 * That silently broke anything keyed on them: the header fell back to "User" instead of the person's
 * name, `useFormLayoutPreference` had already grown a local `?? user?.userName ?? "anon"` workaround,
 * and PreferencesContext (which loads when `user.id` appears) never loaded at all — so HRMS ignored
 * the user's saved theme and language.
 *
 * Normalising here fixes all of them in one place. Accepts either shape; anything already correct
 * passes through unchanged.
 */
type ApiUser = Partial<User> & { userId?: string; name?: string };

const normalizeUser = (raw: ApiUser | null | undefined): User | null => {
  if (!raw) return null;
  const id = raw.id ?? raw.userId ?? "";
  const fullName = raw.fullName ?? raw.name ?? "";
  return {
    ...(raw as User),
    id,
    fullName,
    userName: raw.userName ?? "",
    email: raw.email ?? "",
    isPublicUser: raw.isPublicUser ?? false,
    accessToken: raw.accessToken ?? "",
  };
};

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(() => {
    // Try to restore user from sessionStorage on initial load
    try {
      const stored = sessionStorage.getItem(SESSION_USER_KEY);
      return stored ? JSON.parse(stored) : null;
    } catch {
      return null;
    }
  });
  const [loading, setLoading] = useState(true);
  const checkLoginId = useRef(0);

  const logout = useCallback(() => {
    checkLoginId.current += 1;
    setUser(null);
    sessionStorage.removeItem(SESSION_USER_KEY);
    setLoading(false);
  }, []);

  const login = useCallback((userData: User) => {
    checkLoginId.current += 1;
    const normalized = normalizeUser(userData);
    setUser(normalized);
    // Store in sessionStorage for persistence across refreshes
    sessionStorage.setItem(SESSION_USER_KEY, JSON.stringify(normalized));
    setLoading(false);
  }, []);

  // Check login status on mount by calling the server
  useEffect(() => {
    let isMounted = true;
    const initCheckId = checkLoginId.current;

    const checkLoginStatus = async () => {
      // If we already have user from sessionStorage, don't overwrite with null
      // This handles the case where cookie isn't being sent but user has valid session
      const existingUser = sessionStorage.getItem(SESSION_USER_KEY);
      
      try {
        // Verify session with server using cookies
        const response = await getLoginStatus();
        if (isMounted && checkLoginId.current === initCheckId) {
          if (response) {
            // ⚠️ Normalised: this endpoint answers with userId/name, not id/fullName.
            const validatedUser = normalizeUser(response as unknown as ApiUser);
            setUser(validatedUser);
            // Update sessionStorage with fresh data
            sessionStorage.setItem(SESSION_USER_KEY, JSON.stringify(validatedUser));
          } else if (!existingUser) {
            // Only set user to null if there was no existing session
            setUser(null);
          }
          // If response is null but existingUser exists, keep the existing user
        }
      } catch (error) {
        if (isMounted && checkLoginId.current === initCheckId) {
          console.error("Error checking login status:", error);
          // Only clear user if there was no existing session
          if (!existingUser) {
            setUser(null);
          }
        }
      } finally {
        if (isMounted && checkLoginId.current === initCheckId) {
          setLoading(false);
        }
      }
    };

    checkLoginStatus();

    return () => {
      isMounted = false;
    };
  }, []);

  // Listen for authentication errors from API client (e.g., 401 responses)
  useEffect(() => {
    const handleAuthError = () => {
      console.log("Session expired, logging out...");
      logout();
    };

    window.addEventListener(AUTH_ERROR_EVENT, handleAuthError);

    return () => {
      window.removeEventListener(AUTH_ERROR_EVENT, handleAuthError);
    };
  }, [logout]);

  return (
    <AuthContext.Provider value={{ user, loading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};

export default AuthContext;
