<script lang="ts">
	import Button from '$lib/components/ui/Button.svelte';
	import { goto } from '$app/navigation';
</script>

<div class="flex min-h-screen flex-col items-center justify-center bg-gray-50 p-4">
	<div class="max-w-md space-y-6 text-center">
		<div
			class="mx-auto flex h-24 w-24 items-center justify-center rounded-full bg-red-100 text-5xl text-red-600"
		>
			⚠️
		</div>

		<h1 class="text-3xl font-bold text-gray-900">An error occurred</h1>

		<p class="text-gray-600">
			Sorry, an error occurred during processing or you do not have permission to access this page.
		</p>

		<div class="pt-4">
			<Button variant="primary" onclick={async () => await goto('/')}>Go Home</Button>
		</div>
	</div>
</div>
