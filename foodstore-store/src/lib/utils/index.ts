// Utility functions for formatting

export function formatCurrency(amount: number): string {
	return new Intl.NumberFormat('en-US', {
		style: 'currency',
		currency: 'VND',
		minimumFractionDigits: 0,
		maximumFractionDigits: 0
	}).format(amount);
}

export function formatDate(date: string | Date): string {
	return new Intl.DateTimeFormat('en-US', {
		year: 'numeric',
		month: '2-digit',
		day: '2-digit',
		hour: '2-digit',
		minute: '2-digit'
	}).format(new Date(date));
}

export function formatTime(date: string | Date): string {
	return new Intl.DateTimeFormat('en-US', {
		hour: '2-digit',
		minute: '2-digit'
	}).format(new Date(date));
}

export function generateOrderCode(): string {
	const now = new Date();
	const year = now.getFullYear().toString().slice(-2);
	const month = (now.getMonth() + 1).toString().padStart(2, '0');
	const day = now.getDate().toString().padStart(2, '0');
	const random = Math.random().toString(36).substring(2, 6).toUpperCase();
	return `ORD${year}${month}${day}${random}`;
}

export * from './cart.js';
export * from './auth.js';
export * from './state.js';
