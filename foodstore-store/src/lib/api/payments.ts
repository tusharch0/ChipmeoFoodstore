import { apiRequest } from '$lib/api/utils.js';
import { API_ENDPOINTS } from '$lib/config/index.js';

export type PaymentIntentStatus = 'created' | 'pending' | 'succeeded' | 'failed' | 'expired' | 'cancelled' | 'refund_pending' | 'refunded';

export interface PaymentIntent {
	id: string;
	orderId: string;
	amount: number;
	currency: string;
	provider: string;
	providerReference?: string | null;
	checkoutId?: string | null;
	status: PaymentIntentStatus;
	customerPhone?: string | null;
	failureReason?: string | null;
	expiresAt?: string | null;
}

export function createPaymentIntent(orderId: string, phone: string, idempotencyKey: string, customerName?: string): Promise<PaymentIntent> {
	return apiRequest<PaymentIntent>(API_ENDPOINTS.payments.createIntent, {
		method: 'POST', headers: { 'Idempotency-Key': idempotencyKey }, body: JSON.stringify({ orderId, phone, customerName })
	});
}

export function getPaymentIntent(id: string): Promise<PaymentIntent> {
	return apiRequest<PaymentIntent>(API_ENDPOINTS.payments.intent(id));
}
