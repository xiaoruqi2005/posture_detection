/* import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [vue()],
    server: {
        port: 5173, // 前端开发服务器端口 (可自定义)
        proxy: {
            // 配置代理以解决开发时的跨域问题
            // 所有以 /api 开头的请求都会被代理到后端服务器
            '/api': {
                target: 'http://localhost:5098', // 你的后端 ASP.NET Core 服务器地址和端口
                changeOrigin: true, // 需要虚拟主机站点
                // secure: false, // 如果后端是 https 且证书无效，可能需要
                // rewrite: (path) => path.replace(/^\/api/, '') // 如果后端 API 路径不包含 /api 前缀
            },
            // SignalR 的代理配置 (路径通常是 Hub 的映射路径)
            '/postureHub': {
                target: 'http://localhost:5098', // 你的后端 ASP.NET Core 服务器地址和端口
                changeOrigin: true,
                ws: true // 重要：为 WebSocket 开启代理
            }
        }
    }
}) */

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path' // 导入 path 模块

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [vue()],
    resolve: { // 添加 resolve 配置
        alias: {
            '@': path.resolve(__dirname, './src'), // 配置 @ 别名指向 src 目录
            // 你也可以在这里定义其他别名，例如：
            // 'components': path.resolve(__dirname, './src/components'),
            // 'views': path.resolve(__dirname, './src/views'),
        },
    },
    server: {
        port: 5173, // 前端开发服务器端口 (可自定义)
        proxy: {
            // 配置代理以解决开发时的跨域问题
            // 所有以 /api 开头的请求都会被代理到后端服务器
            '/api': {
                target: 'http://localhost:5098', // 你的后端 ASP.NET Core 服务器地址和端口
                changeOrigin: true, // 需要虚拟主机站点
                // secure: false, // 如果后端是 https 且证书无效，可能需要
                // rewrite: (path) => path.replace(/^\/api/, '') // 如果后端 API 路径不包含 /api 前缀
            },
            // SignalR 的代理配置 (路径通常是 Hub 的映射路径)
            '/postureHub': {
                target: 'http://localhost:5098', // 你的后端 ASP.NET Core 服务器地址和端口
                changeOrigin: true,
                ws: true // 重要：为 WebSocket 开启代理
            }
        }
    }
})