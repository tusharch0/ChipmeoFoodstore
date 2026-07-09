import { apiClient } from "@/lib/api-client";
import type { PaymentSetting, PaymentSettingCreateDto, PaymentSettingUpdateDto } from "@/lib/types";
import { authHeaders, type ApiResponse } from "./utils";

export interface Bank {
  id: number;
  name: string;
  shortName: string;
  code: string;
  logo: string;
}

export const paymentSettingService = {
  async getAll(): Promise<PaymentSetting[]> {
    const res = await apiClient<ApiResponse<PaymentSetting[]>>("/admin/payment-settings/all", authHeaders());
    return res.data;
  },
  async getById(id: string): Promise<PaymentSetting> {
    const res = await apiClient<ApiResponse<PaymentSetting>>(`/admin/payment-settings/${id}`, authHeaders());
    return res.data;
  },
  async create(data: PaymentSettingCreateDto): Promise<PaymentSetting> {
    const res = await apiClient<ApiResponse<PaymentSetting>>("/admin/payment-settings", {
      method: "POST", body: JSON.stringify(data), ...authHeaders(),
    });
    return res.data;
  },
  async update(id: string, data: PaymentSettingUpdateDto): Promise<void> {
    await apiClient(`/admin/payment-settings/${id}`, {
      method: "PUT", body: JSON.stringify(data), ...authHeaders(),
    });
  },
  async delete(id: string): Promise<void> {
    await apiClient(`/admin/payment-settings/${id}`, {
      method: "DELETE", ...authHeaders(),
    });
  },
  async setDefault(id: string): Promise<void> {
    await apiClient(`/admin/payment-settings/${id}/set-default`, {
      method: "PUT", ...authHeaders(),
    });
  },
  async getBanks(): Promise<Bank[]> {
    return [
      { id: 1, name: "Vietnam Joint Stock Commercial Bank for Foreign Trade", shortName: "Vietcombank", code: "VCB", logo: "" },
      { id: 2, name: "Joint Stock Commercial Bank for Investment and Development of Vietnam", shortName: "BIDV", code: "BIDV", logo: "" },
      { id: 3, name: "Vietnam Joint Stock Commercial Bank for Industry and Trade", shortName: "VietinBank", code: "ICB", logo: "" },
      { id: 4, name: "Vietnam Technological and Commercial Joint Stock Bank", shortName: "Techcombank", code: "TCB", logo: "" },
      { id: 5, name: "Asia Commercial Joint Stock Bank", shortName: "ACB", code: "ACB", logo: "" },
      { id: 6, name: "Military Commercial Joint Stock Bank", shortName: "MB Bank", code: "MB", logo: "" },
      { id: 7, name: "Saigon Thuong Tin Commercial Joint Stock Bank", shortName: "Sacombank", code: "STB", logo: "" },
      { id: 8, name: "Tien Phong Commercial Joint Stock Bank", shortName: "TPBank", code: "TPB", logo: "" },
      { id: 9, name: "Timo Digital Bank", shortName: "Timo", code: "TIMO", logo: "" },
    ];
  },
};
