import { apiClient, downloadApiFile } from "@/lib/api-client";
import { authHeaders, type ApiResponse } from "./utils";
import type { FinancialReport } from "@/lib/types";

export const financialReportService = {
  get: async (
    organizationId: string,
    fromDate: string,
    toDate: string,
    branchId?: string,
  ): Promise<FinancialReport> => {
    const query = new URLSearchParams({ fromDate, toDate });
    if (branchId) query.set("branchId", branchId);
    return (
      await apiClient<ApiResponse<FinancialReport>>(
        `/reports/financial/${organizationId}?${query}`,
        authHeaders(),
      )
    ).data;
  },
  export: async (
    organizationId: string,
    fromDate: string,
    toDate: string,
    branchId?: string,
  ) => {
    const query = new URLSearchParams({ fromDate, toDate });
    if (branchId) query.set("branchId", branchId);
    await downloadApiFile(
      `/reports/financial/${organizationId}/export?${query}`,
      authHeaders().token,
    );
  },
  exportPdf: async (
    organizationId: string,
    fromDate: string,
    toDate: string,
    branchId?: string,
  ) => {
    const query = new URLSearchParams({ fromDate, toDate });
    if (branchId) query.set("branchId", branchId);
    await downloadApiFile(
      `/reports/financial/${organizationId}/export.pdf?${query}`,
      authHeaders().token,
    );
  },
};
