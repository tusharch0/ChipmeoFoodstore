<script lang="ts">
	import { goto } from '$app/navigation';
	import { cart, cartTotals, cartActions } from '$lib/utils/index.js';
	import { posAPI } from '$lib/api/index.ts';
	import { formatCurrency } from '$lib/utils/index.ts';
	import Button from '$lib/components/ui/Button.svelte';
	import Card from '$lib/components/ui/Card.svelte';
	import Toast from '$lib/components/ui/Toast.svelte';
	import Icon from '$lib/components/ui/Icon.svelte';

	let paymentMethod = $state<'cash' | 'banking' | 'qr'>('cash');
	let customerName = $state($cart.customerName || '');
	let customerPhone = $state($cart.customerPhone || '');
	let note = $state($cart.note || '');
	let cashReceived = $state<number>(0);
	let loading = $state(false);
	let showToast = $state(false);
	let toastMessage = $state('');
	let toastType = $state<'success' | 'error'>('success');

	const denominations = [50000, 100000, 200000, 500000, 1000000];
	const changeAmount = $derived(Math.max(0, cashReceived - $cartTotals.total));

	// Payment options
	const paymentOptions = [
		{ id: 'cash', name: 'Cash', icon: '💵', description: 'Cash payment' },
		{ id: 'banking', name: 'Bank Transfer', icon: '🏦', description: 'Bank transfer' },
		{ id: 'qr', name: 'QR Code', icon: '📱', description: 'Scan QR code to pay' }
	];

	async function handleCheckout() {
		if ($cart.items.length === 0) {
			toastMessage = 'Cart is empty!';
			toastType = 'error';
			showToast = true;
			return;
		}

		loading = true;

		try {
			// Prepare order data for backend API
			const orderData = {
				sourceId: $cart.selectedSource?.id,
				customerName: customerName || undefined,
				customerPhone: customerPhone || undefined,
				note: note || undefined,
				discountCode: $cart.discount?.code,
				items: $cart.items.map((item) => ({
					menuItemId: item.menuItem?.id,
					comboId: item.combo?.id,
					quantity: item.quantity,
					note: item.note,
					addons: item.selectedAddons.map((addon) => ({
						addonId: addon.addon.id,
						quantity: addon.quantity
					}))
				}))
			};

			// Call REAL backend API to create order
			const createdOrder = await posAPI.createOrder(orderData);

			toastMessage = `Order ${createdOrder.orderCode} created successfully!`;
			toastType = 'success';
			showToast = true;

			// Clear cart and redirect after success
			await new Promise((resolve) => setTimeout(resolve, 2000));
			cartActions.clearCart();
			await goto('/pos');
		} catch (error) {
			console.error('Failed to create order:', error);
			const message = error instanceof Error ? error.message : 'Unable to connect to server';
			toastMessage = 'Failed to create order: ' + message;
			toastType = 'error';
			showToast = true;
		} finally {
			loading = false;
		}
	}

	async function goBack() {
		await goto('/pos');
	}
</script>

<svelte:head>
	<title>Checkout - Foodstore POS</title>
</svelte:head>

<div class="min-h-screen bg-gray-50">
	<!-- Header -->
	<header class="border-b border-gray-200 bg-white shadow-sm">
		<div class="flex items-center justify-between px-6 py-4">
			<div class="flex items-center gap-4">
				<button
					onclick={goBack}
					class="rounded-lg p-2 transition-colors hover:bg-gray-100"
					aria-label="Go back"
				>
					<Icon name="tabler:chevron-left" class="h-6 w-6" />
				</button>
				<div>
					<h1 class="text-2xl font-bold text-gray-900">Checkout</h1>
					<p class="text-sm text-gray-500">Complete the order</p>
				</div>
			</div>
		</div>
	</header>

	<div class="mx-auto max-w-6xl p-6">
		<div class="grid grid-cols-1 gap-6 lg:grid-cols-3">
			<!-- Left: Order Summary -->
			<div class="space-y-6 lg:col-span-2">
				<!-- Customer Info -->
				<Card>
					<h2 class="mb-4 text-lg font-semibold text-gray-900">Customer Information</h2>
					<div class="grid grid-cols-1 gap-4 md:grid-cols-2">
						<div>
							<label for="customerName" class="mb-2 block text-sm font-medium text-gray-700"
								>Customer name</label
							>
							<input
								id="customerName"
								type="text"
								bind:value={customerName}
								placeholder="Enter name (optional)"
								class="w-full rounded-lg border border-gray-300 px-4 py-2 focus:border-transparent focus:ring-2 focus:ring-indigo-500"
							/>
						</div>
						<div>
							<label for="customerPhone" class="mb-2 block text-sm font-medium text-gray-700"
								>Phone number</label
							>
							<input
								id="customerPhone"
								type="tel"
								bind:value={customerPhone}
								placeholder="Enter phone (optional)"
								class="w-full rounded-lg border border-gray-300 px-4 py-2 focus:border-transparent focus:ring-2 focus:ring-indigo-500"
							/>
						</div>
					</div>
					<div class="mt-4">
						<label for="orderNote" class="mb-2 block text-sm font-medium text-gray-700"
							>Note</label
						>
						<textarea
							id="orderNote"
							bind:value={note}
							rows="2"
							placeholder="Special instructions..."
							class="w-full rounded-lg border border-gray-300 px-4 py-2 focus:border-transparent focus:ring-2 focus:ring-indigo-500"
						></textarea>
					</div>
				</Card>

				<!-- Payment Method -->
				<Card>
					<div class="mb-4 flex items-center justify-between">
						<h2 class="text-lg font-semibold text-gray-900">Payment Method</h2>
						<div class="flex gap-2">
							{#each paymentOptions as option (option.id)}
								<button
									onclick={() => (paymentMethod = option.id as 'cash' | 'banking' | 'qr')}
									class="rounded-full px-4 py-1.5 text-xs font-bold transition-all
										{paymentMethod === option.id
										? 'bg-indigo-600 text-white shadow-md'
										: 'bg-gray-100 text-gray-500 hover:bg-gray-200'}"
								>
									{option.name}
								</button>
							{/each}
						</div>
					</div>

					{#if paymentMethod === 'cash'}
						<div class="space-y-4 duration-300 animate-in slide-in-from-top-2">
							<div>
								<label for="cashReceived" class="mb-2 block text-sm font-medium text-gray-700"
									>Amount received</label
								>
								<div class="relative">
									<input
										id="cashReceived"
										type="number"
										bind:value={cashReceived}
										class="w-full rounded-xl border-gray-200 bg-gray-50 py-3 pr-12 pl-4 text-2xl font-black text-indigo-600 focus:border-indigo-500 focus:ring-indigo-500"
										placeholder="0"
									/>
									<span class="absolute top-1/2 right-4 -translate-y-1/2 font-bold text-gray-400"
										>₫</span
									>
								</div>
							</div>

							<div class="flex flex-wrap gap-2">
								{#each denominations as amount}
									<button
										onclick={() => (cashReceived = amount)}
										class="rounded-lg border border-indigo-100 bg-white px-3 py-2 text-sm font-bold text-indigo-600 shadow-sm transition-all hover:bg-indigo-50 active:scale-95"
									>
										{amount >= 1000000 ? amount / 1000000 + 'M' : amount / 1000 + 'K'}
									</button>
								{/each}
								<button
									onclick={() => (cashReceived = $cartTotals.total)}
									class="rounded-lg border border-green-200 bg-green-50 px-3 py-2 text-sm font-bold text-green-700 shadow-sm transition-all hover:bg-green-100 active:scale-95"
								>
									Exact amount
								</button>
							</div>

							<div class="flex items-center justify-between rounded-xl bg-gray-900 p-4 text-white">
								<span class="text-sm font-medium opacity-70">Change due</span>
								<span class="text-2xl font-black text-emerald-400"
									>{formatCurrency(changeAmount)}</span
								>
							</div>
						</div>
					{:else}
						<div
							class="flex flex-col items-center justify-center rounded-xl border-2 border-dashed border-gray-200 p-10 text-center duration-500 animate-in fade-in"
						>
							<div class="mb-3 text-5xl">
								{paymentMethod === 'banking' ? '🏦' : '📱'}
							</div>
							<p class="font-bold text-gray-900">
								The system will generate a {paymentMethod.toUpperCase()} code
							</p>
							<p class="text-sm text-gray-500">
								Please guide the customer to scan the QR code or make a bank transfer
							</p>
						</div>
					{/if}
				</Card>

				<!-- Order Items -->
				<Card>
					<h2 class="mb-4 text-lg font-semibold text-gray-900">
						Order Items ({$cart.items.length})
					</h2>
					<div class="space-y-3">
						{#each $cart.items as item, index (index)}
							<div class="flex items-start justify-between rounded-lg bg-gray-50 p-3">
								<div class="flex-1">
									<div class="mb-1 flex items-center gap-2">
										<span class="font-semibold text-gray-900">{item.quantity}x</span>
										<span class="font-medium text-gray-900"
											>{item.menuItem?.name || item.combo?.name}</span
										>
									</div>
									{#if item.selectedAddons.length > 0}
										<div class="ml-8 text-xs text-gray-600">
											+ {item.selectedAddons.map((a) => a.addon.name).join(', ')}
										</div>
									{/if}
									{#if item.note}
										<div class="ml-8 text-xs text-gray-500 italic">Note: {item.note}</div>
									{/if}
								</div>
								<div class="font-semibold text-gray-900">{formatCurrency(item.subtotal)}</div>
							</div>
						{/each}
					</div>
				</Card>
			</div>

			<!-- Right: Payment Summary -->
			<div class="lg:col-span-1">
				<div class="sticky top-6">
					<Card>
						<h2 class="mb-4 text-lg font-semibold text-gray-900">Summary</h2>

						{#if $cart.selectedSource}
							<div class="mb-4 rounded-lg bg-indigo-50 p-3">
								<div class="text-sm text-indigo-700">Order Source:</div>
								<div class="font-semibold text-indigo-900">{$cart.selectedSource.name}</div>
							</div>
						{/if}

						<div class="mb-6 space-y-3">
							<div class="flex justify-between text-sm">
								<span class="text-gray-600">Subtotal</span>
								<span class="font-semibold">{formatCurrency($cartTotals.subtotal)}</span>
							</div>
							{#if $cartTotals.discountAmount > 0}
								<div class="flex justify-between text-sm text-green-600">
									<span>Discount</span>
									<span class="font-semibold">-{formatCurrency($cartTotals.discountAmount)}</span>
								</div>
							{/if}
							<div class="flex justify-between border-t pt-3">
								<span class="text-lg font-semibold text-gray-900">Total</span>
								<span class="text-2xl font-bold text-indigo-600"
									>{formatCurrency($cartTotals.total)}</span
								>
							</div>
						</div>

						<Button
							variant="primary"
							fullWidth={true}
							onclick={handleCheckout}
							disabled={loading || $cart.items.length === 0}
						>
							{loading ? 'Processing...' : 'Confirm Payment'}
						</Button>

						<Button variant="secondary" fullWidth={true} onclick={goBack} disabled={loading}>
							Go Back
						</Button>
					</Card>
				</div>
			</div>
		</div>
	</div>
</div>

<Toast bind:show={showToast} message={toastMessage} type={toastType} />
