<template>
  <div class="ai-assistant-page">
    <header class="page-header">
      <h1>AI 体态分析助手</h1>
      <p>基于您最近的体态数据，向 AI 提问以获得分析和建议。</p>
    </header>

    <div class="input-section">
      <textarea
        v-model="userPrompt"
        placeholder="例如：根据我最近一周的体态数据，我的主要体态问题是什么？有什么改善建议？"
        rows="4"
        class="prompt-input"
        :disabled="isLoading"
      ></textarea>
      <button @click="requestAIAnalysis" class="btn btn-primary" :disabled="isLoading || !userPrompt.trim()">
        {{ isLoading ? '分析中...' : '获取 AI 分析' }}
      </button>
    </div>

    <div v-if="isLoading" class="loading-message">
      <p>AI 正在努力分析您的数据... 🧠</p>
    </div>

    <div v-if="error" class="error-message">
      <p>分析失败：{{ error }} 😞</p>
    </div>

    <div v-if="aiResponse" class="response-section">
      <h2>AI 分析结果：</h2>
      <pre class="ai-response-text">{{ aiResponse }}</pre>
    </div>

    <div v-if="historyForContext.length > 0 && !isLoading" class="context-section">
      <h3>用于分析的历史数据片段 (最近 {{ HISTORY_CONTEXT_LIMIT }} 条):</h3>
      <ul class="history-list">
        <li v-for="item in historyForContext" :key="item.timestamp" class="history-item">
          <strong>时间:</strong> {{ new Date(item.timestamp).toLocaleString() }} <br />
          <strong>头部倾斜:</strong> {{ item.headTiltAngle?.toFixed(1) }}° ({{ item.headTiltState }}) <br />
          <strong>双肩倾斜:</strong> {{ item.shoulderTiltAngle?.toFixed(1) }}° ({{ item.shoulderState }}) <br />
          <strong>驼背状态:</strong> {{ item.hunchbackState }}
        </li>
      </ul>
    </div>

     <div class="actions">
      <router-link to="/" class="btn btn-secondary">返回首页</router-link>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { postureApi } from '../services/apiService'; // 确保路径正确

const userPrompt = ref('');
const aiResponse = ref('');
const isLoading = ref(false);
const error = ref(null);
const historyForContext = ref([]); // 用于发送给AI的历史数据上下文

const HISTORY_CONTEXT_LIMIT = 20; // 获取最近20条数据作为上下文

// 获取最近的历史数据作为上下文
const fetchHistoryContext = async () => {
  isLoading.value = true;
  error.value = null;
  try {
    const response = await postureApi.getHistory(HISTORY_CONTEXT_LIMIT);
    // API 返回的是最新的在前面，我们需要按时间顺序（旧->新）
    // 或者，如果API已经是按时间升序的，这里就不需要 reverse
    historyForContext.value = response.data.sort((a,b) => a.timestamp - b.timestamp);
  } catch (err) {
    console.error('获取历史数据上下文失败:', err);
    error.value = '无法加载用于分析的历史数据。';
    historyForContext.value = []; // 清空，以防万一
  } finally {
    isLoading.value = false;
  }
};

const requestAIAnalysis = async () => {
  if (!userPrompt.value.trim()) {
    error.value = '请输入您的问题或分析请求。';
    return;
  }
  if (historyForContext.value.length === 0 && !error.value) {
    // 如果没有历史数据上下文，可以先尝试获取
    await fetchHistoryContext();
    if (historyForContext.value.length === 0) {
        error.value = '没有可供分析的历史数据。请先使用一段时间后再尝试分析。';
        return;
    }
  }


  isLoading.value = true;
  aiResponse.value = '';
  error.value = null;

  try {
    // 将历史数据转换为更简洁的字符串格式，或者直接发送对象数组
    // 这里示例将历史数据对象数组直接传递
    const response = await postureApi.analyzeHistoryWithLLM(userPrompt.value, historyForContext.value);
    aiResponse.value = response.data.response; // 假设后端返回 { response: "AI的回答" }
  } catch (err) {
    console.error('AI分析请求失败:', err);
    error.value = err.response?.data?.message || err.message || 'AI分析服务暂时不可用，请稍后再试。';
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  fetchHistoryContext(); // 页面加载时获取历史数据上下文
});
</script>

<style scoped>
.ai-assistant-page {
  padding: 20px;
  max-width: 800px;
  margin: 0 auto;
  font-family: sans-serif;
}

.page-header {
  text-align: center;
  margin-bottom: 30px;
}

.page-header h1 {
  color: #2c3e50;
  font-size: 2em;
}
.page-header p {
  color: #555;
  font-size: 1.1em;
}

.input-section {
  margin-bottom: 30px;
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.prompt-input {
  width: 100%;
  padding: 12px;
  border: 1px solid #ccc;
  border-radius: 6px;
  font-size: 1em;
  box-sizing: border-box;
  resize: vertical; /* 允许垂直调整大小 */
}

.btn {
  padding: 12px 24px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 1em;
  transition: background-color 0.3s ease;
  text-decoration: none;
}

.btn-primary {
  background-color: #007bff;
  color: white;
  align-self: flex-start; /* 按钮靠左对齐 */
}
.btn-primary:hover {
  background-color: #0056b3;
}
.btn-primary:disabled {
  background-color: #cccccc;
  cursor: not-allowed;
}


.btn-secondary {
  background-color: #6c757d;
  color: white;
}
.btn-secondary:hover {
  background-color: #545b62;
}

.loading-message,
.error-message {
  text-align: center;
  padding: 20px;
  margin-bottom: 20px;
  border-radius: 6px;
}

.loading-message p {
  font-size: 1.1em;
  color: #007bff;
}

.error-message {
  background-color: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
}

.response-section {
  margin-top: 20px;
  background-color: #f9f9f9;
  padding: 20px;
  border-radius: 6px;
  border: 1px solid #eee;
}

.response-section h2 {
  margin-top: 0;
  color: #333;
}

.ai-response-text {
  white-space: pre-wrap; /* 保留换行和空格 */
  word-wrap: break-word;
  font-family: 'Courier New', Courier, monospace;
  background-color: #fff;
  padding: 15px;
  border-radius: 4px;
  border: 1px solid #ddd;
  max-height: 400px; /* 可以限制最大高度并添加滚动条 */
  overflow-y: auto;
}

.context-section {
  margin-top: 30px;
  padding-top: 20px;
  border-top: 1px solid #eee;
}
.context-section h3 {
  color: #444;
  margin-bottom: 10px;
}
.history-list {
  list-style-type: none;
  padding: 0;
  max-height: 300px;
  overflow-y: auto;
  background-color: #fdfdfd;
  border: 1px solid #efefef;
  border-radius: 4px;
}
.history-item {
  padding: 10px;
  border-bottom: 1px solid #eee;
  font-size: 0.9em;
  color: #555;
}
.history-item:last-child {
  border-bottom: none;
}


.actions {
  text-align: center;
  margin-top: 30px;
}
</style>