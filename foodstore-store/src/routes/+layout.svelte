<script lang="ts">
	import './layout.css';
	import { onMount } from 'svelte';
	import { auth } from '$lib/utils/index.js';
	import LandscapePrompt from '$lib/components/LandscapePrompt.svelte';
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';

	let { children } = $props();
	let checking = $state(true);

	onMount(() => {
		auth.checkAuth();
	});

	$effect(() => {
		(async () => {
			const path = $page.url.pathname;
			const isProtected = path.startsWith('/pos') || path.startsWith('/kitchen');

			if (!isProtected) {
				checking = false;
				return;
			}

			if ($auth.loading) return;

			if (!$auth.isAuthenticated) {
				checking = false;
				await goto('/');
				return;
			}

			if ($auth.user?.roleName === 'customer') {
				checking = false;
				await goto('/error');
				return;
			}

			checking = false;
		})().catch(console.error);
	});
</script>

<LandscapePrompt />

{#if !checking}
	{@render children()}
{:else}
	<div class="flex min-h-screen items-center justify-center bg-gray-50">
		<div class="flex flex-col items-center gap-4">
			<div
				class="h-12 w-12 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent"
			></div>
			<p class="font-medium text-gray-500">Checking authentication...</p>
		</div>
	</div>
{/if}
