<!-- <template>
    <div class="page-container assessment-page">
        <h1 class="page-title">实时体态评估</h1>

        <section class="controls">
            <button @click="startAssessment" :disabled="isAssessing || isLoading" class="action-button start">
                <span v-if="isLoading && isStarting">正在启动...</span>
                <span v-else>🚀 开始评估</span>
            </button>
            <button @click="stopAssessment" :disabled="!isAssessing || isLoading" class="action-button stop">
                <span v-if="isLoading && !isStarting">正在停止...</span>
                <span v-else>🛑 停止评估</span>
            </button>
        </section>

        <section class="status-panel">
            <p :class="['status-message', connectionStatus.type]">
                服务器连接状态: <strong>{{ connectionStatus.message }}</strong>
            </p>
            <p v-if="isAssessing && !realtimeData && connectionStatus.type === 'success'" class="status-message info">
                正在等待实时数据...
            </p>
        </section>

        <section v-if="isAssessing || lastKnownData" class="realtime-data-display">
            <h2 class="section-title">实时数据面板</h2>
            <div v-if="realtimeData || lastKnownData" class="data-cards">
                <div class="data-card">
                    <h3 class="card-title">头部倾斜角</h3>
                    <p class="card-value">
                        {{ (realtimeData || lastKnownData)?.headTilt?.toFixed(1) ?? 'N/A' }}°
                    </p>
                    <p class="card-interpretation">{{ getTiltInterpretation((realtimeData || lastKnownData)?.headTilt)
                    }}</p>
                </div>
                <div class="data-card">
                    <h3 class="card-title">两肩倾斜角</h3>
                    <p class="card-value">
                        {{ (realtimeData || lastKnownData)?.shoulderTilt?.toFixed(1) ?? 'N/A' }}°
                    </p>
                    <p class="card-interpretation">{{ getTiltInterpretation((realtimeData ||
                        lastKnownData)?.shoulderTilt) }}</p>
                </div>
                <div :class="['data-card', hunchbackStatusClass((realtimeData || lastKnownData)?.isHunchbacked)]">
                    <h3 class="card-title">是否驼背</h3>
                    <p class="card-value big-status">
                        {{ (realtimeData || lastKnownData)?.isHunchbacked ? '是' : '否' }}
                    </p>
                    <p class="card-interpretation">{{ (realtimeData || lastKnownData)?.isHunchbacked ? '请注意调整姿势！' :
                        '体态良好！' }}</p>
                </div>
            </div>
            <p v-if="isAssessing && !realtimeData && connectionStatus.type === 'success'" class="no-data-placeholder">
                数据传输中...
            </p>
            <p v-if="!isAssessing && lastKnownData" class="info-text">
                评估已停止，显示最后一次接收到的数据。
            </p>
        </section>

        <div class="navigation-footer">
            <router-link to="/" class="back-button">返回首页</router-link>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue';
import { postureApi } from '@/services/apiService'; // 调整路径
import {
    createSignalRConnection,
    startSignalRConnection,
    stopSignalRConnection,
    registerDataHandler,
    registerConnectionStatusHandler,
    cleanupSignalR
} from '@/services/signalrService'; // 调整路径

const isAssessing = ref(false);
const isLoading = ref(false); // 用于 API 调用加载状态
const isStarting = ref(false); // 用于区分是启动还是停止的加载
const realtimeData = ref(null);
const lastKnownData = ref(null); // 存储最后一次有效数据

const connectionStatus = ref({ type: 'info', message: '未连接' }); // success, error, info

// --- SignalR Handlers ---
const handleNewPostureData = (data) => {
    realtimeData.value = data;
    lastKnownData.value = data; // 持续更新最后已知数据
};

const handleConnectionStatus = (isConnected, message) => {
    connectionStatus.value = {
        type: isConnected ? 'success' : (message.includes('失败') || message.includes('断开') && !message.includes('手动') ? 'error' : 'info'),
        message: message
    };
    if (!isConnected && isAssessing.value) {
        // 如果评估中连接断开，可以考虑自动停止评估状态或提示用户
        isAssessing.value = false;
        console.warn("评估已开始，但 SignalR 连接中断。");
    }
};

// --- API Calls ---
async function startAssessment() {
    if (isAssessing.value) return;
    isLoading.value = true;
    isStarting.value = true;
    realtimeData.value = null; // 清空旧的实时数据
    try {
        await postureApi.start();
        isAssessing.value = true;
        console.log("Assessment started via API.");
        // 确保 SignalR 连接在开始评估后启动或已连接
        await startSignalRConnection();
    } catch (error) {
        console.error("Error starting assessment:", error);
        connectionStatus.value = { type: 'error', message: '启动评估失败: ' + (error.response?.data?.message || error.message) };
    } finally {
        isLoading.value = false;
        isStarting.value = false;
    }
}

async function stopAssessment() {
    if (!isAssessing.value) return;
    isLoading.value = true;
    isStarting.value = false;
    try {
        await postureApi.stop();
        isAssessing.value = false;
        console.log("Assessment stopped via API.");
        // 可选: 停止评估后也断开 SignalR 连接，或保持连接以便快速重启
        await stopSignalRConnection();
    } catch (error) {
        console.error("Error stopping assessment:", error);
        connectionStatus.value = { type: 'error', message: '停止评估失败: ' + (error.response?.data?.message || error.message) };
    } finally {
        isLoading.value = false;
    }
}

// --- Helper Functions for UI ---
function getTiltInterpretation(angle) {
    if (angle == null) return '等待数据';
    const absAngle = Math.abs(angle);
    if (absAngle < 5) return '正常范围';
    if (absAngle < 10) return '轻微倾斜';
    return '明显倾斜，请注意调整';
}

const hunchbackStatusClass = computed(() => (isHunchbacked) => {
    if (isHunchbacked === null || isHunchbacked === undefined) return '';
    return isHunchbacked ? 'status-warning' : 'status-ok';
});


// --- Lifecycle Hooks ---
onMounted(() => {
    createSignalRConnection(); // 创建但不立即启动连接
    registerDataHandler(handleNewPostureData);
    registerConnectionStatusHandler(handleConnectionStatus);
    // 可以选择在页面加载时尝试连接，或者在点击“开始评估”时再连接
    // await startSignalRConnection();
});

onUnmounted(() => {
    // 如果仍在评估中，离开页面时自动停止评估
    if (isAssessing.value) {
        stopAssessment(); // 这个是异步的，但 onUnmounted 通常是同步的
    }
    cleanupSignalR(); // 清理 SignalR 相关的事件监听器和连接
});

</script>

<style scoped>
.page-container.assessment-page {
    max-width: 900px;
    margin: 20px auto;
    padding: 30px;
    background-color: #f9f9f9;
    border-radius: 12px;
    box-shadow: 0 8px 25px rgba(0, 0, 0, 0.08);
    font-family: 'Nunito', sans-serif;
}

.page-title {
    text-align: center;
    color: #333;
    font-size: 2.2em;
    margin-bottom: 30px;
}

.controls {
    display: flex;
    justify-content: center;
    gap: 20px;
    margin-bottom: 30px;
}

.action-button {
    padding: 12px 25px;
    font-size: 1.1em;
    font-weight: 600;
    border: none;
    border-radius: 25px;
    cursor: pointer;
    transition: all 0.3s ease;
    min-width: 150px;
}

.action-button.start {
    background-color: #28a745;
    color: white;
}

.action-button.start:hover:not(:disabled) {
    background-color: #218838;
    transform: translateY(-2px);
    box-shadow: 0 4px 10px rgba(40, 167, 69, 0.3);
}

.action-button.stop {
    background-color: #dc3545;
    color: white;
}

.action-button.stop:hover:not(:disabled) {
    background-color: #c82333;
    transform: translateY(-2px);
    box-shadow: 0 4px 10px rgba(220, 53, 69, 0.3);
}

.action-button:disabled {
    background-color: #ccc;
    cursor: not-allowed;
    opacity: 0.7;
}

.status-panel {
    background-color: #fff;
    padding: 15px;
    border-radius: 8px;
    margin-bottom: 30px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    text-align: center;
}

.status-message {
    font-size: 1em;
    margin: 5px 0;
}

.status-message.success strong {
    color: #28a745;
}

.status-message.error strong {
    color: #dc3545;
}

.status-message.info strong {
    color: #17a2b8;
}

.realtime-data-display {
    background-color: #fff;
    padding: 25px;
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.07);
}

.section-title {
    text-align: center;
    color: #444;
    font-size: 1.6em;
    margin-bottom: 25px;
    border-bottom: 1px solid #eee;
    padding-bottom: 10px;
}

.data-cards {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
    gap: 20px;
}

.data-card {
    background-color: #f8f9fa;
    padding: 20px;
    border-radius: 8px;
    text-align: center;
    border: 1px solid #e0e0e0;
    transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.data-card:hover {
    transform: translateY(-3px);
    box-shadow: 0 6px 15px rgba(0, 0, 0, 0.08);
}

.card-title {
    font-size: 1.1em;
    color: #555;
    margin-bottom: 10px;
    font-weight: 600;
}

.card-value {
    font-size: 1.8em;
    font-weight: 700;
    color: #007bff;
    margin-bottom: 8px;
}

.card-value.big-status {
    font-size: 2.2em;
}

.card-interpretation {
    font-size: 0.9em;
    color: #6c757d;
    min-height: 2.7em;
    /* 保证有足够空间放两行文字 */
}

.data-card.status-warning .card-value,
.data-card.status-warning .card-title {
    color: #ffc107;
    /* 黄色警告 */
}

.data-card.status-warning {
    border-left: 5px solid #ffc107;
}

.data-card.status-ok .card-value,
.data-card.status-ok .card-title {
    color: #28a745;
    /* 绿色正常 */
}

.data-card.status-ok {
    border-left: 5px solid #28a745;
}


.no-data-placeholder,
.info-text {
    text-align: center;
    color: #6c757d;
    font-style: italic;
    margin-top: 20px;
    padding: 15px;
    background-color: #e9ecef;
    border-radius: 6px;
}

.navigation-footer {
    text-align: center;
    margin-top: 30px;
}

.back-button {
    display: inline-block;
    padding: 10px 20px;
    background-color: #6c757d;
    color: white;
    text-decoration: none;
    border-radius: 20px;
    font-weight: 500;
    transition: background-color 0.3s;
}

.back-button:hover {
    background-color: #5a6268;
}
</style> -->

<template>
    <div class="page-container assessment-page">
        <h1 class="page-title">实时体态评估</h1>

        <!-- 通知权限提示 -->
        <div v-if="showPermissionBanner" class="permission-banner">
            <p>为了在检测到不良体态时及时提醒您，请允许通知权限</p>
            <button @click="requestNotificationPermission" class="permission-button">允许通知</button>
        </div>

        <section class="controls">
            <button @click="startAssessment" :disabled="isAssessing || isLoading" class="action-button start">
                <span v-if="isLoading && isStarting">正在启动...</span>
                <span v-else>🚀 开始评估</span>
            </button>
            <button @click="stopAssessment" :disabled="!isAssessing || isLoading" class="action-button stop">
                <span v-if="isLoading && !isStarting">正在停止...</span>
                <span v-else>🛑 停止评估</span>
            </button>
        </section>

        <section class="status-panel">
            <p :class="['status-message', connectionStatus.type]">
                服务器连接状态: <strong>{{ connectionStatus.message }}</strong>
            </p>
            <p v-if="isAssessing && !currentDataToDisplay && connectionStatus.type === 'success'"
               class="status-message info">
                正在等待实时数据...
            </p>
            <p v-if="notificationStatus" :class="['status-message', notificationStatus.type]">
                通知状态: <strong>{{ notificationStatus.message }}</strong>
            </p>
        </section>

        <section v-if="isAssessing || lastKnownData" class="realtime-data-display">
            <h2 class="section-title">实时数据面板</h2>
            <div v-if="currentDataToDisplay" class="data-cards">
                <!-- 头部倾斜 -->
                <div :class="['data-card', getSeverityClass(currentDataToDisplay.headTiltState, 'head')]">
                    <h3 class="card-title">头部姿态</h3>
                    <p class="card-value small-angle">
                        {{ currentDataToDisplay.headTiltAngle?.toFixed(1) ?? 'N/A' }}°
                    </p>
                    <p v-if="headTiltDuration > 0" class="duration-display">
                        持续时间: {{ headTiltDuration }}秒
                    </p>
                    <p class="card-state">{{ headTiltStateText(currentDataToDisplay.headTiltState) }}</p>
                    <p class="card-interpretation">
                        {{ getHeadTiltInterpretation(currentDataToDisplay.headTiltState) }}
                    </p>
                </div>

                <!-- 两肩水平 -->
                <div :class="['data-card', getSeverityClass(currentDataToDisplay.shoulderState, 'shoulder')]">
                    <h3 class="card-title">双肩水平</h3>
                    <p class="card-value small-angle">
                        {{ currentDataToDisplay.shoulderTiltAngle?.toFixed(1) ?? 'N/A' }}°
                    </p>
                    <p v-if="shoulderDuration > 0" class="duration-display">
                        持续时间: {{ shoulderDuration }}秒
                    </p>
                    <p class="card-state">{{ shoulderStateText(currentDataToDisplay.shoulderState) }}</p>
                    <p class="card-interpretation">
                        {{ getShoulderInterpretation(currentDataToDisplay.shoulderState) }}
                    </p>
                </div>

                <!-- 驼背状态 -->
                <div :class="['data-card', getSeverityClass(currentDataToDisplay.hunchbackState, 'hunchback')]">
                    <h3 class="card-title">驼背状态</h3>
                    <p class="card-value big-status">
                        {{ hunchbackStateText(currentDataToDisplay.hunchbackState) }}
                    </p>
                    <p v-if="hunchbackDuration > 0" class="duration-display">
                        持续时间: {{ hunchbackDuration }}秒
                    </p>
                    <p class="card-interpretation">
                        {{ getHunchbackInterpretation(currentDataToDisplay.hunchbackState) }}
                    </p>
                </div>
            </div>
            <p v-if="isAssessing && !currentDataToDisplay && connectionStatus.type === 'success'"
               class="no-data-placeholder">
                数据传输中...
            </p>
            <p v-if="!isAssessing && lastKnownData" class="info-text">
                评估已停止，显示最后一次接收到的数据。
            </p>
        </section>

        <!-- 通知设置面板 -->
        <section class="notification-settings">
            <h2 class="section-title">通知设置</h2>
            <div class="settings-options">
                <label class="setting-option">
                    <input type="checkbox" v-model="notificationSettings.enabled">
                    启用不良体态通知
                </label>
                <label class="setting-option" v-if="notificationSettings.enabled">
                    <input type="checkbox" v-model="notificationSettings.sound">
                    启用通知声音
                </label>
                <label class="setting-option" v-if="notificationSettings.enabled">
                    通知间隔(分钟):
                    <input type="number" v-model.number="notificationSettings.interval" min="1" max="60">
                </label>
            </div>
        </section>

        <div class="navigation-footer">
            <router-link to="/" class="back-button">返回首页</router-link>
        </div>

        <!-- 驼背警告弹窗 -->
        <div v-if="showHunchbackWarning" class="warning-dialog-overlay">
            <div class="warning-dialog">
                <h3>⚠️ 长期驼背警告</h3>
                <div class="warning-content">
                    <p>您已保持<strong>{{ hunchbackStateText(currentDataToDisplay?.hunchbackState) }}</strong>状态超过30秒！</p>
                    <p>长时间保持不良姿势可能导致：</p>
                    <ul>
                        <li>颈椎和脊椎损伤</li>
                        <li>肌肉疲劳和疼痛</li>
                        <li>血液循环不良</li>
                    </ul>
                    <p class="suggestion-title">建议立即：</p>
                    <ol class="suggestion-steps">
                        <li>挺直背部，肩胛骨自然下沉</li>
                        <li>收下巴，保持头部正直</li>
                        <li>双脚平放在地面上</li>
                        <li>做3次深呼吸放松肩颈</li>
                    </ol>
                </div>
                <button @click="closeWarningDialog" class="confirm-button">我已调整姿势</button>
            </div>
        </div>
        <!-- 头部倾斜警告弹窗 -->
        <div v-if="showHeadTiltWarning" class="warning-dialog-overlay">
            <div class="warning-dialog">
                <h3>⚠️ 长期头部倾斜警告</h3>
                <div class="warning-content">
                    <p>您的头部已保持<strong>{{ headTiltStateText(currentDataToDisplay?.headTiltState) }}</strong>状态超过30秒！</p>
                    <p>长时间保持头部倾斜可能导致：</p>
                    <ul>
                        <li>颈椎压力增加</li>
                        <li>颈部肌肉紧张</li>
                        <li>头痛和不适</li>
                    </ul>
                    <p class="suggestion-title">建议立即：</p>
                    <ol class="suggestion-steps">
                        <li>将头部回到正中位置</li>
                        <li>放松颈部肌肉</li>
                        <li>做缓慢的颈部伸展运动</li>
                        <li>调整显示器高度与视线平齐</li>
                    </ol>
                </div>
                <button @click="closeHeadTiltWarning" class="confirm-button">我已调整姿势</button>
            </div>
        </div>

        <!-- 高低肩警告弹窗 -->
        <div v-if="showShoulderWarning" class="warning-dialog-overlay">
            <div class="warning-dialog">
                <h3>⚠️ 长期高低肩警告</h3>
                <div class="warning-content">
                    <p>您已保持<strong>{{ shoulderStateText(currentDataToDisplay?.shoulderState) }}</strong>状态超过30秒！</p>
                    <p>长时间保持高低肩可能导致：</p>
                    <ul>
                        <li>肩颈肌肉不平衡</li>
                        <li>脊椎侧弯风险</li>
                        <li>肩部疼痛和僵硬</li>
                    </ul>
                    <p class="suggestion-title">建议立即：</p>
                    <ol class="suggestion-steps">
                        <li>放松双肩，让它们自然下垂</li>
                        <li>有意识地降低较高的肩膀</li>
                        <li>做肩部环绕运动</li>
                        <li>检查工作台高度是否合适</li>
                    </ol>
                </div>
                <button @click="closeShoulderWarning" class="confirm-button">我已调整姿势</button>
            </div>
        </div>
    </div>
</template>

<script setup>
    // ... (你的 <script setup> 内容保持不变) ...
    import { ref, onMounted, onUnmounted, computed, onBeforeUnmount } from 'vue';
    import { postureApi } from '@/services/apiService';
    import {
        createSignalRConnection,
        startSignalRConnection,
        stopSignalRConnection,
        registerDataHandler,
        registerConnectionStatusHandler,
        cleanupSignalR
    } from '@/services/signalrService';

    const isAssessing = ref(false);
    const isLoading = ref(false);
    const isStarting = ref(false);
    const realtimeData = ref(null);
    const lastKnownData = ref(null);


    // 通知相关状态
    const showPermissionBanner = ref(false);
    const notificationStatus = ref(null);
    const notificationSettings = ref({
        enabled: true,
        sound: true,
        interval: 1 // 分钟
    });

    // 驼背计时相关
    const hunchbackTimer = ref(null);
    const hunchbackStartTime = ref(0);
    const hunchbackDuration = ref(0);
    const showHunchbackWarning = ref(false);
    const lastWarningTime = ref(0);

    // Add these new refs for head and shoulder timers
    const headTiltTimer = ref(null);
    const headTiltStartTime = ref(0);
    const headTiltDuration = ref(0);
    const showHeadTiltWarning = ref(false);

    const shoulderTimer = ref(null);
    const shoulderStartTime = ref(0);
    const shoulderDuration = ref(0);
    const showShoulderWarning = ref(false);

    const connectionStatus = ref({ type: 'info', message: '未连接' });




    const currentDataToDisplay = computed(() => {
        return realtimeData.value || lastKnownData.value;
    });

    const handleNewPostureData = (data) => {
        console.log("Received posture data:", data);
        realtimeData.value = data;
        lastKnownData.value = data;

        // Handle all timing logic
        handleHunchbackTiming(data.hunchbackState);
        handleHeadTiltTiming(data.headTiltState);
        handleShoulderTiming(data.shoulderState);
    };

    const handleConnectionStatus = (isConnected, message) => {
        connectionStatus.value = {
            type: isConnected ? 'success' : (message.includes('失败') || message.includes('断开') && !message.includes('手动') ? 'error' : 'info'),
            message: message
        };
        if (!isConnected && isAssessing.value) {
            console.warn("评估进行中，但 SignalR 连接已中断。");
        }
    };

    async function startAssessment() {
        if (isAssessing.value) return;
        isLoading.value = true;
        isStarting.value = true;
        realtimeData.value = null;
        try {
            await postureApi.start();
            isAssessing.value = true;
            await startSignalRConnection();
        } catch (error) {
            connectionStatus.value = { type: 'error', message: '启动评估失败: ' + (error.response?.data?.message || error.message) };
        } finally {
            isLoading.value = false;
            isStarting.value = false;
        }
    }

    // async function stopAssessment() {
    //     if (!isAssessing.value) return;
    //     isLoading.value = true;
    //     isStarting.value = false;
    //     try {
    //         await postureApi.stop();
    //         isAssessing.value = false;
    //         realtimeData.value = null;
    //     } catch (error) {
    //         connectionStatus.value = { type: 'error', message: '停止评估失败: ' + (error.response?.data?.message || error.message) };
    //     } finally {
    //         isLoading.value = false;
    //     }
    // }

    // Modify stopAssessment to reset all timers
    async function stopAssessment() {
        if (!isAssessing.value) return;
        isLoading.value = true;
        isStarting.value = false;
        try {
            await postureApi.stop();
            isAssessing.value = false;
            realtimeData.value = null;

            // Reset all timers when assessment stops
            resetHunchbackTimer();
            resetHeadTiltTimer();
            resetShoulderTimer();
        } catch (error) {
            connectionStatus.value = { type: 'error', message: '停止评估失败: ' + (error.response?.data?.message || error.message) };
        } finally {
            isLoading.value = false;
        }
    }

    const headTiltStateText = (state) => {
        if (!state) return 'N/A';
        switch (state) {
            case 'Unknown': return '未知';
            case 'Upright': return '正直';
            case 'SlightlyTiltedLeft': return '轻微左倾';
            case 'SignificantlyTiltedLeft': return '明显左倾';
            case 'SlightlyTiltedRight': return '轻微右倾';
            case 'SignificantlyTiltedRight': return '明显右倾';
            default: return state;
        }
    };

    const getHeadTiltInterpretation = (state) => {
        if (!state || state === 'Unknown') return '等待数据或无法评估';
        if (state === 'Upright') return '头部姿态良好！';
        if (state === 'SlightlyTiltedLeft' || state === 'SlightlyTiltedRight') return '轻微倾斜，请注意放松颈部。';
        return '倾斜明显，请及时调整！';
    };

    const shoulderStateText = (state) => {
        if (!state) return 'N/A';
        switch (state) {
            case 'Unknown': return '未知';
            case 'Level': return '水平';
            case 'LeftSlightlyHigh': return '左肩轻微偏高';
            case 'RightSlightlyHigh': return '右肩轻微偏高';
            case 'LeftObviouslyHigh': return '左肩明显偏高';
            case 'RightObviouslyHigh': return '右肩明显偏高';
            default: return state;
        }
    };

    const getShoulderInterpretation = (state) => {
        if (!state || state === 'Unknown') return '等待数据或无法评估';
        if (state === 'Level') return '双肩姿态良好！';
        return '双肩不平，请注意放松并调整。';
    };

    const hunchbackStateText = (state) => {
        if (!state) return 'N/A';
        switch (state) {
            case 'Unknown': return '未知';
            case 'NoHunchback': return '无驼背';
            case 'SlightHunchback': return '轻微驼背';
            case 'ObviousHunchback': return '明显驼背';
            default: return state;
        }
    };

    const getHunchbackInterpretation = (state) => {
        if (!state || state === 'Unknown') return '等待数据或无法评估';
        if (state === 'NoHunchback') return '背部姿态良好！';
        if (state === 'SlightHunchback') return '轻微驼背，请注意挺直腰背。';
        return '驼背明显，请务必调整姿势！';
    };

    const getSeverityClass = (state, type) => {
        if (!state || state === 'Unknown') return 'status-unknown';
        if (type === 'hunchback') {
            if (state === 'NoHunchback') return 'status-ok';
            if (state === 'SlightHunchback') return 'status-notice';
            if (state === 'ObviousHunchback') return 'status-warning';
        } else {
            if (state === 'Upright' || state === 'Level') return 'status-ok';
            if (state.includes('Slightly')) return 'status-notice';
            if (state.includes('Significantly') || state.includes('High')) return 'status-warning';
        }
        return '';
    };

    // 检查通知支持性
    const isNotificationSupported = () => 'Notification' in window;

    // 请求通知权限
    const requestNotificationPermission = async () => {
        if (!isNotificationSupported()) {
            notificationStatus.value = {
                type: 'error',
                message: '您的浏览器不支持通知功能'
            };
            return;
        }

        try {
            const permission = await Notification.requestPermission();
            if (permission === 'granted') {
                notificationStatus.value = {
                    type: 'success',
                    message: '通知权限已授予'
                };
                showPermissionBanner.value = false;
                showNotification('测试通知', '体态评估通知功能已启用');
            } else {
                notificationStatus.value = {
                    type: 'warning',
                    message: '通知权限被拒绝'
                };
            }
        } catch (error) {
            console.error('请求通知权限失败:', error);
            notificationStatus.value = {
                type: 'error',
                message: '请求通知权限失败'
            };
        }
    };


    // 显示通知
    const showNotification = (title, body) => {
        if (!notificationSettings.value.enabled) return;
        if (!isNotificationSupported() || Notification.permission !== 'granted') return;

        // 检查通知间隔
        const now = Date.now();
        if (now - lastWarningTime.value < notificationSettings.value.interval * 60 * 1000) {
            return;
        }
        lastWarningTime.value = now;

        // 创建通知
        const notification = new Notification(title, {
            body: body,
            icon: '/notification-icon.png',
            vibrate: [200, 100, 200]
        });

        // 播放声音
        if (notificationSettings.value.sound) {
            const audio = new Audio('/notification-sound.mp3');
            audio.play().catch(e => console.error('播放通知声音失败:', e));
        }

        // 点击通知时的行为
        notification.onclick = () => {
            window.focus();
            notification.close();
        };

        // 自动关闭通知
        setTimeout(() => notification.close(), 5000);
    };

    // 驼背状态计时逻辑
    const handleHunchbackTiming = (currentState) => {
        const isBadHunchback = currentState === 'SlightHunchback' || currentState === 'ObviousHunchback';

        if (isBadHunchback && !hunchbackTimer.value) {
            startHunchbackTimer();
        } else if (!isBadHunchback && hunchbackTimer.value) {
            resetHunchbackTimer();
        }

        // 更新持续时间显示
        if (hunchbackTimer.value) {
            hunchbackDuration.value = Math.floor((Date.now() - hunchbackStartTime.value) / 1000);

            // 检查是否达到30秒
            if (hunchbackDuration.value >= 30 && !showHunchbackWarning.value) {
                triggerHunchbackWarning();
            }
        }
    };

    // 开始驼背计时器
    const startHunchbackTimer = () => {
        hunchbackStartTime.value = Date.now();
        hunchbackDuration.value = 0;
        showHunchbackWarning.value = false;

        hunchbackTimer.value = setInterval(() => {
            hunchbackDuration.value = Math.floor((Date.now() - hunchbackStartTime.value) / 1000);

            if (hunchbackDuration.value >= 30 && !showHunchbackWarning.value) {
                triggerHunchbackWarning();
            }
        }, 1000);
    };

    // 重置驼背计时器
    const resetHunchbackTimer = () => {
        if (hunchbackTimer.value) {
            clearInterval(hunchbackTimer.value);
            hunchbackTimer.value = null;
        }
        hunchbackDuration.value = 0;
        showHunchbackWarning.value = false;
    };

    // 触发驼背警告
    const triggerHunchbackWarning = () => {
        showHunchbackWarning.value = true;

        // 发送通知
        if (Notification.permission === 'granted') {
            showNotification('长期驼背警告', '您已保持不良姿势超过30秒，请立即调整坐姿！');
        }
    };

    // 关闭警告弹窗
    const closeWarningDialog = () => {
        showHunchbackWarning.value = false;
    };

    // Add new timer functions for head tilt
    const handleHeadTiltTiming = (currentState) => {
        const isBadHeadTilt = currentState === 'SlightlyTiltedLeft' ||
            currentState === 'SignificantlyTiltedLeft' ||
            currentState === 'SlightlyTiltedRight' ||
            currentState === 'SignificantlyTiltedRight';

        if (isBadHeadTilt && !headTiltTimer.value) {
            startHeadTiltTimer();
        } else if (!isBadHeadTilt && headTiltTimer.value) {
            resetHeadTiltTimer();
        }

        if (headTiltTimer.value) {
            headTiltDuration.value = Math.floor((Date.now() - headTiltStartTime.value) / 1000);

            if (headTiltDuration.value >= 30 && !showHeadTiltWarning.value) {
                triggerHeadTiltWarning();
            }
        }
    };

    const startHeadTiltTimer = () => {
        headTiltStartTime.value = Date.now();
        headTiltDuration.value = 0;
        showHeadTiltWarning.value = false;

        headTiltTimer.value = setInterval(() => {
            headTiltDuration.value = Math.floor((Date.now() - headTiltStartTime.value) / 1000);

            if (headTiltDuration.value >= 30 && !showHeadTiltWarning.value) {
                triggerHeadTiltWarning();
            }
        }, 1000);
    };

    const resetHeadTiltTimer = () => {
        if (headTiltTimer.value) {
            clearInterval(headTiltTimer.value);
            headTiltTimer.value = null;
        }
        headTiltDuration.value = 0;
        showHeadTiltWarning.value = false;
    };

    const triggerHeadTiltWarning = () => {
        showHeadTiltWarning.value = true;

        if (Notification.permission === 'granted') {
            showNotification('头部姿态警告', '您的头部已保持倾斜姿势超过30秒，请立即调整！');
        }
    };

    const closeHeadTiltWarning = () => {
        showHeadTiltWarning.value = false;
    };


    // Add new timer functions for shoulder tilt
    const handleShoulderTiming = (currentState) => {
        const isBadShoulder = currentState === 'LeftSlightlyHigh' || currentState === 'RightSlightlyHigh' || currentState === 'LeftObviouslyHigh' || currentState === 'RightObviouslyHigh';

        if (isBadShoulder && !shoulderTimer.value) {
            startShoulderTimer();
        } else if (!isBadShoulder && shoulderTimer.value) {
            resetShoulderTimer();
        }

        if (shoulderTimer.value) {
            shoulderDuration.value = Math.floor((Date.now() - shoulderStartTime.value) / 1000);

            if (shoulderDuration.value >= 30 && !showShoulderWarning.value) {
                triggerShoulderWarning();
            }
        }
    };

    const startShoulderTimer = () => {
        shoulderStartTime.value = Date.now();
        shoulderDuration.value = 0;
        showShoulderWarning.value = false;

        shoulderTimer.value = setInterval(() => {
            shoulderDuration.value = Math.floor((Date.now() - shoulderStartTime.value) / 1000);

            if (shoulderDuration.value >= 30 && !showShoulderWarning.value) {
                triggerShoulderWarning();
            }
        }, 1000);
    };

    const resetShoulderTimer = () => {
        if (shoulderTimer.value) {
            clearInterval(shoulderTimer.value);
            shoulderTimer.value = null;
        }
        shoulderDuration.value = 0;
        showShoulderWarning.value = false;
    };

    const triggerShoulderWarning = () => {
        showShoulderWarning.value = true;

        if (Notification.permission === 'granted') {
            showNotification('高低肩警告', '您已保持高低肩姿势超过30秒，请立即调整！');
        }
    };

    const closeShoulderWarning = () => {
        showShoulderWarning.value = false;
    };




    onMounted(() => {
        createSignalRConnection();
        registerDataHandler(handleNewPostureData);
        registerConnectionStatusHandler(handleConnectionStatus);
        if (isNotificationSupported()) {
            if (Notification.permission === 'default') {
                showPermissionBanner.value = true;
            } else if (Notification.permission === 'granted') {
                notificationStatus.value = {
                    type: 'success',
                    message: '通知权限已授予'
                };
            } else {
                notificationStatus.value = {
                    type: 'warning',
                    message: '通知权限被拒绝'
                };
            }
        } else {
            notificationStatus.value = {
                type: 'error',
                message: '您的浏览器不支持通知功能'
            };
        }
    });

    // // 组件卸载时清理
    // onBeforeUnmount(() => {
    //     resetHunchbackTimer();
    // });

    // Add cleanup for new timers in onBeforeUnmount
    onBeforeUnmount(() => {
        resetHunchbackTimer();
        resetHeadTiltTimer();
        resetShoulderTimer();
    });

    onUnmounted(() => {
        if (isAssessing.value) {
            stopAssessment();
        }
        cleanupSignalR();
    });
</script>

<style scoped>
    .page-container.assessment-page {
        max-width: 960px;
        /* 稍微加宽以容纳三张卡片并排 */
        margin: 30px auto;
        /* 上下间距调整 */
        padding: 30px 40px;
        /* 内边距调整 */
        background-color: #f7f9fc;
        /* 更淡雅的背景色 */
        border-radius: 16px;
        /* 更大的圆角 */
        box-shadow: 0 10px 30px rgba(0, 0, 0, 0.07);
        /* 柔和阴影 */
        font-family: 'Nunito', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif;
    }

    .page-title {
        text-align: center;
        color: #334155;
        /* 深蓝灰色 */
        font-size: 2.5em;
        /* 调整标题大小 */
        font-weight: 700;
        margin-bottom: 35px;
        letter-spacing: -0.5px;
    }

    .controls {
        display: flex;
        justify-content: center;
        gap: 25px;
        /* 按钮间距 */
        margin-bottom: 35px;
    }

    .action-button {
        padding: 14px 30px;
        /* 调整按钮padding */
        font-size: 1.15em;
        /* 按钮字体大小 */
        font-weight: 600;
        border: none;
        border-radius: 30px;
        /* 完全圆角 */
        cursor: pointer;
        transition: all 0.25s cubic-bezier(0.25, 0.46, 0.45, 0.94);
        /* 平滑过渡 */
        min-width: 160px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        /* 按钮阴影 */
        display: inline-flex;
        /* 让span元素和文字在同一行 */
        align-items: center;
        justify-content: center;
    }

        .action-button span {
            /* 按钮内的emoji或图标 */
            margin-right: 8px;
        }

        .action-button.start {
            background-color: #48bb78;
            /* 柔和的绿色 */
            color: white;
        }

            .action-button.start:hover:not(:disabled) {
                background-color: #38a169;
                /* 绿色加深 */
                transform: translateY(-2px);
                box-shadow: 0 6px 15px rgba(72, 187, 120, 0.3);
            }

        .action-button.stop {
            background-color: #f56565;
            /* 柔和的红色 */
            color: white;
        }

            .action-button.stop:hover:not(:disabled) {
                background-color: #e53e3e;
                /* 红色加深 */
                transform: translateY(-2px);
                box-shadow: 0 6px 15px rgba(245, 101, 101, 0.3);
            }

        .action-button:disabled {
            background-color: #cbd5e0;
            /* 禁用时灰色 */
            color: #a0aec0;
            cursor: not-allowed;
            box-shadow: none;
            transform: translateY(0);
            opacity: 0.8;
        }

    .status-panel {
        background-color: #ffffff;
        padding: 18px 25px;
        /* 调整padding */
        border-radius: 10px;
        /* 圆角 */
        margin-bottom: 35px;
        box-shadow: 0 3px 10px rgba(0, 0, 0, 0.04);
        text-align: center;
        border-left: 5px solid #63b3ed;
        /* 默认蓝色边框 */
    }

    .status-message {
        font-size: 1.05em;
        /* 调整字体大小 */
        margin: 5px 0;
        color: #4a5568;
        /* 文本颜色 */
    }

        .status-message strong {
            font-weight: 600;
        }

        .status-message.success {
            border-left-color: #48bb78;
        }

            /* 成功时绿色边框 */
            .status-message.success strong {
                color: #38a169;
            }

        .status-message.error {
            border-left-color: #f56565;
        }

            /* 错误时红色边框 */
            .status-message.error strong {
                color: #c53030;
            }

        .status-message.info {
            border-left-color: #63b3ed;
        }

            /* 信息时蓝色边框 */
            .status-message.info strong {
                color: #3182ce;
            }


    .realtime-data-display {
        background-color: transparent;
        /* 数据面板背景透明，依赖父级背景 */
        padding: 0;
        /* 移除内边距，由卡片自身处理 */
        border-radius: 0;
        box-shadow: none;
        /* 移除外层阴影，由卡片自身处理 */
    }

    .section-title {
        text-align: center;
        color: #4a5568;
        /* 标题颜色 */
        font-size: 1.8em;
        /* 标题字体大小 */
        font-weight: 700;
        margin-bottom: 30px;
        padding-bottom: 15px;
        position: relative;
    }

        .section-title::after {
            /* 标题下划线装饰 */
            content: '';
            position: absolute;
            display: block;
            width: 60px;
            height: 3px;
            background: #63b3ed;
            /* 装饰线颜色 */
            bottom: 0;
            left: 50%;
            transform: translateX(-50%);
            border-radius: 2px;
        }


    .data-cards {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
        /* 调整卡片最小宽度 */
        gap: 25px;
        /* 卡片间距 */
    }

    .data-card {
        background-color: #ffffff;
        padding: 25px;
        /* 卡片内边距 */
        border-radius: 12px;
        /* 卡片圆角 */
        text-align: center;
        border: 1px solid #e2e8f0;
        /* 更淡的边框 */
        transition: transform 0.25s ease, box-shadow 0.25s ease;
        box-shadow: 0 5px 15px rgba(0, 0, 0, 0.05);
        /* 初始阴影 */
    }

        .data-card:hover {
            transform: translateY(-5px);
            /* 悬停时上移更多 */
            box-shadow: 0 8px 25px rgba(0, 0, 0, 0.08);
            /* 悬停时阴影更明显 */
        }

    .card-title {
        font-size: 1.25em;
        /* 标题字体大小 */
        color: #4a5568;
        margin-bottom: 12px;
        /* 调整间距 */
        font-weight: 600;
    }

    .card-value {
        font-size: 1.8em;
        /* 默认值大小 */
        font-weight: 700;
        margin-bottom: 10px;
        line-height: 1.2;
    }

        .card-value.small-angle {
            font-size: 2.5em;
            /* 角度值更大 */
            color: #4299e1;
            /* 蓝色系 */
        }

        .card-value.big-status {
            font-size: 1.6em;
            /* 状态文字大小适中 */
            text-transform: uppercase;
            /* 状态文字大写 */
            letter-spacing: 0.5px;
        }

    .card-state {
        font-size: 1.2em;
        /* 状态文字大小 */
        font-weight: 600;
        margin-bottom: 8px;
        color: #2d3748;
        /* 深色状态文字 */
    }

    .card-interpretation {
        font-size: 0.95em;
        /* 解读文字大小 */
        color: #718096;
        /* 中灰色解读文字 */
        min-height: 3em;
        /* 确保空间 */
        line-height: 1.5;
    }

    /* 状态颜色应用到 .card-state 和 .card-value.big-status */
    .data-card.status-unknown .card-state,
    .data-card.status-unknown .card-value.big-status {
        color: #a0aec0;
    }

    .data-card.status-unknown {
        border-left: 6px solid #a0aec0;
    }

    .data-card.status-ok .card-state,
    .data-card.status-ok .card-value.big-status {
        color: #48bb78;
    }

    .data-card.status-ok {
        border-left: 6px solid #48bb78;
    }

    .data-card.status-notice .card-state,
    .data-card.status-notice .card-value.big-status {
        color: #ed8936;
    }

    /* 调整为更明显的橙色 */
    .data-card.status-notice {
        border-left: 6px solid #ed8936;
    }

    .data-card.status-warning .card-state,
    .data-card.status-warning .card-value.big-status {
        color: #f56565;
    }

    .data-card.status-warning {
        border-left: 6px solid #f56565;
    }


    .no-data-placeholder,
    .info-text {
        text-align: center;
        color: #718096;
        font-style: italic;
        margin-top: 25px;
        padding: 18px;
        background-color: #edf2f7;
        /* 更淡的背景 */
        border-radius: 8px;
        font-size: 1em;
    }

    .navigation-footer {
        text-align: center;
        margin-top: 40px;
    }

    .back-button {
        display: inline-block;
        padding: 12px 28px;
        background-color: #718096;
        /* 中灰色返回按钮 */
        color: white;
        text-decoration: none;
        border-radius: 25px;
        /* 圆角 */
        font-weight: 600;
        font-size: 1em;
        transition: background-color 0.25s ease, transform 0.25s ease;
    }

        .back-button:hover {
            background-color: #4a5568;
            /* 悬停时加深 */
            transform: translateY(-2px);
        }

    /* 响应式调整 */
    @media (max-width: 768px) {
        .page-container.assessment-page {
            padding: 20px;
        }

        .page-title {
            font-size: 2em;
        }

        .controls {
            flex-direction: column;
            /* 小屏幕按钮垂直排列 */
            gap: 15px;
        }

        .action-button {
            width: 100%;
            /* 按钮占满宽度 */
            max-width: 300px;
            /* 但有个最大宽度 */
            margin-left: auto;
            margin-right: auto;
        }

        .data-cards {
            grid-template-columns: 1fr;
            /* 小屏幕卡片单列 */
        }

        .section-title {
            font-size: 1.5em;
        }
    }

    /* 新增弹窗样式 */
    .posture-warning-dialog {
        position: fixed;
        top: 20px;
        right: 20px;
        background: #ffebee;
        border-left: 4px solid #f44336;
        padding: 16px;
        z-index: 1000;
    }

    /* 深度选择器示例（如需修改子组件样式） */
    ::v-deep .some-child-element {
        margin: 0;
    }

    /* Add these new styles for warning dialogs */
    .warning-dialog-overlay {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background-color: rgba(0, 0, 0, 0.5);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 1000;
    }

    .warning-dialog {
        background-color: white;
        border-radius: 12px;
        padding: 25px;
        max-width: 500px;
        width: 90%;
        box-shadow: 0 5px 20px rgba(0, 0, 0, 0.2);
    }

        .warning-dialog h3 {
            color: #d32f2f;
            margin-top: 0;
            text-align: center;
        }

    .warning-content {
        margin-bottom: 20px;
    }

        .warning-content p {
            margin: 10px 0;
        }

    .suggestion-title {
        font-weight: bold;
        margin-top: 15px;
    }

    .suggestion-steps {
        padding-left: 20px;
    }

        .suggestion-steps li {
            margin-bottom: 8px;
        }

    .confirm-button {
        background-color: #4285f4;
        color: white;
        border: none;
        padding: 10px 20px;
        border-radius: 5px;
        cursor: pointer;
        font-size: 1em;
        display: block;
        margin: 20px auto 0;
        transition: background-color 0.3s;
    }

        .confirm-button:hover {
            background-color: #3367d6;
        }

    .duration-display {
        font-size: 0.9em;
        color: #666;
        margin: 5px 0;
    }
</style>