<script lang="ts">
	import { onMount, onDestroy, tick } from 'svelte';
	import { goto } from '$app/navigation';
	import { Dropdown } from 'flowbite';
	import { categories, combos, sources, discounts, auth } from '$lib/utils/index.js';
	import { cart, cartTotals, cartActions } from '$lib/utils/index.js';
	import { formatCurrency, formatTime } from '$lib/utils/index.js';
	import Button from '$lib/components/ui/Button.svelte';
	import Modal from '$lib/components/ui/Modal.svelte';
	import Toast from '$lib/components/ui/Toast.svelte';
	import PaymentModal from '$lib/components/PaymentModal.svelte';
	import { posStore } from './store.svelte.js';
	import type { Addon } from '$lib/types/index.js';
	import Icon from '$lib/components/ui/Icon.svelte';

	let userDropdown = $state<Dropdown | null>(null);

	onMount(async () => {
		await tick();
		const triggerEl = document.getElementById('userDropdownButton');
		const targetEl = document.getElementById('userDropdown');
		if (triggerEl && targetEl) {
			userDropdown = new Dropdown(targetEl, triggerEl, {
				placement: 'bottom-start'
			});
		}
		posStore.init();
	});

	onDestroy(() => {
		userDropdown?.destroy();
		posStore.cleanup();
	});

	function toggleUserDropdown() {
		userDropdown?.toggle();
	}

	function handleLogout() {
		posStore.handleLogout();
	}
</script>

<svelte:head><title>POS - Foodstore</title></svelte:head>

<div class="flex h-screen overflow-hidden bg-gray-100">
	<!-- Left Sidebar: Categories & Menu -->
	<div class="flex min-w-0 flex-1 flex-col">
		<!-- Header -->
		<div
			class="flex items-center justify-between gap-4 border-b border-default bg-neutral-primary p-4"
		>
			<!-- LEFT: User Avatar Dropdown -->
			<div class="flex items-center gap-3">
				<!-- User Dropdown Trigger -->
				<button
					id="userDropdownButton"
					type="button"
					onclick={toggleUserDropdown}
					class="relative h-10 w-10 overflow-hidden rounded-full cursor-pointer"
				>
					{#if $auth.user?.avatarUrl}
						<img src={$auth.user.avatarUrl} alt="Avatar" class="h-full w-full object-cover" />
					{:else}
						<div class="flex h-full w-full items-center justify-center bg-neutral-secondary-medium">
							<span class="font-medium text-body">
								{($auth.user?.fullName || $auth.user?.username || 'U')[0].toUpperCase()}
							</span>
						</div>
					{/if}
				</button>

				<!-- User Dropdown Menu -->
				<div
					id="userDropdown"
					class="z-10 hidden bg-neutral-primary-medium border border-default-medium rounded-base shadow-lg w-72"
				>
					<!-- User Header -->
					<div class="px-4 py-3 border-b border-default-medium">
						<div class="font-medium text-heading">
							{$auth.user?.fullName || $auth.user?.username}
						</div>
						<div class="truncate text-sm text-body">{$auth.user?.roleName}</div>
					</div>

					<!-- Navigation Links -->
					<div class="p-2 border-b border-default-medium">
						<p class="px-2 py-1 text-xs font-semibold text-body-subtle uppercase tracking-wider">
							Navigation
						</p>
						<ul class="mt-1 space-y-1">
							<li>
								<a
									href="/pos"
									class="flex items-center gap-2 rounded-md px-2 py-2 text-sm font-medium text-heading hover:bg-neutral-tertiary-medium hover:text-heading"
								>
									<Icon name="tabler:news" class="h-4 w-4" />
									POS (Cashier)
								</a>
							</li>
							<li>
								<a
									href="/kitchen"
									class="flex items-center gap-2 rounded-md px-2 py-2 text-sm font-medium text-heading hover:bg-neutral-tertiary-medium hover:text-heading"
								>
									<Icon name="tabler:clock" class="h-4 w-4" />
									Kitchen
								</a>
							</li>
						</ul>
					</div>

					<!-- Sign out -->
					<div class="p-2">
						<button
							type="button"
							onclick={handleLogout}
							class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm font-medium text-fg-danger hover:bg-danger-soft"
						>
							<Icon name="tabler:door-open" class="h-4 w-4" />
							Sign out
						</button>
					</div>
				</div>

				<!-- User Name (visible on larger screens) -->
				<div class="hidden lg:block">
					<div class="text-sm font-medium text-heading">
						{$auth.user?.fullName || $auth.user?.username}
					</div>
					<div class="text-xs text-body">{$auth.user?.roleName}</div>
				</div>
			</div>

			<!-- RIGHT: Search + Actions -->
			<div class="flex flex-1 items-center justify-end gap-3">
				<!-- Search -->
				<div class="relative max-w-md flex-1">
					<span class="absolute inset-y-0 left-0 flex items-center pl-3">
						<Icon name="tabler:search" class="h-5 w-5 text-body" />
					</span>
					<input
						type="text"
						bind:value={posStore.searchQuery}
						placeholder="Search menu items..."
						class="w-full rounded-base border border-default-medium bg-neutral-secondary-medium py-2 pl-10 pr-4 text-sm text-heading shadow-xs placeholder:text-body focus:border-brand focus:ring-brand"
					/>
				</div>

				<!-- Orders Button -->
				<button
					onclick={() => posStore.openOrdersModal()}
					class="flex items-center gap-2 rounded-base border border-default-medium bg-neutral-secondary-medium px-4 py-2 text-sm font-medium text-heading shadow-xs hover:bg-neutral-tertiary-medium"
				>
					<Icon name="tabler:file-text" class="h-5 w-5" />
					<span class="hidden lg:inline">Orders</span>
				</button>

				<!-- Logout Button -->
				<button
					onclick={() => posStore.handleLogout()}
					class="flex items-center gap-2 rounded-base border border-danger-subtle bg-neutral-secondary-medium px-4 py-2 text-sm font-medium text-fg-danger shadow-xs hover:bg-danger-soft"
					title="Sign out"
				>
					<Icon name="tabler:logout" class="h-5 w-5" />
					<span class="hidden lg:inline">Sign out</span>
				</button>
			</div>
		</div>

		<!-- Categories -->
		<div class="flex gap-2 overflow-x-auto border-b bg-white p-2 whitespace-nowrap">
			<button
				onclick={() => (posStore.selectedCategoryId = null)}
				class="rounded-full px-4 py-2 text-sm font-medium transition-colors
                    {posStore.selectedCategoryId === null
					? 'bg-indigo-600 text-white'
					: 'bg-gray-100 text-gray-700 hover:bg-gray-200'}"
			>
				All
			</button>
			{#each $categories as category (category.id)}
				<button
					onclick={() => (posStore.selectedCategoryId = category.id)}
					class="rounded-full px-4 py-2 text-sm font-medium transition-colors
                        {posStore.selectedCategoryId === category.id
						? 'bg-indigo-600 text-white'
						: 'bg-gray-100 text-gray-700 hover:bg-gray-200'}"
				>
					{category.name}
				</button>
			{/each}
		</div>

		<!-- Menu Grid -->
		<div class="flex-1 overflow-y-auto p-4">
			{#if posStore.loading}
				<div class="flex h-64 items-center justify-center">
					<div class="h-12 w-12 animate-spin rounded-full border-b-2 border-indigo-600"></div>
				</div>
			{:else}
				<!-- Ultra-compact Grid: Max efficiency -->
				<div
					class="relative z-0 grid grid-cols-2 gap-2 pb-20 md:grid-cols-4 md:gap-3 md:pb-0 lg:grid-cols-5 xl:grid-cols-7"
				>
					{#each posStore.filteredMenuItems as item (item.id)}
						<button
							type="button"
							onclick={() => posStore.openAddonModal(item)}
							class="group relative flex h-full cursor-pointer flex-col overflow-hidden rounded-lg border border-gray-200 bg-white text-left shadow-sm transition-all duration-200 hover:border-indigo-400 hover:shadow-md active:scale-95"
						>
							{#if item.imageUrl}
								<div class="relative aspect-[4/3] w-full overflow-hidden bg-gray-50">
									<img
										src={item.imageUrl}
										alt={item.name}
										class="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
									/>
								</div>
							{:else}
								<div
									class="relative flex aspect-[4/3] w-full items-center justify-center bg-gradient-to-br from-indigo-50 to-purple-50"
								>
									<span class="text-2xl opacity-40">🍽️</span>
								</div>
							{/if}
							<div class="flex flex-1 flex-col p-2">
								<h3
									class="mb-0.5 line-clamp-2 text-xs leading-tight font-bold text-gray-900 group-hover:text-indigo-600"
								>
									{item.name}
								</h3>
								<div class="mt-auto flex items-center justify-between gap-1 pt-1">
									<div class="text-xs font-black text-indigo-600">
										{formatCurrency(item.price)}
									</div>
									<div
										class="rounded bg-indigo-50 px-1 py-0.5 text-[10px] font-bold text-indigo-600 opacity-0 transition-opacity group-hover:opacity-100"
									>
										SELECT
									</div>
								</div>
							</div>
						</button>
					{/each}
				</div>

				<!-- Combo Section -->
				{#if $combos && $combos.filter((c) => c.isActive).length > 0}
					<div class="mt-6">
						<h3 class="mb-3 flex items-center gap-2 text-lg font-bold text-gray-800">
							<span class="text-2xl">🎁</span>
							Promotional Combos
						</h3>
						<div
							class="relative z-0 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5"
						>
							{#each $combos.filter((c) => c.isActive) as combo (combo.id)}
								<button
									type="button"
									onclick={() => posStore.handleComboClick(combo)}
									class="cursor-pointer rounded-xl border-2 border-amber-400 bg-gradient-to-br from-amber-50 to-orange-50 p-4 text-left transition-all duration-200 hover:scale-105 hover:from-amber-100 hover:to-orange-100 hover:shadow-lg active:scale-95"
								>
									{#if combo.imageUrl}
										<img
											src={combo.imageUrl}
											alt={combo.name}
											class="mb-2 aspect-square w-full rounded-lg border border-amber-200 object-cover shadow-sm"
										/>
									{/if}
									<div class="pointer-events-none mb-2 line-clamp-2 font-semibold text-gray-900">
										{combo.name}
									</div>
									{#if combo.description}
										<div class="pointer-events-none mb-2 line-clamp-1 text-xs text-gray-600">
											{combo.description}
										</div>
									{/if}
									<div class="pointer-events-none text-lg font-bold text-orange-600">
										{formatCurrency(combo.comboPrice)}
									</div>
									{#if combo.items && combo.items.length > 0}
										<div class="pointer-events-none mt-1 text-xs text-gray-500">
											{combo.items.length} items
										</div>
									{/if}
								</button>
							{/each}
						</div>
					</div>
				{/if}
			{/if}
		</div>
	</div>

	<!-- Right Sidebar: Cart - Responsive (Hidden on mobile, shown as drawer/modal or full width) -->
	<!-- Mobile Cart Toggle Button (Floating) -->
	<!-- Mobile Cart Toggle Button (Floating) -->
	{#if !posStore.isCartOpen}
		<div class="fixed right-4 bottom-4 z-50 md:hidden">
			<button
				onclick={() => (posStore.isCartOpen = true)}
				class="animate-bounce-subtle relative flex items-center gap-2 rounded-full bg-indigo-600 p-4 text-white shadow-lg"
				aria-label="Open cart"
			>
				<Icon name="tabler:shopping-cart" class="h-6 w-6" />
				<span class="font-bold">{$cart.items.reduce((sum, item) => sum + item.quantity, 0)}</span>
			</button>
		</div>
	{/if}

	<div
		id="cart-sidebar"
		class="fixed inset-y-0 right-0 z-40 flex w-full flex-shrink-0 transform flex-col border-l bg-white shadow-2xl transition-transform duration-300 sm:w-80 md:relative md:w-96
        {posStore.isCartOpen ? 'translate-x-0' : 'translate-x-full md:translate-x-0'}"
	>
		<!-- Header -->
		<div class="flex flex-shrink-0 flex-col gap-3 border-b bg-gray-50 p-3 md:p-4">
			<div class="flex items-center justify-between">
				<h2 class="flex items-center gap-2 text-lg font-bold text-gray-800">
					<Icon name="tabler:shopping-cart" class="h-5 w-5" />
					Cart ({$cart.items.reduce((sum, item) => sum + item.quantity, 0)})
				</h2>
				<div class="flex items-center gap-3">
					<button
						onclick={() => posStore.clearCartAndEditState()}
						class="text-sm font-medium text-red-500 hover:text-red-700"
						disabled={$cart.items.length === 0}
					>
						Clear
					</button>
					<!-- Close Button (Mobile Only) -->
					<button
						onclick={() => (posStore.isCartOpen = false)}
						class="rounded-full bg-gray-100 p-1 text-gray-500 hover:text-gray-700 md:hidden"
						aria-label="Close cart"
					>
						<Icon name="tabler:x" class="h-5 w-5" />
					</button>
				</div>
			</div>

			{#if posStore.editingOrder}
				<div
					class="flex items-center justify-between border-b border-amber-200 bg-amber-100 px-3 py-2 text-sm font-bold text-amber-800"
				>
					<span>Editing: #{posStore.editingOrder.code}</span>
					<button
						onclick={() => posStore.clearCartAndEditState()}
						class="text-xs text-amber-900 underline">Cancel edit</button
					>
				</div>
			{/if}

			<!-- Compact Source Selection (Sidebar) -->
			<button
				onclick={() => (posStore.showSourceModal = true)}
				class="flex w-full items-center justify-between rounded-lg border p-2 transition-all
                    {$cart.selectedSource
					? 'border-indigo-600 bg-indigo-50 text-indigo-700'
					: 'border-gray-300 bg-white text-gray-600'}"
			>
				<div class="flex items-center gap-2">
					<span class="text-xl">{$cart.selectedSource ? '📍' : '❓'}</span>
					<div class="text-left leading-tight">
						<div class="text-xs opacity-75">Order Source</div>
						<div class="text-sm font-bold">
							{$cart.selectedSource ? $cart.selectedSource.name : 'Not selected'}
						</div>
					</div>
				</div>
				<span class="rounded border border-black/5 bg-white/50 px-2 py-1 text-xs font-medium"
					>Change</span
				>
			</button>

			<!-- Customer Selection (Sidebar) -->
			<button
				onclick={() => (posStore.showCustomerModal = true)}
				class="flex w-full items-center justify-between rounded-lg border p-2 transition-all
                    {posStore.selectedCustomer
					? 'border-green-600 bg-green-50 text-green-700'
					: 'border-gray-300 bg-white text-gray-600'}"
			>
				<div class="flex items-center gap-2">
					<span class="text-xl">{posStore.selectedCustomer ? '👤' : '👥'}</span>
					<div class="text-left leading-tight">
						<div class="text-xs opacity-75">Customer</div>
						<div class="text-sm font-bold">
							{posStore.selectedCustomer ? posStore.selectedCustomer.fullName : 'Walk-in'}
						</div>
						{#if posStore.selectedCustomer}
							<div class="text-[10px] font-bold text-green-600">
								Points: {posStore.selectedCustomer.points}
							</div>
						{/if}
					</div>
				</div>
				{#if posStore.selectedCustomer}
					<div
						role="button"
						tabindex="0"
						onclick={(e) => {
							e.stopPropagation();
							posStore.removeCustomer();
						}}
						onkeydown={(e) => e.key === 'Enter' && posStore.removeCustomer()}
						class="cursor-pointer rounded border border-black/5 bg-white/50 px-2 py-1 text-xs font-medium hover:bg-red-50 hover:text-red-600"
					>
						Remove
					</div>
				{:else}
					<span class="rounded border border-black/5 bg-white/50 px-2 py-1 text-xs font-medium"
						>Select</span
					>
				{/if}
			</button>
		</div>

		<div class="min-h-0 flex-1 space-y-3 overflow-y-auto p-3 md:p-4">
			{#if $cart.items.length === 0}
				<div class="py-12 text-center text-gray-500">
					<p>No items in cart</p>
				</div>
			{:else}
				{#each $cart.items as item, index (index)}
					<div
						class="flex gap-3 rounded-xl border-2 border-gray-100 bg-white p-3 shadow-sm transition-all hover:border-indigo-100 hover:shadow-md"
					>
						<div class="min-w-0 flex-1">
							<div class="flex items-start justify-between gap-2">
								<h4 class="line-clamp-2 text-sm font-bold text-gray-900">
									{item.menuItem?.name || item.combo?.name}
									{#if item.combo}
										<span
											class="ml-1 rounded-full bg-orange-100 px-2 py-0.5 text-[10px] font-bold whitespace-nowrap text-orange-600"
											>COMBO</span
										>
									{/if}
								</h4>
								<button
									onclick={() => cartActions.removeItem(index)}
									class="flex-shrink-0 touch-manipulation rounded-lg p-1.5 text-gray-400 transition-all hover:bg-red-50 hover:text-red-600 active:scale-90"
									aria-label="Remove item"
								>
									<Icon name="tabler:x" class="h-4 w-4" />
								</button>
							</div>

							{#if item.selectedAddons.length > 0}
								<div class="mt-1 line-clamp-1 text-xs text-gray-500">
									+ {item.selectedAddons
										.map((a: { addon: Addon; quantity: number }) => a.addon.name)
										.join(', ')}
								</div>
							{/if}

							{#if item.note}
								<div class="mt-1 line-clamp-1 text-xs text-gray-500 italic">📝 {item.note}</div>
							{/if}

							<div class="mt-3 flex items-center justify-between">
								<!-- Inline Quantity Controls -->
								<div class="flex items-center gap-2">
									<button
										onclick={() => {
											if (item.quantity > 1) {
												cartActions.updateQuantity(index, item.quantity - 1);
											}
										}}
										class="flex h-7 w-7 items-center justify-center rounded-lg bg-gray-100 text-gray-700 transition-all hover:bg-gray-200 active:scale-90"
										disabled={item.quantity <= 1}
										aria-label="Decrease quantity"
									>
										<Icon name="tabler:minus" class="h-4 w-4" />
									</button>
									<span class="min-w-[2rem] text-center text-base font-bold text-gray-900"
										>{item.quantity}</span
									>
									<button
										onclick={() => cartActions.updateQuantity(index, item.quantity + 1)}
										class="flex h-7 w-7 items-center justify-center rounded-lg bg-indigo-600 text-white transition-all hover:bg-indigo-700 active:scale-90"
										aria-label="Increase quantity"
									>
										<Icon name="tabler:plus" class="h-4 w-4" />
									</button>
								</div>
								<!-- Item Total Price -->
								<div class="text-base font-black text-indigo-600">
									{formatCurrency(
										((item.menuItem?.price || item.combo?.comboPrice || 0) +
											(item.selectedAddons?.reduce((sum, a) => sum + (a.addon?.price || 0), 0) ||
												0)) *
											(item.quantity || 1)
									)}
								</div>
							</div>
						</div>
					</div>
				{/each}
			{/if}
		</div>

		<div class="flex-shrink-0 space-y-2 border-t bg-gray-50 p-3 pb-4 md:space-y-3 md:p-4 md:pb-4">
			<!-- Discount Selector -->
			<div class="flex gap-2">
				<select
					class="flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-transparent focus:ring-2 focus:ring-indigo-500"
					value={$cart.discount?.id || ''}
					onchange={(e) => {
						const id = parseInt(e.currentTarget.value);
						const discount = $discounts.find((d) => d.id === id) || null;
						cartActions.setDiscount(discount);
					}}
				>
					<option value="">-- Discount code --</option>
					{#each $discounts as discount (discount.id)}
						{#if discount.isActive}
							<option value={discount.id}>
								{discount.code} ({discount.type === 'percent'
									? `-${discount.value}%`
									: `-${formatCurrency(discount.value)}`})
							</option>
						{/if}
					{/each}
				</select>
			</div>

			<div class="flex justify-between text-sm">
				<span class="text-gray-600">Subtotal:</span>
				<span class="font-medium">{formatCurrency($cartTotals.subtotal)}</span>
			</div>
			{#if $cartTotals.discountAmount > 0}
				<div class="flex justify-between text-sm text-green-600">
					<span>Discount:</span>
					<span>-{formatCurrency($cartTotals.discountAmount)}</span>
				</div>
			{/if}
			{#if $cartTotals.vatAmount > 0}
				<div class="flex justify-between text-sm text-gray-600">
					<span>VAT (10%):</span>
					<span>+{formatCurrency($cartTotals.vatAmount)}</span>
				</div>
			{/if}
			<div class="flex justify-between text-lg font-bold text-indigo-600 md:text-xl">
				<span>Total:</span>
				<span>{formatCurrency($cartTotals.total)}</span>
			</div>
			<Button
				variant="primary"
				fullWidth={true}
				onclick={() => posStore.handlePlaceOrder()}
				disabled={$cart.items.length === 0}
			>
				Pay Now
			</Button>
		</div>
	</div>
</div>

<!-- Source Selection Modal -->
<Modal
	bind:open={posStore.showSourceModal}
	title="Select Order Source"
	onClose={() => (posStore.showSourceModal = false)}
>
	<div class="grid grid-cols-2 gap-3 sm:grid-cols-3 md:gap-4">
		{#each $sources as source (source.id)}
			<button
				onclick={() => posStore.selectSource(source)}
				class="flex flex-col items-center justify-center gap-3 rounded-xl border-2 p-6 transition-all hover:shadow-md
                    {$cart.selectedSource?.id === source.id
					? 'border-indigo-600 bg-indigo-50 text-indigo-700'
					: 'border-gray-200 bg-white hover:border-indigo-300'}"
			>
				<div
					class="rounded-full bg-gray-100 p-3 text-gray-600 {$cart.selectedSource?.id === source.id
						? 'bg-indigo-100 text-indigo-600'
						: ''}"
				>
					<Icon name="tabler:building" class="h-8 w-8" />
				</div>
				<span class="text-base font-bold md:text-lg">{source.name}</span>
			</button>
		{/each}
	</div>
</Modal>

<!-- Addon Modal -->
<Modal
	bind:open={posStore.showAddonModal}
	title={posStore.selectedMenuItem?.name || 'Add Item'}
	onClose={() => (posStore.showAddonModal = false)}
>
	{#if posStore.selectedMenuItem}
		<div class="space-y-6">
			<!-- Image & Description -->
			{#if posStore.selectedMenuItem.imageUrl}
				<img
					src={posStore.selectedMenuItem.imageUrl}
					alt={posStore.selectedMenuItem.name}
					class="h-48 w-full rounded-xl object-cover"
				/>
			{/if}
			{#if posStore.selectedMenuItem.description}
				<p class="text-gray-600">{posStore.selectedMenuItem.description}</p>
			{/if}

			<!-- Addons -->
			{#if posStore.selectedMenuItem.addons && posStore.selectedMenuItem.addons.length > 0}
				<div>
					<h4 class="mb-3 font-semibold text-gray-900">Add-ons:</h4>
					<div class="space-y-2">
						{#each posStore.selectedMenuItem.addons as addon (addon.id)}
							<label
								class="flex cursor-pointer items-center justify-between rounded-lg border p-3 transition-colors hover:bg-gray-50
                                {posStore.selectedAddons.some((a) => a.addon.id === addon.id)
									? 'border-indigo-600 bg-indigo-50'
									: 'border-gray-200'}"
							>
								<div class="flex items-center gap-3">
									<input
										type="checkbox"
										checked={posStore.selectedAddons.some((a) => a.addon.id === addon.id)}
										onchange={() => posStore.toggleAddon(addon)}
										class="h-5 w-5 rounded text-indigo-600 focus:ring-indigo-500"
									/>
									<span class="font-medium">{addon.name}</span>
								</div>
								<span class="font-semibold text-indigo-600">+{formatCurrency(addon.price)}</span>
							</label>
						{/each}
					</div>
				</div>
			{/if}

			<!-- Note -->
			<div>
				<label for="note" class="mb-1 block text-sm font-medium text-gray-700">Note:</label>
				<textarea
					id="note"
					bind:value={posStore.itemNote}
					rows="2"
					class="w-full rounded-lg border border-gray-300 px-3 py-2 focus:border-transparent focus:ring-2 focus:ring-indigo-500"
					placeholder="e.g., Less ice, more milk..."></textarea>
			</div>

			<!-- Quantity & Total -->
			<div class="flex items-center justify-between border-t pt-4">
				<div class="flex items-center gap-3">
					<button
						onclick={() => (posStore.itemQuantity = Math.max(1, posStore.itemQuantity - 1))}
						class="h-10 w-10 rounded-lg bg-gray-100 font-semibold hover:bg-gray-200"
						aria-label="Decrease quantity"
					>
						-
					</button>
					<input
						id="quantity-input"
						type="number"
						bind:value={posStore.itemQuantity}
						min="1"
						class="w-20 rounded-lg border border-gray-300 px-4 py-2 text-center"
					/>
					<button
						onclick={() => posStore.itemQuantity++}
						class="h-10 w-10 rounded-lg bg-gray-100 font-semibold hover:bg-gray-200"
						aria-label="Increase quantity"
					>
						+
					</button>
				</div>
				<div class="text-xl font-bold text-indigo-600">
					{formatCurrency(
						(posStore.selectedMenuItem.price +
							posStore.selectedAddons.reduce((sum, a) => sum + a.addon.price, 0)) *
							posStore.itemQuantity
					)}
				</div>
			</div>

			<Button variant="primary" fullWidth={true} onclick={() => posStore.addToCart()}>
				Add to Cart - {formatCurrency(
					(posStore.selectedMenuItem.price +
						posStore.selectedAddons.reduce((sum, a) => sum + a.addon.price, 0)) *
						posStore.itemQuantity
				)}
			</Button>
		</div>
	{/if}
</Modal>

<!-- Pending Orders Modal -->
<Modal
	bind:open={posStore.showOrdersModal}
	title="Pending Orders"
	onClose={() => (posStore.showOrdersModal = false)}
>
	<div class="max-h-[60vh] space-y-4 overflow-y-auto pr-2">
		{#if posStore.loadingOrders}
			<div class="flex justify-center py-8">
				<div class="h-8 w-8 animate-spin rounded-full border-b-2 border-indigo-600"></div>
			</div>
		{:else if posStore.pendingOrders.length === 0}
			<div class="py-8 text-center text-gray-500">No pending orders.</div>
		{:else}
			{#each posStore.pendingOrders as order (order.id)}
				<div
					class="rounded-xl border border-gray-200 bg-white p-4 shadow-sm transition-shadow hover:shadow-md"
				>
					<div class="mb-3 flex items-start justify-between">
						<div>
							<div class="text-lg font-bold text-gray-900">{order.sourceName || 'Takeaway'}</div>
							<div class="text-sm text-gray-500">#{order.orderCode}</div>
						</div>
						<div class="text-right">
							<div class="text-lg font-bold text-indigo-600">
								{formatCurrency(order.totalAmount)}
							</div>
							<div class="text-xs text-gray-400">{formatTime(order.createdAt)}</div>
						</div>
					</div>

					<div class="my-3 border-t border-gray-100 pt-3">
						<div class="mb-2 text-sm text-gray-600">
							{order.items.length} items: {order.items.map((i) => i.menuItemName).join(', ')}
						</div>
					</div>

					<div class="grid grid-cols-3 gap-2">
						<button
							onclick={() => posStore.cancelOrder(order.id)}
							class="flex h-10 items-center justify-center rounded-lg border-2 border-red-50 bg-white text-xs font-bold text-red-500 transition-all hover:bg-red-50 active:scale-95"
						>
							CANCEL
						</button>
						<button
							onclick={() => posStore.adjustOrder(order)}
							class="flex h-10 items-center justify-center rounded-lg border-2 border-amber-50 bg-white text-xs font-bold text-amber-600 transition-all hover:bg-amber-50 active:scale-95"
						>
							EDIT
						</button>
						<button
							onclick={() => posStore.openPaymentModal(order)}
							class="flex h-10 items-center justify-center rounded-lg bg-indigo-600 text-xs font-bold text-white shadow-md shadow-indigo-100 transition-all hover:bg-indigo-700 active:scale-95"
						>
							PAY
						</button>
					</div>
				</div>
			{/each}
		{/if}
	</div>
</Modal>

<!-- Customer Modal -->
<Modal
	bind:open={posStore.showCustomerModal}
	title="Member Customer"
	onClose={() => (posStore.showCustomerModal = false)}
>
	<div class="space-y-6">
		<!-- Lookup Section -->
		<div class="flex gap-2">
			<input
				type="text"
				bind:value={posStore.customerPhone}
				placeholder="Enter customer phone number..."
				class="flex-1 rounded-lg border border-gray-300 px-4 py-2 focus:ring-2 focus:ring-indigo-500"
				onkeydown={(e) => e.key === 'Enter' && posStore.lookupCustomer(posStore.customerPhone)}
			/>
			<Button variant="primary" onclick={() => posStore.lookupCustomer(posStore.customerPhone)}>
				Search
			</Button>
		</div>

		{#if posStore.selectedCustomer}
			<div
				class="flex items-center justify-between rounded-xl border border-green-200 bg-green-50 p-4"
			>
				<div>
					<div class="text-lg font-bold text-green-800">{posStore.selectedCustomer.fullName}</div>
					<div class="text-green-600">{posStore.selectedCustomer.phone}</div>
					<div class="mt-1 text-sm text-green-700">
						Loyalty points: <span class="font-bold">{posStore.selectedCustomer.points}</span>
					</div>
				</div>
				<div class="text-4xl text-green-500">✓</div>
			</div>
			<div class="text-center">
				<Button
					variant="secondary"
					onclick={() => (posStore.showCustomerModal = false)}
					fullWidth={true}
				>
					Confirm
				</Button>
			</div>
		{/if}

		<div class="relative flex items-center py-2">
			<div class="flex-grow border-t border-gray-200"></div>
			<span class="mx-4 flex-shrink-0 text-sm text-gray-400">Or create new</span>
			<div class="flex-grow border-t border-gray-200"></div>
		</div>

		<!-- Create New Section -->
		<form
			class="space-y-4"
			onsubmit={(e) => {
				e.preventDefault();
				const formData = new FormData(e.currentTarget);
				posStore.createCustomer({
					fullName: formData.get('fullName') as string,
					phone: formData.get('phone') as string,
					email: formData.get('email') as string
				});
			}}
		>
			<div>
				<label for="customer-fullname" class="mb-1 block text-sm font-medium text-gray-700"
					>Customer name</label
				>
				<input
					id="customer-fullname"
					name="fullName"
					type="text"
					required
					class="w-full rounded-lg border border-gray-300 px-4 py-2 focus:ring-2 focus:ring-indigo-500"
					placeholder="e.g., John Smith"
				/>
			</div>
			<div>
				<label for="customer-phone" class="mb-1 block text-sm font-medium text-gray-700"
					>Phone number</label
				>
				<input
					id="customer-phone"
					name="tabler:phone"
					type="tel"
					required
					bind:value={posStore.customerPhone}
					class="w-full rounded-lg border border-gray-300 px-4 py-2 focus:ring-2 focus:ring-indigo-500"
					placeholder="e.g., 0912345678"
				/>
			</div>
			<div>
				<label for="customer-email" class="mb-1 block text-sm font-medium text-gray-700"
					>Email (Optional)</label
				>
				<input
					id="customer-email"
					name="tabler:mail"
					type="email"
					class="w-full rounded-lg border border-gray-300 px-4 py-2 focus:ring-2 focus:ring-indigo-500"
					placeholder="e.g., email@example.com"
				/>
			</div>
			<Button type="submit" variant="secondary" fullWidth={true}>Create & Select New Customer</Button>
		</form>
	</div>
</Modal>

<Toast bind:show={posStore.showToast} message={posStore.toastMessage} type={posStore.toastType} />

<PaymentModal
	bind:open={posStore.showPaymentModal}
	order={posStore.selectedOrder}
	onPaymentComplete={() => posStore.handlePaymentComplete()}
/>
