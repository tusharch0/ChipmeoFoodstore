import { apiClient, downloadApiFile } from "@/lib/api-client";
import { authHeaders, type ApiResponse } from "./utils";
import type { SettlementBatch } from "@/lib/types";
export const settlementService = {
  async list(organizationId: string): Promise<SettlementBatch[]> {
    return (
      await apiClient<ApiResponse<SettlementBatch[]>>(
        `/admin/settlements/${organizationId}`,
        authHeaders(),
      )
    ).data;
  },
  async get(id: string): Promise<SettlementBatch> {
    return (
      await apiClient<ApiResponse<SettlementBatch>>(
        `/admin/settlements/batch/${id}`,
        authHeaders(),
      )
    ).data;
  },
  async export(id: string): Promise<void> {
    await downloadApiFile(
      `/admin/settlements/batch/${id}/export`,
      authHeaders().token,
    );
  },
  async generate(
    organizationId: string,
    date: string,
    currency: string,
  ): Promise<SettlementBatch> {
    return (
      await apiClient<ApiResponse<SettlementBatch>>(
        `/admin/settlements/${organizationId}/generate?date=${date}&currency=${currency}`,
        { method: "POST", ...authHeaders() },
      )
    ).data;
  },
  async transition(
    id: string,
    status: string,
    payoutReference?: string,
  ): Promise<SettlementBatch> {
    return (
      await apiClient<ApiResponse<SettlementBatch>>(
        `/admin/settlements/${id}/transition`,
        {
          method: "POST",
          body: JSON.stringify({ status, payoutReference }),
          ...authHeaders(),
        },
      )
    ).data;
  },
};
