import {defineConfig} from 'vite';
import react from '@vitejs/plugin-react';
import dotenv from 'dotenv';
dotenv.config();

export default defineConfig({
	plugins: [react()],
	proxy: {
		'/api': process.env.VITE_BACKEND_URL
	},
});