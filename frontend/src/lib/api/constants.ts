/**
 * API configuration constants
 */
export const API_BASE_URL =
	process.env.NEXT_PUBLIC_API_URL ||
	(process.env.NODE_ENV === 'production'
		? typeof window !== 'undefined'
			? window.location.origin
			: ''
		: 'http://localhost:5050');
