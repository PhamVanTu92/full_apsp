import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import VitePluginVueDevTools from 'vite-plugin-vue-devtools';
import Components from 'unplugin-vue-components/vite';
import { PrimeVueResolver } from 'unplugin-vue-components/resolvers';

export default defineConfig(() => {
    return {
        plugins: [
            vue(),
            VitePluginVueDevTools(),
            Components({
                resolvers: [PrimeVueResolver()],
                dts: 'src/components.d.ts',
            }),
        ],
        resolve: {
            alias: {
                '@': fileURLToPath(new URL('./src', import.meta.url))
            }
        },
        css: {
            preprocessorOptions: {
                scss: {
                    // Tắt deprecation warning từ node_modules (primeflex dùng global built-ins cũ)
                    quietDeps: true
                }
            }
        },
        server: {
            host: '0.0.0.0',
            port: 1072,
            proxy: {
                '/api': {
                    target: 'http://localhost:5279',
                    changeOrigin: true,
                    secure: false,
                    ws: true,
                },
                '/notificationHubs': {
                    target: 'ws://localhost:5279',
                    ws: true,
                    changeOrigin: true,
                    secure: false,
                },
            },
        },
        test: {
            environment: 'jsdom',
            globals: true,
            exclude: [
                '**/node_modules/**',
                'src/apis/api.test.js',
                'src/apis/dummy_api.test.js'
            ],
            alias: {
                '@': fileURLToPath(new URL('./src', import.meta.url))
            },
            coverage: {
                provider: 'v8',
                reporter: ['text', 'html'],
                include: ['src/helpers/**', 'src/utils/**', 'src/Pinia/**', 'src/composables/**']
            }
        }
    };
});
