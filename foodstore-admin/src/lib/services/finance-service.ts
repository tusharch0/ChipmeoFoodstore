import { apiClient, downloadApiFile } from "@/lib/api-client";
import { authHeaders, type ApiResponse } from "./utils";
import type {
  LedgerAccount,
  LedgerJournal,
  Organization,
  WalletBalance,
} from "@/lib/types";

export const financeService = {
  async organizations(): Promise<Organization[]> {
    return (
      await apiClient<ApiResponse<Organization[]>>(
        "/admin/organizations",
        authHeaders(),
      )
    ).data;
  },
  async accounts(organizationId: string): Promise<LedgerAccount[]> {
    return (
      await apiClient<ApiResponse<LedgerAccount[]>>(
        `/admin/finance/accounts/${organizationId}`,
        authHeaders(),
      )
    ).data;
  },
  async wallet(branchId: string): Promise<WalletBalance> {
    return (
      await apiClient<ApiResponse<WalletBalance>>(
        `/admin/finance/wallet/${branchId}`,
        authHeaders(),
      )
    ).data;
  },
  async ledger(
    branchId: string,
    from?: string,
    to?: string,
  ): Promise<LedgerJournal[]> {
    const query = new URLSearchParams();
    if (from) query.set("from", from);
    if (to) query.set("to", to);
    return (
      await apiClient<ApiResponse<LedgerJournal[]>>(
        `/admin/finance/ledger/${branchId}?${query}`,
        authHeaders(),
      )
    ).data;
  },
  async exportLedger(
    branchId: string,
    from?: string,
    to?: string,
  ): Promise<void> {
    const query = new URLSearchParams();
    if (from) query.set("from", from);
    if (to) query.set("to", to);
    await downloadApiFile(
      `/admin/finance/ledger/${branchId}/export?${query}`,
      authHeaders().token,
    );
  },
};
