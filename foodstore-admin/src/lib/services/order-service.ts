import { apiClient } from "@/lib/api-client";
import type {
  Order,
  OrderCreateDto,
  OrderReceipt,
  PagedOrdersResponse,
} from "@/lib/types";
import { authHeaders, type ApiResponse } from "./utils";

export const orderService = {
  async getAll(): Promise<Order[]> {
    const res = await apiClient<ApiResponse<Order[]>>(
      "/admin/orders",
      authHeaders(),
    );
    return res.data;
  },
  async getById(id: string): Promise<Order> {
    const res = await apiClient<ApiResponse<Order>>(
      `/admin/orders/${id}`,
      authHeaders(),
    );
    return res.data;
  },
  async getReceipt(id: string): Promise<OrderReceipt> {
    const res = await apiClient<ApiResponse<OrderReceipt>>(
      `/admin/orders/${id}/receipt`,
      authHeaders(),
    );
    return res.data;
  },
  async create(data: OrderCreateDto): Promise<Order> {
    const res = await apiClient<ApiResponse<Order>>("/admin/orders", {
      method: "POST",
      body: JSON.stringify(data),
      ...authHeaders(),
    });
    return res.data;
  },
  async update(id: string, data: Partial<OrderCreateDto>): Promise<Order> {
    const res = await apiClient<ApiResponse<Order>>(`/admin/orders/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
      ...authHeaders(),
    });
    return res.data;
  },
  async delete(id: string): Promise<void> {
    await apiClient(`/admin/orders/${id}`, {
      method: "DELETE",
      ...authHeaders(),
    });
  },
  async updateStatus(
    id: string,
    status: string,
    paymentMethod?: string,
    paymentAmount?: number,
  ): Promise<boolean> {
    await apiClient(`/admin/orders/${id}/status`, {
      method: "PUT",
      body: JSON.stringify({ status, paymentMethod, paymentAmount }),
      ...authHeaders(),
    });
    return true;
  },
  async setUnpaid(id: string): Promise<void> {
    await apiClient(`/admin/orders/${id}/set-unpaid`, {
      method: "PUT",
      ...authHeaders(),
    });
  },
  async getPaged(
    page: number,
    pageSize: number,
    fromDate?: string,
    toDate?: string,
  ): Promise<PagedOrdersResponse> {
    const params = new URLSearchParams({
      page: String(page),
      pageSize: String(pageSize),
    });
    if (fromDate) params.set("fromDate", fromDate);
    if (toDate) params.set("toDate", toDate);
    const res = await apiClient<ApiResponse<PagedOrdersResponse>>(
      `/admin/orders/paged?${params}`,
      authHeaders(),
    );
    return res.data;
  },
  async getByStatus(status: string): Promise<Order[]> {
    const res = await apiClient<ApiResponse<Order[]>>(
      `/admin/orders/status/${status}`,
      authHeaders(),
    );
    return res.data;
  },
};
