<script lang="ts">
	import { onMount } from 'svelte';
	import Modal from './ui/Modal.svelte';
	import Button from './ui/Button.svelte';
	import { formatCurrency, formatTime } from '$lib/utils/index.js';
	import type { Order } from '$lib/types/index.js';
	import { posAPI, ordersAPI, api } from '$lib/api/index.js';
	import { API_ENDPOINTS } from '$lib/config/index.js';
	import Icon from './ui/Icon.svelte';

	let {
		open = $bindable(),
		order,
		onPaymentComplete
	}: {
		open: boolean;
		order: Order | null;
		onPaymentComplete: () => void;
	} = $props();

	let processing = $state(false);
	let paymentMethod = $state<'cash' | 'qr' | 'momo' | 'zalopay'>('cash');
	let cashReceived = $state(0);
	let success = $state(false);
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	let paymentSettings = $state<any>(null);

	onMount(async () => {
		try {
			const response = await api.get(API_ENDPOINTS.paymentSettings);
			paymentSettings = response;
		} catch (err) {
			console.error('[PaymentModal] Failed to load payment settings:', err);
			paymentSettings = null;
		}
	});

	// Reset state when modal opens
	$effect(() => {
		if (open && order) {
			paymentMethod = 'cash';
			cashReceived = 0;
			processing = false;
		}
	});

	/**
	 * Handles the payment confirmation process.
	 * Validates order, processes payment via API, and handles success/failure.
	 */
	async function handleConfirmPayment() {
		if (!order) return;

		if (processing) {
			return;
		}

		processing = true;

		try {
			await ordersAPI.processPayment(
				order.id,
				paymentMethod,
				paymentMethod === 'cash' ? cashReceived : undefined
			);

			success = true;

			// Immediately refresh page to update order list
			onPaymentComplete();
			window.location.reload();
		} catch (err: any) {
			console.error('[PaymentModal] Payment failed:', err);
			alert('Payment error: ' + (err.message || 'Unable to connect to server.'));
		} finally {
			processing = false;
		}
	}

	function setExactAmount() {
		if (order) cashReceived = order.totalAmount;
	}

	function addAmount(amount: number) {
		cashReceived += amount;
	}

	function appendNumber(num: number) {
		if (cashReceived >= 100000000) return; // Limit max amount
		cashReceived = cashReceived * 10 + num;
	}

	function appendTripleZero() {
		if (cashReceived >= 10000000) return; // Limit max amount
		cashReceived = cashReceived * 1000;
	}

	function backspace() {
		cashReceived = Math.floor(cashReceived / 10);
	}

	function clearAmount() {
		cashReceived = 0;
	}
</script>

<Modal
	bind:open
	title={success ? 'Payment Successful!' : `Pay Order #${order?.orderCode}`}
	onClose={() => {
		if (!success) open = false;
	}}
	closeOnOutsideClick={false}
>
	{#if order}
		{#if success}
			<div class="flex flex-col items-center justify-center py-8 text-center">
				<div class="mb-4 flex h-20 w-20 items-center justify-center rounded-full bg-green-100">
					<Icon name="tabler:check" class="h-10 w-10 text-green-600" />
				</div>
				<h3 class="mb-2 text-xl font-bold text-gray-900">Payment Complete!</h3>
				<p class="text-gray-500">Closing dialog...</p>
			</div>
		{:else}
			<div class="space-y-4">
				<!-- Order Items -->
				<div class="mb-4 overflow-hidden rounded-lg border">
					<table class="w-full text-left text-sm">
						<thead class="border-b bg-gray-50 font-medium text-gray-700">
							<tr>
								<th class="p-2">Item</th>
								<th class="p-2 text-center">Qty</th>
								<th class="p-2 text-right">Unit Price</th>
								<th class="p-2 text-right">Total</th>
							</tr>
						</thead>
						<tbody class="divide-y divide-gray-100">
							{#each order.items as item (item.menuItemId || item.comboId || item.menuItemName)}
								<tr>
									<td class="p-2">
										<div class="font-medium">{item.menuItemName || item.comboName}</div>
										{#if item.addons && item.addons.length > 0}
											<div class="text-xs text-gray-500">
												+ {item.addons.map((a) => a.addonName).join(', ')}
											</div>
										{/if}
									</td>
									<td class="p-2 text-center">{item.quantity}</td>
									<td class="p-2 text-right">{formatCurrency(item.unitPrice)}</td>
									<td class="p-2 text-right font-medium">{formatCurrency(item.totalPrice)}</td>
								</tr>
							{/each}
						</tbody>
						<tfoot class="bg-gray-50 font-medium">
							<tr>
								<td colspan="3" class="p-2 text-right">Subtotal:</td>
								<td class="p-2 text-right">{formatCurrency(order.subtotalAmount)}</td>
							</tr>
							{#if order.discountAmount > 0}
								<tr class="text-green-600">
									<td colspan="3" class="p-2 text-right">Discount:</td>
									<td class="p-2 text-right">-{formatCurrency(order.discountAmount)}</td>
								</tr>
							{/if}
							{#if order.vatAmount && order.vatAmount > 0}
								<tr class="text-gray-600">
									<td colspan="3" class="p-2 text-right">VAT (10%):</td>
									<td class="p-2 text-right">+{formatCurrency(order.vatAmount)}</td>
								</tr>
							{/if}
							<tr class="text-base font-bold text-indigo-600">
								<td colspan="3" class="p-2 text-right">Total:</td>
								<td class="p-2 text-right">{formatCurrency(order.totalAmount)}</td>
							</tr>
						</tfoot>
					</table>
				</div>

				<!-- Order History -->
				{#if order.history && order.history.length > 0}
					<div class="mb-6">
						<h4 class="mb-2 text-sm font-medium text-gray-900">Order History</h4>
						<div
							class="max-h-40 space-y-2 overflow-y-auto rounded-lg border border-gray-100 bg-gray-50 p-3"
						>
							{#each order.history as event (event.changedAt)}
								<div class="flex gap-3 text-xs">
									<div class="w-24 pt-0.5 text-gray-500">{formatTime(event.changedAt)}</div>
									<div class="flex-1">
										<span class="font-medium text-gray-800">
											{event.toStatus === 'pending'
												? 'Order created'
												: event.toStatus === 'paid'
													? 'Paid'
													: event.toStatus === 'preparing'
														? 'Preparing'
														: event.toStatus === 'served'
															? 'Served'
															: event.toStatus}
										</span>
										{#if event.note}
											<span class="mx-1 text-gray-500">-</span>
											<span class="text-gray-500 italic">{event.note}</span>
										{/if}
									</div>
								</div>
							{/each}
						</div>
					</div>
				{/if}

				<!-- Payment Method Tabs -->
				<div class="mb-6 flex rounded-md bg-gray-100 p-1">
					<button
						class="flex-1 rounded-md px-3 py-2 text-sm font-medium transition-all {paymentMethod ===
						'cash'
							? 'bg-white text-indigo-600 shadow-sm'
							: 'text-gray-500 hover:text-gray-700'}"
						onclick={() => (paymentMethod = 'cash')}
					>
						💵 Cash
					</button>
					<button
						class="flex-1 rounded-md px-3 py-2 text-sm font-medium transition-all {paymentMethod ===
						'qr'
							? 'bg-white text-indigo-600 shadow-sm'
							: 'text-gray-500 hover:text-gray-700'}"
						onclick={() => (paymentMethod = 'qr')}
					>
						📱 QR Code
					</button>

					<button
						class="flex-1 rounded-md px-3 py-2 text-sm font-medium transition-all {paymentMethod ===
						'momo'
							? 'bg-white text-pink-600 shadow-sm'
							: 'text-gray-500 hover:text-gray-700'}"
						onclick={() => (paymentMethod = 'momo')}
					>
						👛 Momo
					</button>
					<button
						class="flex-1 rounded-md px-3 py-2 text-sm font-medium transition-all {paymentMethod ===
						'zalopay'
							? 'bg-white text-blue-600 shadow-sm'
							: 'text-gray-500 hover:text-gray-700'}"
						onclick={() => (paymentMethod = 'zalopay')}
					>
						ZaloPay
					</button>
				</div>

				<div class="py-2 text-center">
					<div class="mb-1 text-sm text-gray-500">Total to pay</div>
					<div class="text-3xl font-bold text-indigo-600">{formatCurrency(order.totalAmount)}</div>
				</div>

				{#if paymentMethod === 'cash'}
					<div class="space-y-4 rounded-xl border border-gray-200 bg-gray-50 p-4">
						<div>
							<label for="cashReceived" class="mb-1 block text-sm font-medium text-gray-700"
								>Amount received</label
							>
							<div class="relative mb-4">
								<input
									id="cashReceived"
									type="text"
									value={formatCurrency(cashReceived).replace('₫', '').trim()}
									readonly
									class="w-full rounded-lg border border-gray-300 bg-white py-3 pr-12 pl-4 text-right text-2xl font-bold focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500"
								/>
								<span class="absolute top-1/2 right-4 -translate-y-1/2 font-medium text-gray-500"
									>₫</span
								>
							</div>

							<!-- Calculator Grid -->
							<div class="grid grid-cols-4 gap-2">
								<!-- Quick Amounts -->
								<button
									onclick={() => addAmount(10000)}
									class="rounded-lg bg-blue-50 p-3 font-semibold text-blue-700 hover:bg-blue-100"
									>+10k</button
								>
								<button
									onclick={() => addAmount(20000)}
									class="rounded-lg bg-blue-50 p-3 font-semibold text-blue-700 hover:bg-blue-100"
									>+20k</button
								>
								<button
									onclick={() => addAmount(50000)}
									class="rounded-lg bg-blue-50 p-3 font-semibold text-blue-700 hover:bg-blue-100"
									>+50k</button
								>
								<button
									onclick={() => addAmount(100000)}
									class="rounded-lg bg-blue-50 p-3 font-semibold text-blue-700 hover:bg-blue-100"
									>+100k</button
								>

								<!-- Numpad -->
								<button
									onclick={() => appendNumber(7)}
									class="rounded-lg border border-gray-200 bg-white p-4 text-xl font-medium hover:bg-gray-50"
									>7</button
								>
								<button
									onclick={() => appendNumber(8)}
									class="rounded-lg border border-gray-200 bg-white p-4 text-xl font-medium hover:bg-gray-50"
									>8</button
								>
								<button
									onclick={() => appendNumber(9)}
									class="rounded-lg border border-gray-200 bg-white p-4 text-xl font-medium hover:bg-gray-50"
									>9</button
								>
								<button
									onclick={backspace}
									class="rounded-lg border border-red-100 bg-red-50 p-4 text-red-600 hover:bg-red-100"
									>⌫</button
								>

								<button
									onclick={() => appendNumber(4)}
									class="rounded-lg border border-gray-200 bg-white p-4 text-xl font-medium hover:bg-gray-50"
									>4</button
								>
								<button
									onclick={() => appendNumber(5)}
									class="rounded-lg border border-gray-200 bg-white p-4 text-xl font-medium hover:bg-gray-50"
									>5</button
								>
								<button
									onclick={() => appendNumber(6)}
									class="rounded-lg border border-gray-200 bg-white p-4 text-xl font-medium hover:bg-gray-50"
									>6</button
								>
								<button
									onclick={clearAmount}
									class="rounded-lg border border-gray-200 bg-gray-100 p-4 text-gray-600 hover:bg-gray-200"
									>C</button
								>

								<button
									onclick={() => appendNumber(1)}
									class="rounded-lg border border-gray-200 bg-white p-4 text-xl font-medium hover:bg-gray-50"
									>1</button
								>
								<button
									onclick={() => appendNumber(2)}
									class="rounded-lg border border-gray-200 bg-white p-4 text-xl font-medium hover:bg-gray-50"
									>2</button
								>
								<button
									onclick={() => appendNumber(3)}
									class="rounded-lg border border-gray-200 bg-white p-4 text-xl font-medium hover:bg-gray-50"
									>3</button
								>
								<button
									onclick={setExactAmount}
									class="row-span-2 flex items-center justify-center rounded-lg border border-indigo-100 bg-indigo-50 p-4 font-bold text-indigo-700 hover:bg-indigo-100"
									>Exact amount</button
								>

								<button
									onclick={() => appendNumber(0)}
									class="col-span-2 rounded-lg border border-gray-200 bg-white p-4 text-xl font-medium hover:bg-gray-50"
									>0</button
								>
								<button
									onclick={() => appendTripleZero()}
									class="rounded-lg border border-gray-200 bg-white p-4 text-xl font-medium hover:bg-gray-50"
									>000</button
								>
							</div>
						</div>

						<div class="flex items-center justify-between border-t border-gray-200 pt-2">
							<span class="font-medium text-gray-600">Change:</span>
							<span
								class="text-xl font-bold {cashReceived >= order.totalAmount
									? 'text-green-600'
									: 'text-red-500'}"
							>
								{formatCurrency(Math.max(0, cashReceived - order.totalAmount))}
							</span>
						</div>

						{#if cashReceived < order.totalAmount}
							<div class="text-center text-sm text-red-500">
								Insufficient amount: {formatCurrency(order.totalAmount - cashReceived)}
							</div>
						{/if}
					</div>
				{:else if paymentMethod === 'momo' || paymentMethod === 'zalopay'}
					<div
						class="flex min-h-[300px] flex-col items-center justify-center rounded-xl border border-gray-200 bg-gray-50"
					>
						<div class="mb-4 text-4xl">🚧</div>
						<h3 class="mb-2 text-lg font-bold text-gray-900">Integration coming soon</h3>
						<p class="max-w-xs text-center text-gray-500">
							Payment via {paymentMethod === 'momo' ? 'Momo' : 'ZaloPay'} is under development. Please
							choose another method.
						</p>
					</div>
				{:else}
					<div class="flex min-h-[300px] flex-col items-center justify-center">
						{#if order.qrPaymentUrl}
							<img
								src={order.qrPaymentUrl}
								alt="VietQR Payment"
								class="h-64 w-64 rounded-lg border border-gray-200 bg-white object-contain shadow-sm"
								onerror={() => console.error('[PaymentModal] QR image failed to load')}
							/>
							<p class="mt-2 text-center text-sm text-gray-500">
								Scan QR code to pay<br />
								<span class="font-bold">{formatCurrency(order.totalAmount)}</span>
							</p>
						{:else}
							<div class="rounded-lg border border-yellow-100 bg-yellow-50 p-8 text-center">
								<div class="mb-2 text-4xl">⚠️</div>
								<p class="font-medium text-yellow-800">No QR code available</p>
								<p class="mt-1 text-sm text-yellow-600">Please check payment configuration.</p>
							</div>
						{/if}
					</div>
				{/if}

				<div class="mt-4 flex gap-3 border-t pt-4">
					<Button variant="secondary" fullWidth={true} onclick={() => (open = false)}>Close</Button>
					<Button
						variant="primary"
						fullWidth={true}
						onclick={handleConfirmPayment}
						disabled={processing ||
							(paymentMethod === 'cash' && cashReceived < order.totalAmount) ||
							paymentMethod === 'momo' ||
							paymentMethod === 'zalopay'}
					>
						{processing ? 'Processing...' : 'Confirm Payment Received'}
					</Button>
				</div>
			</div>
		{/if}
	{/if}
</Modal>
