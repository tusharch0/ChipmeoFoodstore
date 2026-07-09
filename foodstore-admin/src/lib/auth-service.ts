import { apiClient } from "./api-client";

export interface AuthUser {
  id: string;
  name: string;
  username: string;
  email: string;
  roleName: string;
  permissions: string[];
  phone?: string;
  avatarUrl?: string;
}

interface ApiResponse<T> {
  data: T;
}

interface LoginPayload {
  token: string;
  user: AuthUser;
  expiresIn: number;
  refreshToken?: string;
}

let authToken: string | null = null;

export function getAuthToken(): string | null {
  return authToken;
}

export const authService = {
  async login(username: string, password: string): Promise<LoginPayload> {
    const res = await apiClient<ApiResponse<LoginPayload>>("/auth/login", {
      method: "POST",
      body: JSON.stringify({ username, password }),
      skipAuth: true,
    });
    authToken = res.data.token;
    if (typeof window !== "undefined") window.localStorage.removeItem("foodstore_admin_branch_id");
    return res.data;
  },

  async logout(): Promise<void> {
    try {
      await apiClient<ApiResponse<unknown>>("/auth/logout", {
        method: "POST",
        token: authToken ?? undefined,
      });
    } catch {
      // ignore
    }
    authToken = null;
  },

  async getProfile(): Promise<AuthUser | null> {
    try {
      const res = await apiClient<ApiResponse<AuthUser>>("/auth/profile", {
        token: authToken ?? undefined,
      });
      return res.data;
    } catch {
      return null;
    }
  },

  async updateProfile(dto: {
    name?: string;
    email?: string;
    phone?: string;
    avatarUrl?: string;
  }): Promise<AuthUser> {
    const res = await apiClient<ApiResponse<AuthUser>>("/auth/profile", {
      method: "PUT",
      body: JSON.stringify(dto),
      token: authToken ?? undefined,
    });
    return res.data;
  },

  async uploadAvatar(file: File): Promise<string> {
    const formData = new FormData();
    formData.append("file", file);
    formData.append("folder", "avatars");
    const res = await apiClient<ApiResponse<{ fileUrl: string }>>("/media/upload", {
      method: "POST",
      body: formData,
      token: authToken ?? undefined,
    });
    return res.data.fileUrl;
  },
};
