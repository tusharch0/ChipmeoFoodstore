import { get } from 'svelte/store';
import { categories, menuItems, combos, addons, sources, discounts } from '$lib/utils/index.js';
import { cart, cartActions } from '$lib/utils/index.js';
import { posAPI, ordersAPI, customerAPI } from '$lib/api/index.js';
import { signalRService } from '$lib/services/signalr.js';
import type { MenuItem, Addon, Source, Order, Combo, Customer } from '$lib/types/index.js';
import { DEFAULT_CUSTOMER_PASSWORD, STORAGE_KEYS } from '$lib/config/index.js';

class POSStore {
	// UI State
	selectedCategoryId = $state<number | null>(null);
	searchQuery = $state('');
	loading = $state(true);

	// Toast State
	showToast = $state(false);
	toastMessage = $state('');
	toastType = $state<'success' | 'error'>('success');

	// Modals State
	showAddonModal = $state(false);
	showSourceModal = $state(false);
	showOrdersModal = $state(false);
	showPaymentModal = $state(false);
	isCartOpen = $state(false);

	// Selection State
	selectedMenuItem = $state<MenuItem | null>(null);
	selectedAddons = $state<{ addon: Addon; quantity: number }[]>([]);
	itemNote = $state('');
	itemQuantity = $state(1);

	// Order Processing State
	pendingOrders = $state<Order[]>([]);
	loadingOrders = $state(false);
	selectedOrder = $state<Order | null>(null);
	editingOrder = $state<{ id: string; code: string } | null>(null);

	// Data State (Synced from Stores)
	localMenuItems = $state<MenuItem[]>([]);
	unsubscribers: (() => void)[] = [];

	constructor() {
		// Initialization logic if needed
	}

	async init() {
		try {
			this.loading = true;

			// Sync menuItems store to local state
			const unsub = menuItems.subscribe((items) => {
				this.localMenuItems = items;
			});
			this.unsubscribers.push(unsub);

			await this.loadMenuData();
			await this.loadSources();
			await this.initSignalR();
			this.showNotification('Data loaded successfully!', 'success');
		} catch (error) {
			console.error('Failed to load data:', error);
			const message = error instanceof Error ? error.message : 'Unable to connect to server';
			this.showNotification('Failed to load data: ' + message, 'error');
		} finally {
			this.loading = false;
		}
	}

	async loadMenuData() {
		const menuData = await posAPI.getMenuData();
		categories.set(menuData.categories);
		menuItems.set(menuData.menuItems);
		addons.set(menuData.addons);
		discounts.set(menuData.discounts);
		if (menuData.combos) combos.set(menuData.combos);
	}

	async loadSources() {
		const sourcesData = await posAPI.getSources();
		sources.set(sourcesData);
	}

	async initSignalR() {
		const token = localStorage.getItem(STORAGE_KEYS.TOKEN);
		if (token) {
			await signalRService.startConnection(token);
			signalRService.on('ReceiveMenuUpdate', this.handleMenuUpdate);
			signalRService.on('ReceiveComboUpdate', this.handleMenuUpdate);
			signalRService.on('ReceiveSourceUpdate', this.handleSourceUpdate);
			signalRService.on('ReceiveDiscountUpdate', this.handleMenuUpdate);
		}
	}

	handleMenuUpdate = () => {
		this.loadMenuData();
		this.showNotification('Menu data has been updated!', 'success');
	};

	handleSourceUpdate = () => {
		this.loadSources();
		this.showNotification('Order source status has been updated!', 'success');
	};

	cleanup() {
		this.unsubscribers.forEach((unsub) => unsub());
		this.unsubscribers = [];
		signalRService.off('ReceiveMenuUpdate', this.handleMenuUpdate);
		signalRService.off('ReceiveComboUpdate', this.handleMenuUpdate);
		signalRService.off('ReceiveSourceUpdate', this.handleSourceUpdate);
		signalRService.off('ReceiveDiscountUpdate', this.handleMenuUpdate);
	}

	generatePassword(fullName: string): string {
		if (!fullName) return DEFAULT_CUSTOMER_PASSWORD;
		// Get initials (e.g. "John Doe" -> "jd")
		const initials = fullName
			.trim()
			.split(/\s+/)
			.map((word) => word.charAt(0).toLowerCase())
			.join('');

		// Get date (ddMMyyyy)
		const now = new Date();
		const day = String(now.getDate()).padStart(2, '0');
		const month = String(now.getMonth() + 1).padStart(2, '0');
		const year = now.getFullYear();

		return `${initials}@${day}${month}${year}`;
	}

	// Getters
	get filteredMenuItems() {
		let items = this.localMenuItems;

		if (this.selectedCategoryId) {
			items = items.filter((item: MenuItem) => item.categoryId === this.selectedCategoryId);
		}

		if (this.searchQuery) {
			items = items.filter((item: MenuItem) =>
				item.name.toLowerCase().includes(this.searchQuery.toLowerCase())
			);
		}
		return items;
	}

	// Actions
	showNotification(message: string, type: 'success' | 'error' = 'success') {
		this.toastMessage = message;
		this.toastType = type;
		this.showToast = true;
	}

	openAddonModal(item: MenuItem) {
		this.selectedMenuItem = item;
		this.selectedAddons = [];
		this.itemNote = '';
		this.itemQuantity = 1;
		this.showAddonModal = true;
	}

	toggleAddon(addon: Addon) {
		const index = this.selectedAddons.findIndex((a) => a.addon.id === addon.id);
		if (index === -1) {
			this.selectedAddons.push({ addon, quantity: 1 });
		} else {
			this.selectedAddons.splice(index, 1);
		}
	}

	addToCart() {
		if (!this.selectedMenuItem) return;

		cartActions.addItem({
			menuItem: this.selectedMenuItem,
			quantity: this.itemQuantity,
			selectedAddons: this.selectedAddons,
			note: this.itemNote,
			subtotal:
				(this.selectedMenuItem.price +
					this.selectedAddons.reduce((sum, a) => sum + a.addon.price, 0)) *
				this.itemQuantity
		});

		this.showAddonModal = false;
		this.showNotification('Added to cart!', 'success');
	}

	handleComboClick(combo: Combo) {
		cartActions.addItem({
			combo: combo,
			menuItem: undefined,
			quantity: 1,
			note: '',
			selectedAddons: [],
			subtotal: combo.comboPrice
		});
		this.showNotification(`Added ${combo.name} to cart`, 'success');
	}

	selectSource(source: Source) {
		cartActions.setSource(source);
		this.showSourceModal = false;
		this.showNotification(`Selected ${source.name}`, 'success');
	}

	async fetchPendingOrders() {
		try {
			this.loadingOrders = true;
			this.pendingOrders = await ordersAPI.getByStatus('pending');
		} catch (error) {
			console.error('Failed to fetch orders:', error);
			this.showNotification('Failed to load order list', 'error');
		} finally {
			this.loadingOrders = false;
		}
	}

	openOrdersModal() {
		this.showOrdersModal = true;
		this.fetchPendingOrders();
	}

	openPaymentModal(order: Order) {
		this.selectedOrder = order;
		this.showPaymentModal = true;
	}

	async cancelOrder(orderId: string) {
		if (!confirm('Are you sure you want to cancel this order?')) return;

		try {
			// Change from delete to soft cancel (update status to cancelled)
			await posAPI.updateOrderStatus(orderId, 'cancelled');
			this.showNotification('Order cancelled', 'success');
			this.fetchPendingOrders();
		} catch (error) {
			console.error('Failed to cancel order:', error);
			this.showNotification('Failed to cancel order', 'error');
		}
	}

	clearCartAndEditState() {
		cartActions.clearCart();
		this.editingOrder = null;
		this.showNotification('Cart cleared', 'success');
	}

	async adjustOrder(order: Order) {
		try {
			// 1. Clear current cart
			cartActions.clearCart();
			this.editingOrder = { id: order.id, code: order.orderCode };

			// 2. Load Source
			const source = get(sources).find(
				(s) => s.name === order.sourceName || s.id === order.sourceId
			);
			if (source) cartActions.setSource(source);

			// Load Discount
			if (order.discountId && order.discountCode) {
				const discount = get(discounts).find((d) => d.id === order.discountId);
				if (discount) cartActions.setDiscount(discount);
			}

			// 3. Load Customer if possible
			if (order.customerId) {
				const { customerAPI } = await import('$lib/api/index.js');
				this.selectedCustomer = await customerAPI
					.getCustomerById(order.customerId)
					.catch(() => null);
			}

			// 4. Load items back to cart
			const allMenuItems = get(menuItems);
			const allCombos = get(combos);

			for (const item of order.items) {
				if (item.menuItemId) {
					const menuItem = allMenuItems.find((m) => m.id === item.menuItemId);
					if (menuItem) {
						cartActions.addItem({
							menuItem,
							quantity: item.quantity,
							note: item.note,
							selectedAddons:
								item.addons?.map((a) => ({
									addon: {
										id: a.addonId,
										name: a.addonName,
										// Map unitPrice from backend to price in frontend Addon
										price: a.unitPrice || 0
									} as Addon,
									quantity: a.quantity
								})) || [],
							subtotal: item.totalPrice
						});
					}
				} else if (item.comboId) {
					const combo = allCombos.find((c) => c.id === item.comboId);
					if (combo) {
						cartActions.addItem({
							combo,
							quantity: item.quantity,
							note: item.note,
							selectedAddons: [],
							subtotal: item.totalPrice
						});
					}
				}
			}

			// 5. NO DELETE original order

			// 6. UI Updates
			this.showOrdersModal = false;
			this.isCartOpen = true;
			this.showNotification(`Editing order #${order.orderCode}`, 'success');
		} catch (error) {
			console.error('Failed to adjust order:', error);
			this.showNotification('Failed to reload order', 'error');
			this.editingOrder = null; // Reset if fail
		}
	}

	handlePaymentComplete() {
		this.showNotification('Payment successful!', 'success');
		this.fetchPendingOrders();
	}

	// Customer State
	selectedCustomer = $state<Customer | null>(null);
	showCustomerModal = $state(false);
	customerPhone = $state('');

	handleLogout() {
		window.location.href = '/logout';
	}

	// Customer Actions
	async lookupCustomer(phone: string) {
		if (!phone) return;
		try {
			const { customerAPI } = await import('$lib/api/index.js');
			this.selectedCustomer = await customerAPI.lookupByPhone(phone);
			this.showNotification(`Selected customer: ${this.selectedCustomer.fullName}`, 'success');
			this.showCustomerModal = false;
		} catch {
			this.showNotification('Customer not found. Please create a new one.', 'error');
			this.selectedCustomer = null;
		}
	}

	async createCustomer(data: { fullName: string; phone: string; email?: string }) {
		try {
			const { customerAPI } = await import('$lib/api/index.js');
			// Backend expects CustomerRegisterDto, but POS might just ask for name/phone
			// We can use register or create a specific pos-create endpoint if needed.
			// Using register for now with dummy password or null if allowed?
			// Register requires password. Let's use a default password for POS created customers or update backend to allow passwordless creation.
			// Backend `CreateAsync` (Admin) allows creation without password logic? No, check CustomerService.CreateAsync.
			// It hashes password.
			// Let's assume we use a default password "123456" for POS created customers for now or ask backend refactor.
			// Ideally we should use the Admin Create which doesn't require "Login" flow but creates the record.

			// Wait, CustomerService.CreateAsync (Admin) implementation:
			// var customer = new Customer { ..., PasswordHash = Hash(dto.Password) ... }
			// So we need a password.

			const newCustomer = await customerAPI.register({
				fullName: data.fullName,
				email: data.email || `pos_customer_${Date.now()}@foodstore.local`, // Auto-gen email if not provided
				phone: data.phone,
				password: this.generatePassword(data.fullName) // Auto-gen: initials@date
			});

			// Show password hint toast
			this.showNotification(
				`Customer created! Default password: ${this.generatePassword(data.fullName)}`,
				'success'
			);

			this.selectedCustomer = newCustomer;
			this.showNotification('New customer created!', 'success');
			this.showCustomerModal = false;
			this.showCustomerModal = false;
		} catch (error) {
			const message = error instanceof Error ? error.message : 'Failed to create customer';
			this.showNotification(message, 'error');
		}
	}

	removeCustomer() {
		this.selectedCustomer = null;
		this.customerPhone = '';
		this.showNotification('Customer deselected', 'success');
	}

	async handlePlaceOrder() {
		const currentCart = get(cart);
		if (currentCart.items.length === 0) {
			this.showNotification('Cart is empty!', 'error');
			return;
		}
		if (!currentCart.selectedSource) {
			this.showNotification('Please select an order source before placing an order!', 'error');
			this.showSourceModal = true;
			return;
		}

		try {
			this.loading = true;

			const orderData = {
				sourceId: currentCart.selectedSource.id,
				customerId: this.selectedCustomer?.id, // Send CustomerID
				items: currentCart.items.map((item) => ({
					menuItemId: item.menuItem?.id,
					comboId: item.combo?.id,
					quantity: item.quantity,
					note: item.note,
					addons: item.selectedAddons.map((a) => ({
						addonId: a.addon.id,
						quantity: a.quantity
					}))
				})),
				discountCode: currentCart.discount?.code
			};

			let orderResult: Order;

			if (this.editingOrder) {
				// Update existing order
				orderResult = await posAPI.updateOrder(this.editingOrder.id, orderData);
				this.showNotification(`Order #${this.editingOrder.code} updated!`, 'success');
			} else {
				// Create new order
				orderResult = await posAPI.createOrder(orderData);
				this.showNotification('New order created!', 'success');
			}

			cartActions.clearCart();
			// Keep customer selected or clear? Usually keep for next order? No, usually clear.
			this.selectedCustomer = null;
			this.customerPhone = '';
			this.editingOrder = null; // Clear edit state

			// Open payment modal immediately
			this.openPaymentModal(orderResult);
		} catch (error) {
			console.error('Order failed:', error);
			const message = error instanceof Error ? error.message : 'Unknown error';
			this.showNotification('Order processing error: ' + message, 'error');
		} finally {
			this.loading = false;
		}
	}
}

export const posStore = new POSStore();
