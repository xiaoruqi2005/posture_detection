<template>
    <div id="app-layout">
        <header class="app-header">
            <router-link to="/" class="logo-link">体态评估系统</router-link>
            <nav class="global-nav">
                <router-link to="/">首页</router-link>
                <router-link :to="{ name: 'Assessment' }">体态评估 (命名路由)</router-link>
                <router-link to="/history">历史数据</router-link>
                <router-link to="/llm">AI建议</router-link>
                <router-link to="/some-path">Go to Some Page (示例)</router-link>
            </nav>
            <button @click="goToHistoryProgrammatically">查看历史 (编程式)</button>
        </header>

        <main class="app-main">
            <!-- 路由匹配到的组件将在这里渲染 -->
            <router-view v-slot="{ Component }">
                <transition name="fade" mode="out-in">
                    <component :is="Component" />
                </transition>
            </router-view>
        </main>

        <footer class="app-footer">
            <p>© {{ new Date().getFullYear() }} 我的课设</p>
        </footer>
    </div>
</template>

<script setup>
// App.vue 现在主要负责布局和全局元素
// 之前在 App.vue 中的页面特定逻辑应该移到各自的页面组件 (src/views/) 中

import { useRouter } from 'vue-router';

const router = useRouter(); // 获取路由实例

function goToHistoryProgrammatically() { // 重命名函数以避免与可能的其他 goToHistory 冲突
    router.push('/history');
    // 或者使用命名路由: router.push({ name: 'History' });
    // 或者带参数: router.push({ name: 'UserProfile', params: { userId: '123' } });
    console.log("Navigating to history programmatically");
}

// 如果 App.vue 还需要其他 setup 逻辑，都放在这里
</script>

<style>
/* 全局布局样式 */
#app-layout {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
    font-family: Avenir, Helvetica, Arial, sans-serif;
}

.app-header {
    background-color: #333;
    color: white;
    padding: 15px 30px;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.logo-link {
    font-size: 1.5em;
    color: white;
    text-decoration: none;
    font-weight: bold;
}

.global-nav a,
.app-header button {
    /* 将按钮也加入导航样式中 */
    color: #f2f2f2;
    text-align: center;
    padding: 10px 16px;
    /* 调整按钮padding */
    text-decoration: none;
    font-size: 17px;
    transition: color 0.3s, background-color 0.3s;
    /* 为按钮添加背景过渡 */
    border: none;
    /* 移除按钮默认边框 */
    background-color: transparent;
    /* 按钮背景透明 */
    cursor: pointer;
    /* 鼠标指针 */
    margin-left: 10px;
    /* 按钮与其他链接的间距 */
}

.app-header button:hover {
    background-color: #555;
    /* 按钮悬停背景 */
    color: #fff;
}

.global-nav a:hover {
    color: #ddd;
}

.global-nav a.router-link-exact-active {
    /* 当前激活路由的样式 */
    color: #42b983;
    /* Vue 绿色 */
    font-weight: bold;
}

.app-main {
    flex-grow: 1;
    padding: 20px;
    background-color: #f4f4f4;
}

.app-footer {
    background-color: #333;
    color: white;
    text-align: center;
    padding: 10px;
}

/* 页面切换过渡效果 (可选) */
.fade-enter-active,
.fade-leave-active {
    transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
    opacity: 0;
}
</style>