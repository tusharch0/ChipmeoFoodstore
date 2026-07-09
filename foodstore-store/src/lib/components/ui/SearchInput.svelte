<script lang="ts">
	import Icon from './Icon.svelte';

	interface Props {
		value?: string;
		placeholder?: string;
		oninput?: (e: Event) => void;
		onchange?: (e: Event) => void;
		class?: string;
	}

	let {
		value = $bindable(''),
		placeholder = 'Search...',
		oninput,
		onchange,
		class: className = ''
	}: Props = $props();
</script>

<div class="relative {className}">
	<div class="absolute inset-y-0 start-0 flex items-center ps-3 pointer-events-none">
		<Icon name="tabler:search" class="w-4 h-4 text-body" />
	</div>
	<input
		type="search"
		{placeholder}
		bind:value
		{oninput}
		{onchange}
		class="block w-full px-3 py-2.5 ps-9 bg-neutral-secondary-medium border border-default-medium text-heading text-sm rounded-base focus:ring-brand focus:border-brand shadow-xs placeholder:text-body"
	/>
</div>
