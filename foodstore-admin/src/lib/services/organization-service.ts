import { apiClient } from "@/lib/api-client"
import { authHeaders, type ApiResponse } from "./utils"
import type { Organization, OrganizationBranch } from "@/lib/types"

export type BranchInput = { name: string; code?: string; address?: string; city?: string; phone?: string; openingHoursJson?: string; taxSettingsJson?: string; kitchenRouting?: string; isActive?: boolean }

export const organizationService = {
  list: async (): Promise<Organization[]> => (await apiClient<ApiResponse<Organization[]>>("/admin/organizations", authHeaders())).data,
  addBranch: async (organizationId: string, body: BranchInput): Promise<OrganizationBranch> => (await apiClient<ApiResponse<OrganizationBranch>>(`/admin/organizations/${organizationId}/branches`, { method: "POST", body: JSON.stringify(body), ...authHeaders() })).data,
  updateBranch: async (organizationId: string, branchId: string, body: BranchInput): Promise<OrganizationBranch> => (await apiClient<ApiResponse<OrganizationBranch>>(`/admin/organizations/${organizationId}/branches/${branchId}`, { method: "PUT", body: JSON.stringify(body), ...authHeaders() })).data,
}
