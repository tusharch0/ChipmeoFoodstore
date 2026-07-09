import { env } from '$env/dynamic/public';

const apiBaseUrl = env.PUBLIC_API_URL;
if (!apiBaseUrl) {
	throw new Error('PUBLIC_API_URL environment variable is not set');
}
export const API_BASE_URL = apiBaseUrl;

const apiRoot = apiBaseUrl.replace(/\/v2\/?$/, '');
export const API_HOST_URL = apiRoot;

const storagePrefix = env.PUBLIC_AUTH_STORAGE_PREFIX || '';
export const STORAGE_KEYS = {
	TOKEN: storagePrefix + 'token',
	USER: storagePrefix + 'user'
};

const signalrHub = env.PUBLIC_SIGNALR_HUB;
if (!signalrHub) {
	throw new Error('PUBLIC_SIGNALR_HUB environment variable is not set');
}
export const SIGNALR_HUB_PATH = signalrHub;

export const SIGNALR_SKIP_NEGOTIATION = env.PUBLIC_SIGNALR_WS_ONLY !== 'false';

const vietqrUrl = env.PUBLIC_VIETQR_API_URL;
if (!vietqrUrl) {
	throw new Error('PUBLIC_VIETQR_API_URL environment variable is not set');
}
export const VIETQR_API_URL = vietqrUrl;

const qrTemplate = env.PUBLIC_DEFAULT_QR_TEMPLATE;
if (!qrTemplate) {
	throw new Error('PUBLIC_DEFAULT_QR_TEMPLATE environment variable is not set');
}
export const DEFAULT_QR_TEMPLATE = qrTemplate;

const siteDomain = env.PUBLIC_SITE_DOMAIN;
if (!siteDomain) {
	throw new Error('PUBLIC_SITE_DOMAIN environment variable is not set');
}
export const SITE_DOMAIN = siteDomain;

const defaultPassword = env.PUBLIC_DEFAULT_CUSTOMER_PASSWORD;
if (!defaultPassword) {
	throw new Error('PUBLIC_DEFAULT_CUSTOMER_PASSWORD environment variable is not set');
}
export const DEFAULT_CUSTOMER_PASSWORD = defaultPassword;

export const API_ENDPOINTS = {
	auth: {
		base: `${API_BASE_URL}/api/auth`,
		login: `${API_BASE_URL}/api/auth/login`,
		logout: `${API_BASE_URL}/api/auth/logout`,
		refresh: `${API_BASE_URL}/api/auth/refresh`
	},
	orders: {
		list: `${API_BASE_URL}/api/admin/orders`,
		create: `${API_BASE_URL}/api/pos/orders`,
		detail: (id: number) => `${API_BASE_URL}/api/admin/orders/${id}`,
		update: (id: number) => `${API_BASE_URL}/api/admin/orders/${id}`,
		updateStatus: (id: number) => `${API_BASE_URL}/api/admin/orders/${id}/status`,
		setUnpaid: (id: number) => `${API_BASE_URL}/api/admin/orders/${id}/set-unpaid`,
		byStatus: (status: string) => `${API_BASE_URL}/api/admin/orders/status/${status}`,
		payment: (id: number) => `${API_BASE_URL}/api/pos/orders/${id}/payment`,
		paged: `${API_BASE_URL}/api/admin/orders/paged`,
		delete: (id: number) => `${API_BASE_URL}/api/admin/orders/${id}`
	},
	posOrders: {
		create: `${API_BASE_URL}/api/pos/orders`,
		updateStatus: (id: number) => `${API_BASE_URL}/api/pos/orders/${id}/status`
	},
	pos: {
		menu: `${API_BASE_URL}/api/pos/menu`,
		tables: `${API_BASE_URL}/api/pos/tables`,
		addons: `${API_BASE_URL}/api/pos/addons`
	},
	categories: {
		list: `${API_BASE_URL}/api/admin/categories`,
		create: `${API_BASE_URL}/api/admin/categories`,
		detail: (id: number) => `${API_BASE_URL}/api/admin/categories/${id}`,
		update: (id: number) => `${API_BASE_URL}/api/admin/categories/${id}`,
		delete: (id: number) => `${API_BASE_URL}/api/admin/categories/${id}`
	},
	menuItems: {
		list: `${API_BASE_URL}/api/admin/menu-items`,
		create: `${API_BASE_URL}/api/admin/menu-items`,
		detail: (id: number) => `${API_BASE_URL}/api/admin/menu-items/${id}`,
		update: (id: number) => `${API_BASE_URL}/api/admin/menu-items/${id}`,
		delete: (id: number) => `${API_BASE_URL}/api/admin/menu-items/${id}`
	},
	addons: {
		list: `${API_BASE_URL}/api/admin/addons`,
		create: `${API_BASE_URL}/api/admin/addons`,
		detail: (id: number) => `${API_BASE_URL}/api/admin/addons/${id}`,
		update: (id: number) => `${API_BASE_URL}/api/admin/addons/${id}`,
		delete: (id: number) => `${API_BASE_URL}/api/admin/addons/${id}`
	},
	sources: {
		list: `${API_BASE_URL}/api/admin/sources`,
		create: `${API_BASE_URL}/api/admin/sources`,
		detail: (id: number) => `${API_BASE_URL}/api/admin/sources/${id}`,
		update: (id: number) => `${API_BASE_URL}/api/admin/sources/${id}`,
		delete: (id: number) => `${API_BASE_URL}/api/admin/sources/${id}`
	},
	combos: {
		list: `${API_BASE_URL}/api/admin/combos`,
		create: `${API_BASE_URL}/api/admin/combos`,
		detail: (id: number) => `${API_BASE_URL}/api/admin/combos/${id}`,
		update: (id: number) => `${API_BASE_URL}/api/admin/combos/${id}`,
		delete: (id: number) => `${API_BASE_URL}/api/admin/combos/${id}`
	},
	discounts: {
		list: `${API_BASE_URL}/api/admin/discounts`,
		create: `${API_BASE_URL}/api/admin/discounts`,
		detail: (id: number) => `${API_BASE_URL}/api/admin/discounts/${id}`,
		update: (id: number) => `${API_BASE_URL}/api/admin/discounts/${id}`,
		delete: (id: number) => `${API_BASE_URL}/api/admin/discounts/${id}`
	},
	employees: {
		list: `${API_BASE_URL}/api/admin/employees`,
		create: `${API_BASE_URL}/api/admin/employees`,
		detail: (id: number) => `${API_BASE_URL}/api/admin/employees/${id}`,
		update: (id: number) => `${API_BASE_URL}/api/admin/employees/${id}`,
		delete: (id: number) => `${API_BASE_URL}/api/admin/employees/${id}`
	},
	roles: {
		list: `${API_BASE_URL}/api/admin/roles`,
		create: `${API_BASE_URL}/api/admin/roles`,
		detail: (id: number) => `${API_BASE_URL}/api/admin/roles/${id}`,
		update: (id: number) => `${API_BASE_URL}/api/admin/roles/${id}`,
		delete: (id: number) => `${API_BASE_URL}/api/admin/roles/${id}`,
		assignPermissions: (id: number) => `${API_BASE_URL}/api/admin/roles/${id}/permissions`
	},
	permissions: {
		list: `${API_BASE_URL}/api/admin/permissions`
	},
	reports: {
		dashboardStats: `${API_BASE_URL}/api/admin/dashboard/stats`,
		analytics: `${API_BASE_URL}/api/admin/dashboard/analytics`,
		forecast: `${API_BASE_URL}/api/admin/dashboard/forecast`,
		recommendations: `${API_BASE_URL}/api/admin/dashboard/recommendations`
	},
	paymentSettings: `${API_BASE_URL}/api/admin/payment-settings`,
	payments: {
		createIntent: `${API_BASE_URL}/api/pos/payments`,
		intent: (id: string) => `${API_BASE_URL}/api/pos/payments/${id}`
	},
	kitchen: {
		list: `${API_BASE_URL}/api/kitchen/orders`,
		start: (id: number) => `${API_BASE_URL}/api/kitchen/orders/${id}/start`,
		complete: (id: number) => `${API_BASE_URL}/api/kitchen/orders/${id}/complete`
	},
	blog: `${API_BASE_URL}/api/blog`,
	customers: `${API_BASE_URL}/api/customers`,
	media: `${API_BASE_URL}/api/media`
};
