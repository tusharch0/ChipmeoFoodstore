import { apiClient } from "@/lib/api-client"
import { authHeaders, type ApiResponse } from "./utils"
import type { FinancialReport } from "@/lib/types"

export const financialReportService = {
  get: async (organizationId: string, fromDate: string, toDate: string, branchId?: string): Promise<FinancialReport> => {
    const query = new URLSearchParams({ fromDate, toDate }); if (branchId) query.set("branchId", branchId)
    return (await apiClient<ApiResponse<FinancialReport>>(`/reports/financial/${organizationId}?${query}`, authHeaders())).data
  },
  exportUrl: (organizationId: string, fromDate: string, toDate: string, branchId?: string) => {
    const query = new URLSearchParams({ fromDate, toDate }); if (branchId) query.set("branchId", branchId)
    return `/api/proxy/reports/financial/${organizationId}/export?${query}`
  },
}
