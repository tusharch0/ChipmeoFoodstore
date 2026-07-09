import { apiClient } from "@/lib/api-client"
import { authHeaders, type ApiResponse } from "./utils"
import type { PaymentIntent, PaymentIntentDetail } from "@/lib/types"

export type PaymentSearch = { orderId?: string; phone?: string; providerReference?: string; status?: string; fromDate?: string; toDate?: string }

export const paymentService = {
  async search(filter: PaymentSearch = {}): Promise<PaymentIntent[]> {
    const params = new URLSearchParams({ page: "1", pageSize: "100" })
    Object.entries(filter).forEach(([key, value]) => { if (value) params.set(key, value) })
    const res = await apiClient<ApiResponse<PaymentIntent[]>>(`/admin/payments?${params}`, authHeaders())
    return res.data
  },
  async get(id: string): Promise<PaymentIntentDetail> {
    const res = await apiClient<ApiResponse<PaymentIntentDetail>>(`/admin/payments/${id}`, authHeaders())
    return res.data
  },
  async refund(id: string, reason: string): Promise<PaymentIntent> {
    const res = await apiClient<ApiResponse<PaymentIntent>>(`/admin/payments/${id}/refund`, { method: "POST", body: JSON.stringify({ reason }), ...authHeaders() })
    return res.data
  },
}
