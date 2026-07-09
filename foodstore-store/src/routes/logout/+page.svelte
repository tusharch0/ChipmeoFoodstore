<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { auth } from '$lib/utils/index.js';

	onMount(async () => {
		try {
			// Use auth store to handle logout
			auth.logout();
		} catch (error) {
			console.error('Logout failed:', error);
		} finally {
			// Redirect to login page
			await goto('/', { replaceState: true });
		}
	});
</script>

<div class="flex min-h-screen items-center justify-center bg-gray-100">
	<div class="text-center">
		<div
			class="mx-auto mb-4 h-12 w-12 animate-spin rounded-full border-b-2 border-indigo-600"
		></div>
		<p class="text-gray-600">Signing out...</p>
	</div>
</div>
