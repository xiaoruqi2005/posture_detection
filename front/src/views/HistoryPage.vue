<template>
  <div class="history-page">
    <header class="page-header">
      <h1>体态评估历史记录与分析</h1>
    </header>

    <!-- 时间筛选器 -->
    <div class="time-filter-controls">
      <button
        v-for="range in timeRanges"
        :key="range.value"
        @click="selectTimeRange(range.value)"
        :class="{ active: selectedTimeRange === range.value }"
        class="btn btn-filter"
      >
        {{ range.label }}
      </button>
    </div>

    <div v-if="isLoading" class="loading-message">
      <p>正在加载历史数据... ⏳</p>
    </div>
    <div v-else-if="error" class="error-message">
      <p>加载数据失败: {{ error }} 😭</p>
      <button @click="fetchData" class="btn">重试</button>
    </div>
    <div v-else-if="!allHistoryData || allHistoryData.length === 0" class="empty-message">
      <p>暂无历史数据可供分析。🤔</p>
    </div>

    <div v-show="allHistoryData && allHistoryData.length > 0">
      <!-- 折线图区域 -->
      <section class="chart-section">
        <h2>数据趋势</h2>
        <div v-if="filteredHistoryDataForLineChart.length === 0" class="empty-message small">
            <p>当前筛选条件下无趋势数据。</p>
        </div>
        <div ref="postureLineChartContainer" class="chart-instance line-chart-instance"></div>
      </section>

      <!-- 饼状图区域 -->
      <section class="chart-section">
        <h2>驼背状态分布 ({{ timeRanges.find(r => r.value === selectedTimeRange)?.label }})</h2>
        <div v-if="pieChartData.length === 0" class="empty-message small">
          <p>当前筛选条件下无驼背状态数据可供统计。</p>
        </div>
        <div ref="hunchbackPieChartContainer" class="chart-instance pie-chart-instance"></div>
      </section>
    </div>


    <div class="actions">
      <router-link to="/" class="btn btn-secondary">返回首页</router-link>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, nextTick, computed, watch } from 'vue';
import * as echarts from 'echarts/core';
import { LineChart, PieChart } from 'echarts/charts'; // 引入饼状图
import {
  TitleComponent,
  TooltipComponent,
  GridComponent,
  LegendComponent,
  DataZoomComponent,
  ToolboxComponent, // 工具箱，可以用于导出图片等
} from 'echarts/components';
import { CanvasRenderer } from 'echarts/renderers';
import { postureApi } from '../services/apiService';
import { useRouter } from 'vue-router';

echarts.use([
  TitleComponent,
  TooltipComponent,
  GridComponent,
  LegendComponent,
  DataZoomComponent,
  ToolboxComponent,
  LineChart,
  PieChart, // 注册饼状图
  CanvasRenderer,
]);

const router = useRouter();
// 图表容器引用
const postureLineChartContainer = ref(null);
const hunchbackPieChartContainer = ref(null);
// 图表实例
let postureLineChartInstance = null;
let hunchbackPieChartInstance = null;

// 数据状态
const allHistoryData = ref([]); // 存储从API获取的所有原始数据
const isLoading = ref(true);
const error = ref(null);
const DATA_LIMIT_FOR_FETCH = 2000; // 获取足够的数据进行周和日分析，或者根据API调整

// 时间筛选
const timeRanges = [
  { label: '今天', value: 'today' },
  { label: '本周', value: 'thisWeek' },
  { label: '全部记录', value: 'allTime' },
];
const selectedTimeRange = ref('today'); // 默认筛选今天

// 将后端枚举字符串映射为用户可读的中文名称 (用于饼图图例和标签)
const mapBackendStateToDisplayName = (backendState) => {
  switch (backendState) {
    case 'NoHunchback': return '未检测到驼背';
    case 'SlightHunchback': return '轻微驼背';
    case 'ObviousHunchback': return '明显驼背';
    case 'Unknown': return '无法判断';
    default: return backendState || '其他状态';
  }
};

// 将后端枚举字符串映射为折线图内部使用的数值
const hunchbackStateToNumberForLine = (backendStateString) => {
  switch (backendStateString) {
    case 'NoHunchback': return 0;
    case 'SlightHunchback': return 1;
    case 'ObviousHunchback': return 2;
    case 'Unknown':
    default: return null;
  }
};

// 将数值映射回折线图Y轴和提示框的中文文本
const numberToHunchbackStateTextForLine = (value) => {
  switch (value) {
    case 0: return '未检测到驼背';
    case 1: return '轻微驼背';
    case 2: return '明显驼背';
    default: return '无法判断';
  }
};

// 计算属性：根据 selectedTimeRange 筛选数据
const filteredHistoryData = computed(() => {
  if (!allHistoryData.value || allHistoryData.value.length === 0) {
    return [];
  }
  const now = new Date();
  const todayStart = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  const todayEnd = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59, 59, 999);

  // 获取本周的开始（周一）和结束（周日）
  const currentDayOfWeek = now.getDay(); // 0 (Sunday) - 6 (Saturday)
  const daysToMonday = currentDayOfWeek === 0 ? -6 : 1 - currentDayOfWeek; // 如果是周日，则减6天；否则是1减去当前星期几
  const weekStart = new Date(now.getFullYear(), now.getMonth(), now.getDate() + daysToMonday);
  weekStart.setHours(0, 0, 0, 0);
  const weekEnd = new Date(weekStart);
  weekEnd.setDate(weekStart.getDate() + 6);
  weekEnd.setHours(23, 59, 59, 999);

  switch (selectedTimeRange.value) {
    case 'today':
      return allHistoryData.value.filter(item => {
        const itemDate = new Date(item.timestamp);
        return itemDate >= todayStart && itemDate <= todayEnd;
      });
    case 'thisWeek':
      return allHistoryData.value.filter(item => {
        const itemDate = new Date(item.timestamp);
        return itemDate >= weekStart && itemDate <= weekEnd;
      });
    case 'allTime':
    default:
      return allHistoryData.value;
  }
});

// 为折线图准备的数据 (可能与饼图的筛选结果一致，但保留独立性以防未来需求不同)
const filteredHistoryDataForLineChart = computed(() => {
    // 当前设计中，折线图也使用相同的时间筛选结果
    return filteredHistoryData.value;
});


// 计算属性：为饼状图准备数据
const pieChartData = computed(() => {
  if (!filteredHistoryData.value || filteredHistoryData.value.length === 0) {
    return [];
  }
  const counts = {};
  filteredHistoryData.value.forEach(item => {
    const state = item.hunchbackState || 'Unknown'; // 如果hunchbackState为null或undefined，归为Unknown
    counts[state] = (counts[state] || 0) + 1;
  });

  return Object.entries(counts).map(([backendState, count]) => ({
    value: count,
    name: mapBackendStateToDisplayName(backendState), // 使用映射函数获取中文名
  }));
});


// 获取数据
const fetchData = async () => {
  isLoading.value = true;
  error.value = null;
  try {
    // 获取足够多的数据以支持客户端筛选，或者根据API能力调整
    const response = await postureApi.getHistory(DATA_LIMIT_FOR_FETCH);
    allHistoryData.value = response.data
      .map(item => ({
        ...item,
        timestamp: new Date(item.timestamp).getTime(), // 原始时间戳，用于筛选
      }))
      .sort((a, b) => a.timestamp - b.timestamp); // 排序方便折线图

    // 数据获取后，立即更新图表
    await nextTick();
    updateCharts();

  } catch (err) {
    console.error('获取历史数据失败:', err);
    error.value = err.message || '未知错误';
  } finally {
    isLoading.value = false;
  }
};

function selectTimeRange(range) {
  selectedTimeRange.value = range;
  // selectedTimeRange 变化时，computed属性会自动更新，watch会触发图表更新
}

// 监听筛选后的数据变化，并更新图表
watch([filteredHistoryDataForLineChart, pieChartData], () => {
    nextTick(() => { // 确保DOM更新后再操作图表
        updateCharts();
    });
}, { deep: true });


function updateCharts() {
    initOrUpdateLineChart();
    initOrUpdatePieChart();
}

// 初始化或更新折线图
const initOrUpdateLineChart = () => {
  if (!postureLineChartContainer.value) return;
  if (!postureLineChartInstance) {
    postureLineChartInstance = echarts.init(postureLineChartContainer.value);
  } else {
    postureLineChartInstance.clear(); // 清除旧数据，避免tooltip等问题
  }
  if(filteredHistoryDataForLineChart.value.length === 0) {
      // 如果没有数据，可以显示空状态或不渲染图表
      // ECharts 在没有 series 数据时会显示一个空的 grid
      postureLineChartInstance.setOption({
          title: { show: false }, // 隐藏标题以避免显示在空图表上
          grid: { show: false },
          xAxis: { show: false },
          yAxis: { show: false },
      });
      return;
  }


  const timestamps = filteredHistoryDataForLineChart.value.map(d => new Date(d.timestamp).toLocaleString());
  const headTiltAngles = filteredHistoryDataForLineChart.value.map(d => d.headTiltAngle?.toFixed(1) ?? null);
  const shoulderTiltAngles = filteredHistoryDataForLineChart.value.map(d => d.shoulderTiltAngle?.toFixed(1) ?? null);
  const hunchbackStatesLine = filteredHistoryDataForLineChart.value.map(d => hunchbackStateToNumberForLine(d.hunchbackState));

  const lineOption = {
    title: {
      text: '体态数据趋势',
      left: 'center',
      show: filteredHistoryDataForLineChart.value.length > 0, // 仅当有数据时显示
    },
    tooltip: { trigger: 'axis', /* formatter... */ },
    legend: { data: ['头部倾斜角度', '双肩倾斜角度', '驼背状态'], top: 'bottom' },
    grid: { left: '3%', right: '4%', bottom: '15%', containLabel: true },
    xAxis: { type: 'category', boundaryGap: false, data: timestamps },
    yAxis: [
      { type: 'value', name: '角度 (°)', /* ... */ },
      { type: 'value', name: '驼背状态', min: 0, max: 2, interval: 1, axisLabel: { formatter: (v) => numberToHunchbackStateTextForLine(v) }, /* ... */ }
    ],
    series: [
      { name: '头部倾斜角度', type: 'line', smooth: true, data: headTiltAngles, yAxisIndex: 0, connectNulls: true },
      { name: '双肩倾斜角度', type: 'line', smooth: true, data: shoulderTiltAngles, yAxisIndex: 0, connectNulls: true },
      { name: '驼背状态', type: 'line', step: 'start', data: hunchbackStatesLine, yAxisIndex: 1, connectNulls: true },
    ],
    dataZoom: [{ type: 'inside' }, { type: 'slider', bottom: '5%' }],
    toolbox: {
        feature: {
            saveAsImage: { title: '保存为图片' }
        }
    }
  };
  postureLineChartInstance.setOption(lineOption, true); // true for notMerge
};

// 初始化或更新饼状图
const initOrUpdatePieChart = () => {
  if (!hunchbackPieChartContainer.value) return;
  if (!hunchbackPieChartInstance) {
    hunchbackPieChartInstance = echarts.init(hunchbackPieChartContainer.value);
  } else {
      hunchbackPieChartInstance.clear(); // 清除旧数据
  }

  if (pieChartData.value.length === 0) {
    hunchbackPieChartInstance.setOption({
        title: { show: false },
    }); // 清空图表或显示无数据提示
    return;
  }

  const pieOption = {
    title: {
      // text: `驼背状态分布 (${selectedTimeRange.value})`, // 标题已移到section的h2
      // subtext: '数据占比',
      left: 'center',
      show: pieChartData.value.length > 0,
    },
    tooltip: {
      trigger: 'item',
      formatter: '{a} <br/>{b} : {c} ({d}%)' // a: series name, b: data name, c: value, d: percentage
    },
    legend: {
      orient: 'vertical',
      left: 'left',
      data: pieChartData.value.map(item => item.name) // 从数据中动态获取图例项
    },
    series: [
      {
        name: '驼背状态',
        type: 'pie',
        radius: '65%', // 饼图半径
        center: ['50%', '55%'], // 饼图中心位置
        data: pieChartData.value,
        emphasis: {
          itemStyle: {
            shadowBlur: 10,
            shadowOffsetX: 0,
            shadowColor: 'rgba(0, 0, 0, 0.5)'
          }
        },
        label: {
          show: true,
          formatter: '{b}: {d}%' // 在饼图上显示名称和百分比
        }
      }
    ],
    toolbox: {
        feature: {
            saveAsImage: { title: '保存为图片' }
        }
    }
  };
  hunchbackPieChartInstance.setOption(pieOption, true); // true for notMerge
};

const handleResize = () => {
  if (postureLineChartInstance) postureLineChartInstance.resize();
  if (hunchbackPieChartInstance) hunchbackPieChartInstance.resize();
};

onMounted(() => {
  fetchData(); // 获取初始数据
  window.addEventListener('resize', handleResize);
});

onUnmounted(() => {
  window.removeEventListener('resize', handleResize);
  if (postureLineChartInstance) {
    postureLineChartInstance.dispose();
    postureLineChartInstance = null;
  }
  if (hunchbackPieChartInstance) {
    hunchbackPieChartInstance.dispose();
    hunchbackPieChartInstance = null;
  }
});

</script>

<style scoped>
.history-page {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
  font-family: sans-serif;
}
.page-header {
  text-align: center;
  margin-bottom: 20px;
}
.page-header h1 {
  color: #2c3e50;
  font-size: 2.2em;
}

.time-filter-controls {
  text-align: center;
  margin-bottom: 30px;
}
.btn-filter {
  margin: 0 8px;
  padding: 8px 15px;
  border: 1px solid #ccc;
  background-color: #f9f9f9;
  cursor: pointer;
  border-radius: 4px;
  transition: background-color 0.2s, color 0.2s, border-color 0.2s;
}
.btn-filter:hover {
  background-color: #e0e0e0;
}
.btn-filter.active {
  background-color: #3498db;
  color: white;
  border-color: #3498db;
}

.loading-message,
.error-message,
.empty-message {
  text-align: center;
  padding: 30px 20px;
  background-color: #fdfdfd;
  border: 1px dashed #eee;
  border-radius: 8px;
  margin: 20px 0;
  color: #777;
}
.empty-message.small {
    padding: 15px;
    margin-top: 10px;
    font-size: 0.9em;
}

.error-message p {
  color: #e74c3c;
}

.chart-section {
  margin-bottom: 40px;
  padding: 20px;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  background-color: #fff;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
}
.chart-section h2 {
  text-align: center;
  margin-bottom: 20px;
  color: #333;
  font-size: 1.5em;
}

.chart-instance {
  width: 100%;
  height: 450px; /* 统一图表高度 */
}
/* 可以为不同图表设置不同高度（如果需要） */
/* .line-chart-instance { height: 450px; } */
/* .pie-chart-instance { height: 400px; } */


.actions {
  text-align: center;
  margin-top: 30px;
}
.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  font-size: 1em;
  transition: background-color 0.3s ease;
  text-decoration: none;
  display: inline-block;
}
.btn-secondary {
  background-color: #7f8c8d;
  color: white;
}
.btn-secondary:hover {
  background-color: #606f70;
}
.error-message .btn {
  background-color: #e74c3c;
  color: white;
}
</style>