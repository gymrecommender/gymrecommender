import {defineConfig} from 'vite';
import react from '@vitejs/plugin-react';
import dotenv from 'dotenv';


dotenv.config();

export default defineConfig({
	plugins: [react()],
	server: {
		proxy: {
			'/api': {
				target: process.env.VITE_BACKEND_URL,
				secure: false,
				ws: true,
				changeOrigin: true
			},
		},
	},
	optimizeDeps: {
		include: ['moment'], // Add moment to optimized dependencies
	},
});