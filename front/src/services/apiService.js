/* import axios from 'axios';

const apiClient = axios.create({
    baseURL: '/api', // Vite 会代理这个路径
    headers: {
        'Content-Type': 'application/json',
    },
});

export const postureApi = {
    start: () => apiClient.post('/posture/start'),
    stop: () => apiClient.post('/posture/stop'),
    getHistory: (limit) => apiClient.get(`/posture/history?limit=${limit}`), // 如果需要
    queryLLM: (prompt) => apiClient.post('/posture/llm', { prompt }), // 如果需要
}; */

// --- START OF FILE apiService.js (Based on your existing and HistoryPage needs) ---

import axios from 'axios';

const apiClient = axios.create({
    baseURL: '/api', // Vite 会代理这个路径
    headers: {
        'Content-Type': 'application/json',
    },
});

export const postureApi = {
    /**
     * 启动体态评估 (假设的接口)
     * @returns {Promise<AxiosResponse<any>>}
     */
    start: () => apiClient.post('/posture/start'),

    /**
     * 停止体态评估 (假设的接口)
     * @returns {Promise<AxiosResponse<any>>}
     */
    stop: () => apiClient.post('/posture/stop'),

    /**
     * 获取历史体态数据记录
     * HistoryPage.vue 会调用这个接口获取原始数据进行分析和图表展示。
     * @param {number} [limit=2000] - 要获取的最大记录数，前端 HistoryPage.vue 使用 DATA_LIMIT_FOR_FETCH 传递
     * @returns {Promise<AxiosResponse<Array<PostureDataFromAPI>>>} 返回包含体态数据对象的数组
     */
    getHistory: (limit = 2000) => apiClient.get(`/posture/history?limit=${limit}`),

    /**
     * 与LLM模型交互 (假设的接口)
     * @param {string} prompt - 发送给LLM的提示
     * @returns {Promise<AxiosResponse<any>>}
     */
    queryLLM: (prompt) => apiClient.post('/posture/llm', { prompt }),

    // --- 如果未来饼图统计移到后端，可以在这里添加 ---
    /**
     * (未来可选) 获取驼背状态的统计数据
     * @param {'today' | 'thisWeek' | 'allTime'} [range='today'] - 时间范围
     * @returns {Promise<AxiosResponse<Array<{name: string, value: number}>>>}
     */
    // getHunchbackStats: (range = 'today') => apiClient.get(`/posture/stats/hunchback?range=${range}`),
};

/* *
 * JSDoc 类型定义，用于描述从 getHistory API 返回的数据结构。
 * 这应该与你后端 PostureData 类的JSON序列化结果一致。
 * @typedef {object} PostureDataFromAPI
 * @property {number} id - 记录的唯一标识符
 * @property {string} timestamp - ISO 8601 格式的时间戳字符串 (例如 "2023-10-27T10:30:00Z")
 * @property {number|null} shoulderTiltAngle - 肩膀倾斜角度 (如果可用)
 * @property {string} shoulderState - 肩膀状态的字符串表示 (例如 "Level", "LeftSlightlyHigh")
 * @property {string} hunchbackState - 驼背状态的字符串表示 (例如 "NoHunchback", "SlightHunchback")
 * @property {number|null} headTiltAngle - 头部倾斜角度 (如果可用)
 * @property {string} headTiltState - 头部倾斜状态的字符串表示 (例如 "Upright", "SlightlyTiltedLeft")
 * // ... 可能还有其他后端返回的字段
 */

// --- END OF FILE apiService.js ---