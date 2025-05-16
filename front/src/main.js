import { createApp } from 'vue'
import './style.css' // 你的全局样式
import App from './App.vue'
import router from './router' // 导入路由配置

const app = createApp(App);

app.use(router); // 使用 Vue Router 插件

app.mount('#app');