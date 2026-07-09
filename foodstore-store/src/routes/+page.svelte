<script lang="ts">
	import { auth } from '$lib/utils/index.js';
	import { goto } from '$app/navigation';
	import Icon from '$lib/components/ui/Icon.svelte';

	let username = $state('');
	let password = $state('');
	let loading = $state(false);
	let errorMessage = $state('');
	let showPassword = $state(false);

	function resolveDefaultRoute(defaultRoute?: string): string {
		if (!defaultRoute || defaultRoute === '/admin') return '/pos';
		return defaultRoute;
	}

	async function handleSubmit(e: Event) {
		e.preventDefault();
		errorMessage = '';

		if (!username || !password) {
			errorMessage = 'Please fill in all fields.';
			return;
		}

		loading = true;
		try {
			const result = await auth.loginAPI(username, password);
			if (result.success) {
				if (result.user?.roleName === 'customer') {
					errorMessage = 'Account does not have access permission.';
					loading = false;
					return;
				}
				await goto(resolveDefaultRoute(result.user?.defaultRoute));
			} else {
				errorMessage = result.error || 'Login failed.';
			}
		} catch (err) {
			console.error(err);
			errorMessage = 'An error occurred.';
		} finally {
			loading = false;
		}
	}

	function handleKeydown(event: KeyboardEvent) {
		if (event.key === 'Enter') {
			handleSubmit(event);
		}
	}
</script>

<svelte:head>
	<title>Login - Foodstore</title>
</svelte:head>

<div class="flex min-h-screen items-center justify-center bg-neutral-secondary-soft p-4">
	<div class="w-full max-w-md">
		<div class="border border-default bg-neutral-primary-soft rounded-base shadow-sm p-6 md:p-8">
			<div class="mb-6 flex flex-col items-center">
				<img src="/cmfs_removed_bg.png" class="mb-4 h-16 w-16 rounded-full" alt="Foodstore Logo" />
				<h1 class="mb-1 text-2xl font-bold text-heading">Foodstore</h1>
				<p class="text-body">Sign in to continue</p>
			</div>

			<form onsubmit={handleSubmit}>
				{#if errorMessage}
					<div
						class="mb-4 flex items-center gap-2 rounded-base bg-danger-soft p-4 text-sm text-fg-danger"
						role="alert"
					>
						<Icon name="tabler:alert-circle" class="h-5 w-5 flex-shrink-0" aria-hidden="true" />
						<span>{errorMessage}</span>
					</div>
				{/if}

				<div class="mb-4">
					<label for="auth-username" class="mb-2.5 block text-sm font-medium text-heading">
						Username
					</label>
					<input
						type="text"
						id="auth-username"
						bind:value={username}
						onkeydown={handleKeydown}
						class="block w-full rounded-base border border-default-medium bg-neutral-secondary-medium px-3 py-2.5 text-sm text-heading shadow-xs placeholder:text-body focus:border-brand focus:ring-brand"
						placeholder="Enter username"
						disabled={loading}
						required
					/>
				</div>

				<div class="mb-6">
					<label for="auth-password" class="mb-2.5 block text-sm font-medium text-heading">
						Password
					</label>
					<div class="relative">
						<input
							type={showPassword ? 'text' : 'password'}
							id="auth-password"
							bind:value={password}
							onkeydown={handleKeydown}
							class="block w-full rounded-base border border-default-medium bg-neutral-secondary-medium px-3 py-2.5 pr-10 text-sm text-heading shadow-xs placeholder:text-body focus:border-brand focus:ring-brand"
							placeholder="Enter password"
							disabled={loading}
							required
						/>
						<button
							type="button"
							class="absolute end-2.5 top-1/2 -translate-y-1/2 rounded-base p-1 text-body hover:text-heading"
							onclick={() => (showPassword = !showPassword)}
							tabindex="-1"
						>
							{#if showPassword}
								<Icon name="tabler:eye-off" class="h-5 w-5" />
							{:else}
								<Icon name="tabler:eye" class="h-5 w-5" />
							{/if}
						</button>
					</div>
				</div>

				<button
					type="submit"
					disabled={loading}
					class="mb-3 w-full rounded-base bg-brand px-4 py-2.5 text-sm font-medium leading-5 text-white shadow-xs hover:bg-brand-strong focus:outline-none focus:ring-4 focus:ring-brand-medium box-border border border-transparent disabled:cursor-not-allowed disabled:opacity-50"
				>
					{loading ? 'Processing...' : 'Sign in'}
				</button>
			</form>
		</div>
	</div>
</div>
