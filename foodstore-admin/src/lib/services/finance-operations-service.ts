import { apiClient } from "@/lib/api-client"
import { authHeaders, type ApiResponse } from "./utils"
import type { FinanceAdjustment, FinanceException, ReconciliationCase } from "@/lib/types"
export const financeOperationsService = {
  exceptions: async (organizationId: string) => (await apiClient<ApiResponse<FinanceException[]>>(`/admin/finance/operations/exceptions/${organizationId}`, authHeaders())).data,
  unmatchedPayments: async () => (await apiClient<ApiResponse<FinanceException[]>>("/admin/finance/operations/exceptions/unmatched", authHeaders())).data,
  adjustments: async (organizationId: string) => (await apiClient<ApiResponse<FinanceAdjustment[]>>(`/admin/finance/operations/adjustments/${organizationId}`, authHeaders())).data,
  requestAdjustment: async (body: object) => (await apiClient<ApiResponse<FinanceAdjustment>>("/admin/finance/operations/adjustments", { method: "POST", body: JSON.stringify(body), ...authHeaders() })).data,
  approveAdjustment: async (id: string) => (await apiClient<ApiResponse<FinanceAdjustment>>(`/admin/finance/operations/adjustments/${id}/approve`, { method: "POST", ...authHeaders() })).data,
  cases: async (organizationId: string) => (await apiClient<ApiResponse<ReconciliationCase[]>>(`/admin/finance/operations/reconciliation/${organizationId}`, authHeaders())).data,
  createCase: async (body: object) => (await apiClient<ApiResponse<ReconciliationCase>>("/admin/finance/operations/reconciliation", { method: "POST", body: JSON.stringify(body), ...authHeaders() })).data,
  resolveCase: async (id: string, resolution: string, evidenceUrl?: string) => (await apiClient<ApiResponse<ReconciliationCase>>(`/admin/finance/operations/reconciliation/${id}/resolve`, { method: "POST", body: JSON.stringify({ resolution, evidenceUrl }), ...authHeaders() })).data,
}
