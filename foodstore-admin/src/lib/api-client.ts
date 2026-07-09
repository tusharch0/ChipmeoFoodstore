type FetchOptions = RequestInit & {
  skipAuth?: boolean;
  token?: string;
};

export async function apiClient<T>(
  path: string,
  options: FetchOptions = {},
): Promise<T> {
  const { skipAuth, token, ...fetchOpts } = options;

  const isFormData = fetchOpts.body instanceof FormData;

  const headers: Record<string, string> = {
    ...(fetchOpts.headers as Record<string, string>),
  };

  if (!isFormData) {
    headers["Content-Type"] = "application/json";
  }

  if (!skipAuth && token) {
    headers["Authorization"] = `Bearer ${token}`;
  }
  if (!skipAuth && typeof window !== "undefined") {
    const branchId = window.localStorage.getItem("foodstore_admin_branch_id");
    if (branchId) headers["X-Branch-ID"] = branchId;
  }

  const res = await fetch(`/api/proxy${path}`, {
    ...fetchOpts,
    headers,
  });

  if (!res.ok) {
    if (
      res.status === 401 &&
      typeof window !== "undefined" &&
      window.location.pathname !== "/login"
    ) {
      window.location.href = "/login";
    }
    const body = await res
      .json()
      .catch(() => ({ message: `HTTP ${res.status}` }));
    throw new Error(body.message || body.title || `Request failed`);
  }

  return res.json();
}

export async function downloadApiFile(
  path: string,
  token?: string,
): Promise<void> {
  const branchId =
    typeof window === "undefined"
      ? null
      : window.localStorage.getItem("foodstore_admin_branch_id");
  const res = await fetch(`/api/proxy${path}`, {
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(branchId ? { "X-Branch-ID": branchId } : {}),
    },
  });
  if (!res.ok) {
    const body = await res
      .json()
      .catch(() => ({ message: `HTTP ${res.status}` }));
    throw new Error(body.message || body.title || "Download failed");
  }
  const blob = await res.blob();
  const disposition = res.headers.get("content-disposition") ?? "";
  const encoded = disposition.match(/filename\*=UTF-8''([^;]+)/i)?.[1];
  const basic = disposition.match(/filename="?([^";]+)"?/i)?.[1];
  const filename = encoded
    ? decodeURIComponent(encoded)
    : (basic ?? "export.csv");
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = filename;
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(url);
}
