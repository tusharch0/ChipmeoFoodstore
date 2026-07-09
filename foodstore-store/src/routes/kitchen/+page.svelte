<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { fade, fly } from 'svelte/transition';
	import { kitchenState as vm } from './kitchen.svelte.js';

	onMount(() => {
		vm.init();
	});

	onDestroy(() => {
		vm.destroy();
	});
</script>

<svelte:head>
	<title>Kitchen Display - Foodstore</title>
</svelte:head>

<div class="flex h-full flex-1 flex-col overflow-hidden bg-gray-100">
	<!-- Mobile Tabs -->
	<div class="flex border-b border-gray-200 bg-white md:hidden">
		{#each vm.tabs as tab (tab.id)}
			<button
				class="flex flex-1 items-center justify-center gap-2 border-b-2 py-3 text-sm font-medium transition-colors
                    {vm.activeTab === tab.id
					? `border-${tab.color}-500 text-${tab.color}-600 bg-${tab.color}-50`
					: 'border-transparent text-gray-500 hover:text-gray-700'}"
				onclick={() => (vm.activeTab = tab.id)}
			>
				<span>{tab.icon}</span>
				{tab.label}
				<span class="ml-1 rounded-full bg-gray-100 px-1.5 py-0.5 text-xs">
					{vm.getOrderCount(tab.id)}
				</span>
			</button>
		{/each}
	</div>

	<div class="flex flex-1 gap-4 overflow-x-auto p-4 md:gap-6 md:p-6">
		<!-- PENDING COLUMN -->
		<div
			class="flex max-h-full min-w-[300px] flex-1 flex-col rounded-xl border-none bg-gray-200 md:min-w-[350px] {vm.activeTab ===
			'pending'
				? 'flex'
				: 'hidden md:flex'}"
		>
			<h2
				class="m-0 flex items-center gap-2.5 rounded-t-xl border-b border-gray-300 bg-white/50 px-5 py-4 text-lg font-bold text-gray-700"
			>
				<span class="text-xl">🔔</span>
				New Orders
				<span
					class="ml-auto rounded-xl bg-white px-2.5 py-0.5 text-sm font-semibold text-gray-600 shadow-sm"
					>{vm.pendingOrders.length}</span
				>
			</h2>
			<div
				class="flex flex-1 scrollbar-thin scrollbar-thumb-gray-300 scrollbar-track-transparent flex-col gap-4 overflow-y-auto p-4"
			>
				{#each vm.pendingOrders as order (order.id)}
					<div
						class="rounded-lg border border-l-4 border-gray-200 border-l-red-500 bg-white p-4 shadow-sm transition-transform duration-200 hover:-translate-y-0.5 hover:shadow-md"
						transition:fly={{ y: 20, duration: 300 }}
					>
						<div class="mb-3 flex justify-between border-b border-gray-100 pb-2 font-bold">
							<span class="text-xl font-extrabold text-gray-900"
								>#{order.orderCode.split('-').pop()}</span
							>
							<span class="text-sm font-medium text-gray-500">{vm.formatTime(order.createdAt)}</span
							>
						</div>
						<div class="flex flex-col gap-2">
							{#if order.kitchenRouting}
								<div class="w-fit rounded bg-slate-100 px-2 py-1 text-xs font-semibold uppercase text-slate-600">Station: {order.kitchenRouting}</div>
							{/if}
							{#each order.items as item, i (i)}
								<div
									class="flex flex-col border-b border-dashed border-gray-200 pb-2 last:border-b-0"
								>
									<div class="flex items-baseline">
										<span class="mr-1.5 text-lg font-extrabold text-red-500">{item.quantity}x</span>
										<span class="text-[1.05rem] font-semibold text-gray-800"
											>{item.menuItemName || item.comboName}</span
										>
									</div>
									{#if item.note}
										<div
											class="mt-0.5 inline-block w-fit rounded bg-red-50 px-1.5 py-0.5 text-sm text-red-600 italic"
										>
											📝 {item.note}
										</div>
									{/if}
									{#if item.addons && item.addons.length > 0}
										<div class="mt-0.5 ml-6 text-sm text-gray-500">
											{#each item.addons as addon, j (j)}
												<div>+ {addon.quantity} {addon.addonName}</div>
											{/each}
										</div>
									{/if}
								</div>
							{/each}
						</div>
						{#if order.note}
							<div
								class="mt-3 rounded-md border border-red-200 bg-red-50 p-2.5 text-sm font-medium text-rose-700"
							>
								⚠️ {order.note}
							</div>
						{/if}
						<div class="mt-4 flex gap-2.5">
							<button
								class="flex-1 cursor-pointer rounded-md border-none bg-blue-500 py-3 text-base font-semibold text-white shadow-sm transition-all duration-200 hover:bg-blue-600"
								onclick={() => vm.updateStatus(order.id, 'preparing')}
							>
								Start Cooking 🔥
							</button>
						</div>
					</div>
				{/each}
				{#if vm.pendingOrders.length === 0}
					<div class="py-10 text-center text-base text-gray-400 italic">No new orders</div>
				{/if}
			</div>
		</div>

		<!-- PREPARING COLUMN -->
		<div
			class="flex max-h-full min-w-[300px] flex-1 flex-col rounded-xl border-none bg-gray-200 md:min-w-[350px] {vm.activeTab ===
			'preparing'
				? 'flex'
				: 'hidden md:flex'}"
		>
			<h2
				class="m-0 flex items-center gap-2.5 rounded-t-xl border-b border-gray-300 bg-white/50 px-5 py-4 text-lg font-bold text-gray-700"
			>
				<span class="text-xl">🔥</span>
				Cooking
				<span
					class="ml-auto rounded-xl bg-white px-2.5 py-0.5 text-sm font-semibold text-gray-600 shadow-sm"
					>{vm.preparingOrders.length}</span
				>
			</h2>
			<div
				class="flex flex-1 scrollbar-thin scrollbar-thumb-gray-300 scrollbar-track-transparent flex-col gap-4 overflow-y-auto p-4"
			>
				{#each vm.preparingOrders as order (order.id)}
					<div
						class="rounded-lg border border-l-4 border-gray-200 border-l-amber-500 bg-white p-4 shadow-sm transition-transform duration-200 hover:-translate-y-0.5 hover:shadow-md"
						transition:fade
					>
						<div class="mb-3 flex justify-between border-b border-gray-100 pb-2 font-bold">
							<span class="text-xl font-extrabold text-gray-900"
								>#{order.orderCode.split('-').pop()}</span
							>
							<span class="rounded bg-amber-100 px-2 py-0.5 text-base font-bold text-amber-600"
								>⏳ {vm.getElapsedTime(order.updatedAt)}</span
							>
						</div>
						<div class="flex flex-col gap-2">
							{#each order.items as item, i (i)}
								<div
									class="flex flex-col border-b border-dashed border-gray-200 pb-2 last:border-b-0"
								>
									<div class="flex items-baseline">
										<span class="mr-1.5 text-lg font-extrabold text-red-500">{item.quantity}x</span>
										<span class="text-[1.05rem] font-semibold text-gray-800"
											>{item.menuItemName || item.comboName}</span
										>
									</div>
									{#if item.note}
										<div
											class="mt-0.5 inline-block w-fit rounded bg-red-50 px-1.5 py-0.5 text-sm text-red-600 italic"
										>
											📝 {item.note}
										</div>
									{/if}
									{#if item.addons && item.addons.length > 0}
										<div class="mt-0.5 ml-6 text-sm text-gray-500">
											{#each item.addons as addon, j (j)}
												<div>+ {addon.quantity} {addon.addonName}</div>
											{/each}
										</div>
									{/if}
								</div>
							{/each}
						</div>
						<div class="mt-4 flex gap-2.5">
							<button
								class="flex-1 cursor-pointer rounded-md border-none bg-emerald-500 py-3 text-base font-semibold text-white shadow-sm transition-all duration-200 hover:bg-emerald-600"
								onclick={() => vm.updateStatus(order.id, 'served')}
							>
								Mark Done ✅
							</button>
						</div>
					</div>
				{/each}
				{#if vm.preparingOrders.length === 0}
					<div class="py-10 text-center text-base text-gray-400 italic">No items being prepared</div>
				{/if}
			</div>
		</div>

		<!-- COMPLETED COLUMN -->
		<div
			class="flex max-h-full min-w-[300px] flex-1 flex-col rounded-xl border-none bg-gray-200 md:min-w-[350px] {vm.activeTab ===
			'completed'
				? 'flex'
				: 'hidden md:flex'}"
		>
			<h2
				class="m-0 flex items-center gap-2.5 rounded-t-xl border-b border-gray-300 bg-white/50 px-5 py-4 text-lg font-bold text-gray-700"
			>
				<span class="text-xl">✅</span>
				Done
				<span
					class="ml-auto rounded-xl bg-white px-2.5 py-0.5 text-sm font-semibold text-gray-600 shadow-sm"
					>{vm.completedOrders.length}</span
				>
			</h2>
			<div
				class="grid flex-1 scrollbar-thin scrollbar-thumb-gray-300 scrollbar-track-transparent grid-cols-[repeat(auto-fill,minmax(150px,1fr))] content-start gap-4 overflow-y-auto p-4"
			>
				{#each vm.completedOrders as order (order.id)}
					<div
						class="flex min-h-[100px] flex-col justify-center rounded-lg border border-t-4 border-gray-200 border-t-green-500 bg-white p-4 text-center shadow-sm"
						transition:fade
					>
						<div class="flex flex-col gap-1">
							<span class="text-3xl leading-none font-extrabold text-gray-900"
								>#{order.orderCode.split('-').pop()}</span
							>
							<div class="text-xs text-gray-400">{vm.formatTime(order.updatedAt)}</div>
						</div>
					</div>
				{/each}
				{#if vm.completedOrders.length === 0}
					<div class="col-span-full py-10 text-center text-base text-gray-400 italic">
						No completed orders yet
					</div>
				{/if}
			</div>
		</div>
	</div>
</div>
