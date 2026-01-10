import '@testing-library/jest-dom';

import React from 'react';

// Next.js Image relies on Next internals; mock it for Jest.
jest.mock('next/image', () => ({
	__esModule: true,
	default: (props) => {
		const { src, alt, ...rest } = props;
		const {
			fill,
			priority,
			placeholder,
			blurDataURL,
			quality,
			loader,
			unoptimized,
			...imgProps
		} = rest;

		void fill;
		void priority;
		void placeholder;
		void blurDataURL;
		void quality;
		void loader;
		void unoptimized;

		const resolvedSrc = typeof src === 'string' ? src : src?.src;
		return React.createElement('img', { src: resolvedSrc, alt, ...imgProps });
	},
}));
