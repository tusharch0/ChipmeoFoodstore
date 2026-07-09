<script lang="ts">
	import Icon from './Icon.svelte';

	interface Props {
		preview?: string;
		onSelect: (file: File) => void;
		onClear?: () => void;
		label?: string;
		class?: string;
	}

	let { preview = '', onSelect, onClear, label = 'Image', class: className = '' }: Props = $props();

	let input: HTMLInputElement | undefined = $state();

	function handleSelect() {
		input?.click();
	}

	function handleChange() {
		const file = input?.files?.[0];
		if (file) onSelect(file);
	}
</script>

<div class="flex items-center gap-4 {className}">
	{#if preview}
		<img
			src={preview}
			alt="Preview"
			class="h-20 w-20 rounded-lg border border-default-medium object-cover"
		/>
	{:else}
		<div
			class="flex h-20 w-20 items-center justify-center rounded-lg border border-default-medium bg-neutral-secondary-medium text-body"
		>
			<Icon name="tabler:photo" class="w-8 h-8" />
		</div>
	{/if}
	<div class="flex flex-col gap-2">
		<button
			type="button"
			onclick={handleSelect}
			class="rounded-base bg-brand-softer border border-brand-subtle px-3 py-1.5 text-sm text-fg-brand-strong hover:bg-brand-soft font-medium"
		>
			Select {label.toLowerCase()}
		</button>
		{#if preview && onClear}
			<button type="button" onclick={onClear} class="text-sm text-fg-danger hover:underline">
				Remove {label.toLowerCase()}
			</button>
		{/if}
	</div>
	<input bind:this={input} type="file" accept="image/*" class="hidden" onchange={handleChange} />
</div>
