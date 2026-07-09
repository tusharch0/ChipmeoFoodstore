import { apiRequest } from '$lib/api/utils.js';
import { API_ENDPOINTS } from '$lib/config/index.ts';
import type { Order, OrderCreateDto } from '$lib/types/index.ts';

export const ordersAPI = {
	async getAll(): Promise<Order[]> {
		return apiRequest<Order[]>(API_ENDPOINTS.orders.list);
	},

	async getById(id: string): Promise<Order> {
		return apiRequest<Order>(API_ENDPOINTS.orders.detail(id));
	},

	async create(data: OrderCreateDto): Promise<Order> {
		return apiRequest<Order>(API_ENDPOINTS.orders.create, {
			method: 'POST',
			body: JSON.stringify(data)
		});
	},

	async update(id: string, data: Partial<OrderCreateDto>): Promise<Order> {
		return apiRequest<Order>(API_ENDPOINTS.orders.update(id), {
			method: 'PUT',
			body: JSON.stringify(data)
		});
	},

	async updateStatus(
		id: string,
		status: string,
		paymentMethod?: string,
		paymentAmount?: number
	): Promise<boolean> {
		await apiRequest<void>(API_ENDPOINTS.orders.updateStatus(id), {
			method: 'PUT',
			body: JSON.stringify({ status, paymentMethod, paymentAmount })
		});
		return true;
	},

	async delete(id: string): Promise<void> {
		return apiRequest<void>(API_ENDPOINTS.orders.delete(id), {
			method: 'DELETE'
		});
	},

	async setUnpaid(id: string): Promise<void> {
		return apiRequest<void>(API_ENDPOINTS.orders.setUnpaid(id), {
			method: 'PUT'
		});
	},

	async getPaged(
		page: number,
		pageSize: number,
		fromDate?: string,
		toDate?: string
	): Promise<{ items: Order[]; totalCount: number; totalPages: number }> {
		const params = new URLSearchParams({
			page: page.toString(),
			pageSize: pageSize.toString()
		});
		if (fromDate) params.append('fromDate', fromDate);
		if (toDate) params.append('toDate', toDate);

		const data = await apiRequest<Order[]>(`${API_ENDPOINTS.orders.paged}?${params.toString()}`);
		// apiRequest unwraps { data, meta } → returns data array.
		// Meta is lost during unwrap, so we fall back to client-side total.
		return {
			items: data as unknown as Order[],
			totalCount:
				(data as unknown as { meta?: { totalCount: number } })?.meta?.totalCount ??
				(data as unknown as Order[]).length,
			totalPages: (data as unknown as { meta?: { totalPages: number } })?.meta?.totalPages ?? 1
		};
	},
	async getByStatus(status: string): Promise<Order[]> {
		return apiRequest<Order[]>(API_ENDPOINTS.orders.byStatus(status));
	},

	async processPayment(id: string, paymentMethod: string, cashReceived?: number): Promise<Order> {
		return apiRequest<Order>(API_ENDPOINTS.orders.payment(id), {
			method: 'POST',
			body: JSON.stringify({ paymentMethod, cashReceived })
		});
	}
};
