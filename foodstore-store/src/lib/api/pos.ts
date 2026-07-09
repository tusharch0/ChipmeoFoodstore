import { apiRequest } from '$lib/api/utils.js';
import { API_ENDPOINTS } from '$lib/config/index.ts';
import type {
	Category,
	MenuItem,
	Addon,
	Discount,
	Combo,
	Source,
	Order,
	OrderCreateDto
} from '$lib/types/index.js';
import { ordersAPI } from '$lib/api/orders.js';

export interface POSMenuData {
	categories: Category[];
	menuItems: MenuItem[];
	addons: Addon[];
	discounts: Discount[];
	combos: Combo[];
}

export const posAPI = {
	async getMenuData(): Promise<POSMenuData> {
		return apiRequest<POSMenuData>(API_ENDPOINTS.pos.menu);
	},

	async getSources(): Promise<Source[]> {
		// Use the admin endpoint for now as shared resource, or pos specific if confirmed
		// In config we changed tables -> sources which points to admin/sources
		return apiRequest<Source[]>(API_ENDPOINTS.sources.list);
	},

	async createOrder(data: OrderCreateDto): Promise<Order> {
		return apiRequest<Order>(API_ENDPOINTS.posOrders.create, {
			method: 'POST',
			body: JSON.stringify(data)
		});
	},

	async updateOrder(id: string, data: OrderCreateDto): Promise<Order> {
		return apiRequest<Order>(`${API_ENDPOINTS.posOrders.create}/${id}`, {
			method: 'PUT',
			body: JSON.stringify(data)
		});
	},

	async updateOrderStatus(
		id: string,
		status: string,
		paymentMethod?: string,
		paymentAmount?: number
	): Promise<boolean> {
		return ordersAPI.updateStatus(id, status, paymentMethod, paymentAmount);
	}
};
