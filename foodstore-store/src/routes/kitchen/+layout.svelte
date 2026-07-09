<script lang="ts">
	import { kitchenState } from './kitchen.svelte.js';
	let { children } = $props();
</script>

<div class="flex h-screen flex-col overflow-hidden bg-gray-100 font-sans text-gray-800">
	<header
		class="flex h-[70px] items-center justify-between border-b border-gray-200 bg-white px-6 shadow-sm"
	>
		<h1 class="m-0 text-2xl font-bold text-gray-900">👨‍🍳 Kitchen Display</h1>
		<div
			class="rounded-full px-4 py-1.5 text-sm font-semibold transition-colors duration-200"
			class:bg-green-100={kitchenState.isConnected}
			class:text-green-700={kitchenState.isConnected}
			class:border={kitchenState.isConnected}
			class:border-green-200={kitchenState.isConnected}
			class:bg-gray-100={!kitchenState.isConnected}
			class:text-gray-500={!kitchenState.isConnected}
		>
			{kitchenState.isConnected ? '🟢 Connected' : '🔴 Disconnected'}
		</div>
	</header>

	{@render children()}
</div>
