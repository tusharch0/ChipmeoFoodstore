<script lang="ts">
	interface Props {
		page: number;
		totalPages: number;
		total?: number;
		onChange: (page: number) => void;
	}

	let { page, totalPages, total = 0, onChange }: Props = $props();
</script>

<div class="flex flex-col items-center">
	{#if total > 0}
		<p class="mb-3 text-sm text-body">
			Showing <span class="font-medium text-heading">{(page - 1) * 10 + 1}</span> -
			<span class="font-medium text-heading">{Math.min(page * 10, total)}</span>
			/ <span class="font-medium text-heading">{total}</span>
		</p>
	{/if}
	<nav aria-label="Page navigation example">
		<ul class="flex -space-x-px text-sm">
			<li>
				<button
					onclick={() => onChange(1)}
					disabled={page <= 1}
					class="flex items-center justify-center text-body bg-neutral-secondary-medium box-border border border-default-medium hover:bg-neutral-tertiary-medium hover:text-heading font-medium rounded-s-base text-sm px-3 h-9 focus:outline-none disabled:opacity-50 disabled:cursor-not-allowed"
				>
					First
				</button>
			</li>
			<li>
				<button
					onclick={() => onChange(page - 1)}
					disabled={page <= 1}
					class="flex items-center justify-center text-body bg-neutral-secondary-medium box-border border border-default-medium hover:bg-neutral-tertiary-medium hover:text-heading font-medium text-sm px-3 h-9 focus:outline-none disabled:opacity-50 disabled:cursor-not-allowed"
				>
					Prev
				</button>
			</li>
			{#each Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
				const start = Math.max(1, Math.min(page - 2, totalPages - 4));
				return start + i;
			}) as p}
				<li>
					<button
						onclick={() => onChange(p)}
						aria-current={p === page ? 'page' : undefined}
						class="flex items-center justify-center box-border border font-medium text-sm w-9 h-9 focus:outline-none {p ===
						page
							? 'text-fg-brand bg-neutral-tertiary-medium border-default-medium hover:text-fg-brand'
							: 'text-body bg-neutral-secondary-medium border-default-medium hover:bg-neutral-tertiary-medium hover:text-heading'}"
					>
						{p}
					</button>
				</li>
			{/each}
			<li>
				<button
					onclick={() => onChange(page + 1)}
					disabled={page >= totalPages}
					class="flex items-center justify-center text-body bg-neutral-secondary-medium box-border border border-default-medium hover:bg-neutral-tertiary-medium hover:text-heading font-medium text-sm px-3 h-9 focus:outline-none disabled:opacity-50 disabled:cursor-not-allowed"
				>
					Next
				</button>
			</li>
			<li>
				<button
					onclick={() => onChange(totalPages)}
					disabled={page >= totalPages}
					class="flex items-center justify-center text-body bg-neutral-secondary-medium box-border border border-default-medium hover:bg-neutral-tertiary-medium hover:text-heading font-medium rounded-e-base text-sm px-3 h-9 focus:outline-none disabled:opacity-50 disabled:cursor-not-allowed"
				>
					Last
				</button>
			</li>
		</ul>
	</nav>
</div>
