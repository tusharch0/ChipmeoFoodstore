export interface Order {
	id: string;
	orderCode: string;
	branchId?: string;
	branchName?: string;
	kitchenRouting?: string;
	sourceId?: string;
	sourceName?: string;
	source?: { id: number; name: string };
	customerId?: number;
	items: OrderItem[];
	subtotalAmount: number;
	discountId?: number;
	discountAmount: number;
	discountCode?: string;
	vatAmount?: number;
	totalAmount: number;
	status: string;

	note?: string;
	customerName?: string;
	customerPhone?: string;
	history?: OrderStatusHistory[];
	qrPaymentUrl?: string;
	createdAt: string;
	updatedAt?: string;
}

export interface OrderItem {
	id?: number;
	menuItemId: number;
	menuItemName?: string;
	comboId?: number;
	comboName?: string;
	quantity: number;
	unitPrice: number;
	totalPrice: number;
	addons?: OrderAddon[];
	note?: string;
}

export interface OrderAddon {
	addonId: number;
	addonName?: string;
	quantity: number;
	// Backend sends unitPrice, not price.
	unitPrice: number;
}

export type OrderStatus =
	| 'pending'
	| 'confirmed'
	| 'preparing'
	| 'ready'
	| 'served'
	| 'paid'
	| 'cancelled';

export interface OrderCreateDto {
	sourceId?: string;
	customerName?: string;
	customerPhone?: string;
	items: OrderItemCreateDto[];
	discountId?: number;
	discountCode?: string;
	note?: string;
}

export interface OrderItemCreateDto {
	menuItemId?: number;
	comboId?: number;
	quantity: number;
	addons?: { addonId: number; quantity: number }[];
	note?: string;
}

export interface OrderUpdateStatusDto {
	status: string;
	paymentMethod?: string;
	paymentAmount?: number;
}

export interface OrderStatusHistory {
	id: number;
	orderId: number;
	fromStatus: string;
	toStatus: string;
	note?: string;
	changedBy: number;
	changedAt: string;
	changedByName?: string;
}
