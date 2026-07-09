export interface Source {
	id: string;
	name: string;
	isActive: boolean;
	createdAt?: string;
	updatedAt?: string;
}

export interface SourceCreateDto {
	name: string;
	isActive: boolean;
}

export interface SourceUpdateDto {
	name: string;
	isActive: boolean;
}
