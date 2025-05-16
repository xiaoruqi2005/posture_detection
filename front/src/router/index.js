import { createRouter, createWebHistory } from 'vue-router';

// 1. 导入你的页面组件
//    假设你将页面组件放在 src/views/ 目录下
import HomePage from '../views/HomePage.vue';
import AssessmentPage from '../views/AssessmentPage.vue';
import HistoryPage from '../views/HistoryPage.vue';
import LLMPage from '../views/LLMPage.vue';
import NotFoundPage from '../views/NotFoundPage.vue'; // 一个404页面

// 2. 定义路由
//    每个路由都需要映射到一个组件。
const routes = [
    {
        path: '/', // 根路径
        name: 'Home', // 路由名称 (可选, 但推荐)
        component: HomePage, // 对应的组件
    },
    {
        path: '/assessment',
        name: 'Assessment',
        component: AssessmentPage,
    },
    {
        path: '/history',
        name: 'History',
        component: HistoryPage,
    },
    {
        path: '/llm',
        name: 'LLM',
        component: LLMPage,
    },
    // 捕获所有未匹配的路由 (404 Not Found) - 应该放在最后
    {
        path: '/:catchAll(.*)*', // 使用动态参数和自定义正则表达式
        name: 'NotFound',
        component: NotFoundPage,
    },
];

// 3. 创建路由实例
const router = createRouter({
    // history: createWebHistory(), // 使用 HTML5 History 模式 (推荐，URL更美观，但服务器需要配置)
    // 或者使用 Hash 模式 (URL中带有 #，不需要服务器特殊配置)
    history: createWebHistory(import.meta.env.BASE_URL), // Vite 项目推荐方式
    routes, // 简写，相当于 routes: routes
});

// (可选) 全局导航守卫 (例如用于登录验证)
// router.beforeEach((to, from, next) => {
//   // ... 逻辑
//   next(); // 必须调用 next() 来解析钩子
// });

export default router;