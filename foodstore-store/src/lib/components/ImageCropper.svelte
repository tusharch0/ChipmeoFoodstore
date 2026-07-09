<script lang="ts">
	import Croppie from 'croppie';
	import 'croppie/croppie.css';
	import Button from './ui/Button.svelte';
	import Icon from './ui/Icon.svelte';

	let {
		imageSrc,
		aspectRatio = 1,
		onCrop,
		onCancel
	} = $props<{
		imageSrc: string;
		aspectRatio?: number;
		onCrop: (blob: Blob) => void;
		onCancel: () => void;
	}>();

	let croppieContainer: HTMLElement;
	let croppieInstance: Croppie;

	$effect(() => {
		if (croppieContainer && imageSrc) {
			croppieInstance = new Croppie(croppieContainer, {
				viewport: { width: 300, height: 300, type: 'square' }, // Fixed square 1:1
				boundary: { width: 400, height: 400 },
				showZoomer: true,
				enableOrientation: true
			});

			croppieInstance.bind({
				url: imageSrc,
				zoom: 0
			});

			return () => {
				croppieInstance.destroy();
			};
		}
	});

	function handleCrop() {
		if (!croppieInstance) return;
		croppieInstance
			.result({
				type: 'blob',
				size: 'original', // Or stick to viewport size
				format: 'jpeg',
				quality: 0.9,
				circle: false
			})
			.then((blob: Blob) => {
				// For some reason croppie types might say result returns something else, but with type:'blob' it is a blob.
				// Casting if necessary or just passing it.
				if (blob) {
					onCrop(blob);
				}
			});
	}
</script>

<div class="fixed inset-0 z-50 flex items-center justify-center bg-black/80 p-4 backdrop-blur-sm">
	<div
		class="flex max-h-[90vh] w-full max-w-2xl flex-col overflow-hidden rounded-xl bg-white shadow-2xl"
	>
		<!-- Header -->
		<div class="flex items-center justify-between border-b bg-gray-50 p-4">
			<h3 class="font-bold text-gray-900">Crop Image</h3>
			<button onclick={onCancel} class="text-gray-500 hover:text-gray-700" aria-label="Close">
				<Icon name="tabler:x" class="h-6 w-6" />
			</button>
		</div>

		<!-- Cropper Area -->
		<div class="flex min-h-[400px] flex-1 items-center justify-center overflow-auto bg-gray-100">
			<!-- Croppie attaches here -->
			<div bind:this={croppieContainer}></div>
		</div>

		<!-- Footer / Controls -->
		<div class="flex justify-end gap-3 border-t bg-gray-50 p-4">
			<Button variant="secondary" onclick={onCancel}>Cancel</Button>
			<Button variant="primary" onclick={handleCrop}>Confirm & Crop</Button>
		</div>
	</div>
</div>

<style>
	/* 
     * Croppie might need some global overrides or specific container styling. 
     * The imported CSS handles most of it.
     */
</style>
