<template>
    <div class="welcome-page">
        <div class="content-wrapper">
            <!-- 这里引用了图标 -->
            <!--  <img src="@/assets/posture-icon.svg" alt="Posture Icon" class="welcome-icon" /> -->
            <h1 class="title">体态评估与健康助手</h1>
            <p class="subtitle">智能分析您的体态，提供个性化建议，助您拥抱健康生活。</p>

            <nav class="navigation-links">
                <router-link to="/assessment" class="nav-button primary">
                    <!-- 你也可以为按钮内的文本前添加小图标 -->
                    <span class="button-icon">🚀</span> 开始评估
                </router-link>
                <router-link to="/history" class="nav-button secondary">
                    <span class="button-icon">📊</span> 查看历史数据
                </router-link>
                <router-link to="/llm" class="nav-button secondary">
                    <span class="button-icon">💡</span> AI 智能建议
                </router-link>
            </nav>

            <p class="welcome-footer">
                保持良好体态，从现在开始。
            </p>
        </div>
    </div>
</template>

<script setup>
// HomePage 通常比较简单，主要用于导航
// 如果需要，可以在这里添加其他逻辑，例如从配置中读取图标路径等
// import { ref } from 'vue';
// const showIcon = ref(true); // 在模板中已直接使用，这里不再需要单独控制，除非有动态逻辑
</script>

<style scoped>
/* @import url('https://fonts.googleapis.com/css2?family=Nunito:wght@300;400;600;700&display=swap'); 确保这个字体已在 index.html 或全局 CSS 中引入 */

.welcome-page {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: calc(100vh - 120px);
    /* 假设 header 和 footer 总高度，根据实际 App.vue 布局调整 */
    padding: 40px 20px;
    background: linear-gradient(135deg, #e0eafc 0%, #cfdef3 100%);
    /* 更柔和的渐变 */
    font-family: 'Nunito', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif;
    color: #3a3f47;
    /* 深灰色，更柔和 */
    box-sizing: border-box;
}

.content-wrapper {
    background-color: white;
    padding: 50px 60px;
    border-radius: 20px;
    /* 更大的圆角 */
    box-shadow: 0 15px 35px rgba(50, 50, 93, 0.1), 0 5px 15px rgba(0, 0, 0, 0.07);
    /* 更精致的阴影 */
    text-align: center;
    max-width: 650px;
    width: 100%;
    animation: fadeInScale 0.7s cubic-bezier(0.25, 0.46, 0.45, 0.94) both;
}

@keyframes fadeInScale {
    0% {
        opacity: 0;
        transform: translateY(30px) scale(0.95);
    }

    100% {
        opacity: 1;
        transform: translateY(0) scale(1);
    }
}

.welcome-icon {
    width: 70px;
    height: 70px;
    margin-bottom: 20px;
    /* SVG颜色可以通过CSS fill控制，如果SVG内部没有硬编码颜色 */
    /* color: #007bff;  如果SVG是字体图标或支持CSS颜色控制 */
    opacity: 0.85;
}

.title {
    font-size: 2.5em;
    /* 调整大小 */
    font-weight: 700;
    color: #1a2b48;
    /* 更深的标题蓝 */
    margin-bottom: 10px;
    line-height: 1.2;
}

.subtitle {
    font-size: 1.1em;
    /* 调整大小 */
    color: #5a6a7a;
    /* 中灰色 */
    margin-bottom: 35px;
    line-height: 1.6;
    max-width: 480px;
    margin-left: auto;
    margin-right: auto;
}

.navigation-links {
    display: flex;
    flex-direction: column;
    gap: 18px;
    align-items: center;
}

.nav-button {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    padding: 14px 28px;
    /* 调整padding */
    text-decoration: none;
    border-radius: 30px;
    /* 更圆的按钮 */
    font-size: 1.05em;
    /* 调整大小 */
    font-weight: 600;
    transition: all 0.25s cubic-bezier(0.25, 0.46, 0.45, 0.94);
    min-width: 230px;
    box-shadow: 0 4px 6px rgba(50, 50, 93, 0.11), 0 1px 3px rgba(0, 0, 0, 0.08);
    border: none;
    /* 移除边框，让背景和阴影更突出 */
}

.button-icon {
    /* 按钮内的小图标 */
    margin-right: 8px;
    font-size: 1.1em;
    /* 可以让emoji图标稍微大一点 */
    line-height: 1;
    /* 确保emoji垂直居中 */
}

.nav-button.primary {
    background-color: #5e72e4;
    /* 品牌主色调 - 示例 (可替换) */
    color: white;
}

.nav-button.primary:hover {
    background-color: #4156c5;
    /* 主色调加深 */
    transform: translateY(-2px);
    box-shadow: 0 7px 14px rgba(50, 50, 93, 0.1), 0 3px 6px rgba(0, 0, 0, 0.08);
}

.nav-button.secondary {
    background-color: #f7fafc;
    /* 浅灰色背景 */
    color: #5e72e4;
    /* 品牌主色调文字 */
    /* border: 1px solid #dee2e6;  如果需要边框 */
}

.nav-button.secondary:hover {
    background-color: #e9ecef;
    /* 悬停时背景加深 */
    color: #4156c5;
    transform: translateY(-2px);
    box-shadow: 0 7px 14px rgba(50, 50, 93, 0.1), 0 3px 6px rgba(0, 0, 0, 0.08);
}

.welcome-footer {
    margin-top: 35px;
    font-size: 0.9em;
    color: #8898aa;
    /* 更浅的灰色 */
}

/* 响应式设计 */
@media (min-width: 600px) {

    /* 在稍大屏幕上，按钮可以并排 */
    .navigation-links {
        flex-direction: row;
        justify-content: center;
        gap: 25px;
        /* 并排时间距可以大一些 */
    }

    .nav-button {
        min-width: auto;
        /* 并排时不需要固定最小宽度 */
    }
}

@media (max-width: 480px) {

    /* 非常小的屏幕 */
    .content-wrapper {
        padding: 35px 20px;
        /* 进一步减少padding */
        margin: 15px;
    }

    .title {
        font-size: 2em;
    }

    .subtitle {
        font-size: 0.95em;
        margin-bottom: 30px;
    }

    .nav-button {
        padding: 12px 20px;
        font-size: 1em;
        min-width: 200px;
    }

    .welcome-icon {
        width: 50px;
        height: 50px;
    }
}
</style>