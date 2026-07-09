import * as signalR from '@microsoft/signalr';
import { SvelteDate } from 'svelte/reactivity';
import { api, API_ENDPOINTS, API_BASE_URL } from '$lib/api/index.js';
import type { Order } from '$lib/types/index.js';
import { API_HOST_URL, SIGNALR_HUB_PATH } from '$lib/config/index.js';

export class KitchenState {
	orders = $state<Order[]>([]);
	isConnected = $state(false);
	activeTab = $state<'pending' | 'preparing' | 'completed'>('pending');
	connection: signalR.HubConnection | null = null;

	readonly tabs = [
		{ id: 'pending' as const, label: 'New Orders', icon: '🔔', color: 'red' },
		{ id: 'preparing' as const, label: 'Cooking', icon: '🔥', color: 'amber' },
		{ id: 'completed' as const, label: 'Done', icon: '✅', color: 'green' }
	] as const;

	// Derived lists using Svelte 5 runes
	pendingOrders = $derived(
		this.orders
			.filter((o) => o.status === 'paid')
			.sort((a, b) => {
				const dateA = new SvelteDate(a.createdAt);
				const dateB = new SvelteDate(b.createdAt);
				return dateA.getTime() - dateB.getTime();
			})
	);

	preparingOrders = $derived(
		this.orders
			.filter((o) => o.status === 'preparing')
			.sort((a, b) => {
				const dateA = new SvelteDate(a.createdAt);
				const dateB = new SvelteDate(b.createdAt);
				return dateA.getTime() - dateB.getTime();
			})
	);

	completedOrders = $derived.by(() => {
		const today = new SvelteDate().toDateString();
		return this.orders
			.filter((o) => {
				if (o.status !== 'served') return false;
				if (!o.updatedAt) return false;
				const orderDate = new SvelteDate(o.updatedAt);
				return orderDate.toDateString() === today;
			})
			.sort((a, b) => {
				const dateA = new SvelteDate(a.updatedAt || a.createdAt);
				const dateB = new SvelteDate(b.updatedAt || b.createdAt);
				return dateB.getTime() - dateA.getTime();
			});
	});

	async init() {
		// Load initial data
		try {
			const res = await api.get(API_ENDPOINTS.kitchen.list);
			this.orders = res as Order[];
		} catch (err) {
			console.error('Failed to load orders:', err);
		}

		// Setup SignalR
		await this.setupSignalR();
	}

	private async setupSignalR() {
		this.connection = new signalR.HubConnectionBuilder()
			.withUrl(`${API_HOST_URL}${SIGNALR_HUB_PATH}`)
			.withAutomaticReconnect()
			.build();

		this.connection.on('ReceiveNewOrder', (order: Order) => {
			// Svelte 5: Direct mutation for reactive arrays
			this.orders.push(order);
		});

		this.connection.on('ReceiveOrderUpdate', (update: { id: number; status: string }) => {
			const index = this.orders.findIndex((o) => o.id === update.id);
			if (index !== -1) {
				// Svelte 5: Direct mutation for reactive objects
				this.orders[index].status = update.status;
				this.orders[index].updatedAt = new SvelteDate().toISOString();
			}
		});

		this.connection.onreconnecting(() => {
			console.log('SignalR Reconnecting...');
			this.isConnected = false;
		});

		this.connection.onreconnected(async () => {
			console.log('SignalR Reconnected');
			this.isConnected = true;
			await this.connection?.invoke('JoinGroup', 'Kitchen');
		});

		this.connection.onclose(() => {
			console.log('SignalR Disconnected');
			this.isConnected = false;
		});

		try {
			await this.connection.start();
			console.log('SignalR Connected');
			this.isConnected = true;
			await this.connection.invoke('JoinGroup', 'Kitchen');
		} catch (err) {
			console.error('SignalR Connection Error:  ', err);
		}
	}

	async destroy() {
		if (this.connection) {
			try {
				await this.connection.invoke('LeaveGroup', 'Kitchen');
			} catch (err) {
				console.error('Failed to leave Kitchen group:', err);
			}
			await this.connection.stop();
		}
		this.isConnected = false;
	}

	async updateStatus(id: number, status: string) {
		try {
			if (status === 'preparing') {
				await api.put(API_ENDPOINTS.kitchen.start(id), {});
			} else if (status === 'served') {
				await api.put(API_ENDPOINTS.kitchen.complete(id), {});
			}

			// Optimistic update with Svelte 5 direct mutation
			const index = this.orders.findIndex((o) => o.id === id);
			if (index !== -1) {
				this.orders[index].status = status;
				this.orders[index].updatedAt = new SvelteDate().toISOString();
			}
		} catch (err) {
			console.error(`Failed to update status to ${status}:`, err);
			// Rollback on error
			await this.init();
			throw new Error('Failed to update status. Please try again.');
		}
	}

	formatTime(dateStr: string | undefined): string {
		if (!dateStr) return '';
		const date = new SvelteDate(dateStr);
		return date.toLocaleTimeString('en-US', {
			hour: '2-digit',
			minute: '2-digit'
		});
	}

	getElapsedTime(dateStr: string | undefined): string {
		if (!dateStr) return '0m';
		const diff = new SvelteDate().getTime() - new SvelteDate(dateStr).getTime();
		const minutes = Math.floor(diff / 60000);
		return `${minutes}m`;
	}

	getOrderCount(tabId: 'pending' | 'preparing' | 'completed'): number {
		switch (tabId) {
			case 'pending':
				return this.pendingOrders.length;
			case 'preparing':
				return this.preparingOrders.length;
			case 'completed':
				return this.completedOrders.length;
		}
	}
}

// Singleton instance for sharing between layout and page
export const kitchenState = new KitchenState();
