import { apiClient } from "@/lib/api-client";
import { authHeaders, type ApiResponse } from "./utils";
import type {
  Organization,
  OrganizationBranch,
  OrganizationMembership,
} from "@/lib/types";

export type BranchInput = {
  name: string;
  code?: string;
  address?: string;
  city?: string;
  phone?: string;
  openingHoursJson?: string;
  taxSettingsJson?: string;
  kitchenRouting?: string;
  isActive?: boolean;
};

export const organizationService = {
  list: async (): Promise<Organization[]> =>
    (
      await apiClient<ApiResponse<Organization[]>>(
        "/admin/organizations",
        authHeaders(),
      )
    ).data,
  addBranch: async (
    organizationId: string,
    body: BranchInput,
  ): Promise<OrganizationBranch> =>
    (
      await apiClient<ApiResponse<OrganizationBranch>>(
        `/admin/organizations/${organizationId}/branches`,
        { method: "POST", body: JSON.stringify(body), ...authHeaders() },
      )
    ).data,
  updateBranch: async (
    organizationId: string,
    branchId: string,
    body: BranchInput,
  ): Promise<OrganizationBranch> =>
    (
      await apiClient<ApiResponse<OrganizationBranch>>(
        `/admin/organizations/${organizationId}/branches/${branchId}`,
        { method: "PUT", body: JSON.stringify(body), ...authHeaders() },
      )
    ).data,
  memberships: async (
    organizationId: string,
  ): Promise<OrganizationMembership[]> =>
    (
      await apiClient<ApiResponse<OrganizationMembership[]>>(
        `/admin/organizations/${organizationId}/memberships`,
        authHeaders(),
      )
    ).data,
  updateMembership: async (
    organizationId: string,
    membershipId: string,
    role: string,
    isActive: boolean,
  ): Promise<OrganizationMembership> =>
    (
      await apiClient<ApiResponse<OrganizationMembership>>(
        `/admin/organizations/${organizationId}/memberships/${membershipId}`,
        {
          method: "PUT",
          body: JSON.stringify({ role, isActive }),
          ...authHeaders(),
        },
      )
    ).data,
};
