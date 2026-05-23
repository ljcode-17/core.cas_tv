import PropTypes from "prop-types";
import {
  createContext,
  useEffect,
  useReducer,
  useCallback,
  useMemo,
} from "react";

import { toast } from "sonner";

import axios from "@shared/utils/axios";
import localStorageAvailable from "@shared/utils/localStorageAvailable";
import {
  GETSESSION,
  isValidToken,
  jwtDecode,
  setSession,
  setUserSession,
} from "@features/auth/utils";
import { AUTH_APIs, HOST_API_KEY, PATH_APIs } from "@shared/config/api.config";
import { ROUTE_PATHS } from "@shared/config/menu.config";

// ----------------------------------------------------------------------

const initialState = {
  isInitialized: false,
  isAuthenticated: false,
  user: null,
  menu: null,
  paths: ROUTE_PATHS, // Default to all known static routes initially to prevent blocking
};

const reducer = (state, action) => {
  if (action.type === "INITIAL") {
    return {
      isInitialized: action.payload.isInitialized,
      isAuthenticated: action.payload.isAuthenticated,
      user: action.payload.user,
      menu: action.payload.menu,
      paths: action.payload.paths,
    };
  }
  if (action.type === "LOGIN") {
    return {
      ...state,
      isAuthenticated: true,
      token: action.payload.token,
      user: action.payload.user ?? state.user,
    };
  }
  if (action.type === "LOGOUT") {
    return {
      ...state,
      isAuthenticated: false,
      user: null,
      menu: null,
      paths: null,
    };
  }
  return state;
};

// ----------------------------------------------------------------------

export const AuthContext = createContext(null);

AuthProvider.propTypes = {
  children: PropTypes.node,
};

export function AuthProvider({ children }) {
  const [state, dispatch] = useReducer(reducer, initialState);

  const storageAvailable = localStorageAvailable();

  const initialize = useCallback(async () => {
    try {
      const params = new URLSearchParams(window.location.search);
      const urlToken = params.get("accessToken") || params.get("token");

      if (urlToken) {
        setSession(urlToken);

        const urlUser = params.get("user");
        if (urlUser) {
          try {
            setUserSession(JSON.parse(decodeURIComponent(urlUser)));
          } catch {
            setUserSession(decodeURIComponent(urlUser));
          }
        }

        try {
          window.history.replaceState(
            {},
            document.title,
            window.location.pathname + window.location.hash
          );
        } catch {
          // Ignore history cleanup failures.
        }
      }
    } catch {
      // Ignore malformed URL query strings.
    }

    try {
      const accessToken = storageAvailable
        ? localStorage.getItem("accessToken")
        : "";

      if (accessToken && isValidToken(accessToken)) {
        setSession(accessToken);

        const response = await axios.get(AUTH_APIs.VERIFY.TOKEN, {
          headers: {
            authorization: GETSESSION(),
            "x-api-key": HOST_API_KEY,
          },
        });
        console.log("[Auth] Token verification response:", response);
        console.log("[Auth] response.data:", response.data);
        console.log("[Auth] response.data keys:", response.data ? Object.keys(response.data) : 'null');

        const { data: menu } = await axios.get(PATH_APIs.MENU.LIST, {
          headers: {
            authorization: GETSESSION(),
            "x-api-key": HOST_API_KEY,
          },
        });

        const { data: routePaths } = await axios.get(PATH_APIs.PATH.LIST, {
          headers: {
            authorization: GETSESSION(),
            "x-api-key": HOST_API_KEY,
          },
        });

        // If API returned no resultData (stripped by WhenWritingNull when null),
        // fall back to reading user info directly from the JWT claims.
        let user = response.data.resultData;
        if (!user) {
          try {
            const decoded = jwtDecode(accessToken);
            console.log("[Auth] Falling back to JWT claims:", decoded);
            const rawName = decoded.name || decoded.unique_name || decoded.given_name || "";
            const nameParts = rawName.trim().split(" ");
            user = {
              userId: decoded.nameid || decoded.sub || decoded.user_id || 0,
              firstName: decoded.given_name || nameParts[0] || "",
              lastName: decoded.family_name || nameParts.slice(1).join(" ") || "",
              fullName: rawName,
              email: decoded.email || decoded.unique_name || "",
              departmentName: "",
            };
          } catch (e) {
            console.warn("[Auth] Failed to decode JWT claims:", e);
          }
        }
        if (user) {
          localStorage.setItem("user", JSON.stringify(user));
        }
        console.log("[Auth] Final user object:", user);


        // Temporarily merge backend paths with static ROUTE_PATHS so local Seat Assignment features are not blocked by the Guard
        const backendPaths = routePaths?.resultData || [];
        const paths = [...new Set([...backendPaths, ...ROUTE_PATHS])];

        // Ensure menu has the expected shape: { leftSideBarMenus: [] }
        let normalizedMenu = menu ?? { leftSideBarMenus: [] };
        if (!Array.isArray(normalizedMenu.leftSideBarMenus)) {
          // Attempt to coerce objects -> array, otherwise fall back to empty array
          if (normalizedMenu.leftSideBarMenus && typeof normalizedMenu.leftSideBarMenus === "object") {
            const vals = Object.values(normalizedMenu.leftSideBarMenus).filter(Boolean);
            normalizedMenu.leftSideBarMenus = Array.isArray(vals) ? vals : [];
          } else {
            normalizedMenu.leftSideBarMenus = [];
          }
          // eslint-disable-next-line no-console
          console.warn("AuthProvider: normalized menu.leftSideBarMenus to an array", normalizedMenu.leftSideBarMenus);
        }

        localStorage.setItem("user", JSON.stringify(user));
        localStorage.setItem("menu", JSON.stringify(normalizedMenu));
        localStorage.setItem("paths", JSON.stringify(paths));

        dispatch({
          type: "INITIAL",
          payload: {
            isInitialized: true,
            isAuthenticated: true,
            user,
            menu: normalizedMenu,
            paths,
          },
        });
      } else {
        if (import.meta.env.DEV) {
          // Dev-mode fallback: simulate a logged-in user so we don't get kicked to login
          const devUser = {
            userId: 1,
            firstName: "Dev",
            lastName: "User",
            fullName: "Dev User",
            email: "dev@localhost",
            departmentName: "IT",
          };
          const devPaths = ROUTE_PATHS;
          const devMenu = { leftSideBarMenus: [] };
          
          dispatch({
            type: "INITIAL",
            payload: {
              isInitialized: true,
              isAuthenticated: true,
              user: devUser,
              menu: devMenu,
              paths: devPaths,
            },
          });
        } else {
          dispatch({
            type: "INITIAL",
            payload: {
              isInitialized: true,
              isAuthenticated: false,
              user: null,
              menu: null,
              paths: null,
            },
          });
        }
      }
    } catch (error) {
      console.log(error);
      const isNetworkError = !error.response;

      if (isNetworkError) {
        toast.warning("You're Session has expired, please login again.");

        // Clear session and logout when offline
        setSession(null);
        setUserSession(null);

        dispatch({
          type: "INITIAL",
          payload: {
            isInitialized: true,
            isAuthenticated: false,
            user: null,
            menu: null,
            paths: null,
          },
        });
      } else {
        dispatch({
          type: "INITIAL",
          payload: {
            isInitialized: true,
            isAuthenticated: false,
            user: null,
            menu: null,
            paths: null,
          },
        });
      }
    }
  }, [storageAvailable]);

  useEffect(() => {
    initialize();
  }, [initialize]);

  // LOGIN
  const login = useCallback(async (email, password) => {
    const response = await axios.post(
      AUTH_APIs.LOGIN.AUTH,
      { email, password },
      { headers: { "x-api-key": HOST_API_KEY } }
    );

    const { token } = response.data;

    setSession(token);
    await initialize();

    dispatch({
      type: "LOGIN",
      payload: {
        token,
      },
    });
  }, [initialize]);

  // LOGOUT
  const logout = useCallback(() => {
    setSession(null);
    setUserSession(null);

    dispatch({
      type: "LOGOUT",
    });
  }, []);

  const memoizedValue = useMemo(
    () => ({
      isInitialized: state.isInitialized,
      isAuthenticated: state.isAuthenticated,
      user: state.user,
      menu: state.menu,
      paths: state.paths,
      method: "jwt",
      login,
      logout,
    }),
    [
      state.isAuthenticated,
      state.isInitialized,
      state.user,
      state.menu,
      state.paths,
      login,
      logout,
    ]
  );

  return (
    <AuthContext.Provider value={memoizedValue}>
      {children}
    </AuthContext.Provider>
  );
}
