# 姿态检测系统（PostureChecker）

## 项目概述

PostureChecker是一个实时姿态检测系统，旨在监测用户的坐姿，预防不良姿势导致的健康问题。系统通过摄像头捕捉用户的姿态，分析肩部倾斜、眼睛水平、驼背状态和头部倾斜等指标，为用户提供实时反馈和改进建议。此外，系统还提供后台监测弹窗提醒、历史数据查看等功能。

## 系统架构

系统采用模块化设计，主要包含以下组件：

```mermaid
graph TD
    subgraph "摄像头模块"
        PoseTcpClient["姿态TCP客户端"]
        PythonScripts["Python MediaPipe脚本"]
    end
    
    subgraph "分析模块"
        Posenalyzer["姿态分析器"]
        AnalysisResult["分析结果"]
    end
    
    subgraph "存储层"
        MySQL["MySQL数据库"]
        DataBaseClass["数据库类"]
    end
    
    subgraph "用户界面"
        WinForms["Windows窗体UI"]
        WebUI["Web应用"]
    end
    
    PythonScripts <-->|"TCP Socket通信"| PoseTcpClient
    PoseTcpClient -->|"姿态数据"| Posenalyzer
    Posenalyzer -->|"创建"| AnalysisResult
    AnalysisResult -->|"存储"| DataBaseClass
    DataBaseClass -->|"持久化"| MySQL
    AnalysisResult -->|"显示"| WinForms
    MySQL <-->|"数据访问"| WebUI
``` [1](#0-0) 

## 主要功能

### 1. 实时姿态检测

- 通过摄像头实时捕捉用户姿态
- 使用MediaPipe进行人体关键点识别
- TCP Socket通信传输姿态数据 [2](#0-1) 

### 2. 姿态分析

系统分析多种姿态指标：

- 肩部倾斜度分析
- 眼睛水平度检测
- 驼背状态评估
- 头部倾斜角度测量
- 头部方向（水平和垂直）判断
- 综合姿态评估 [3](#0-2) 

### 3. 数据存储与历史记录

- MySQL数据库存储姿态分析结果
- 支持历史数据查询和统计
- 提供姿态问题趋势分析 [4](#0-3) 

### 4. Web界面

- 实时姿态评估页面
- 历史数据查看和分析
- 基于SignalR的实时数据推送 [5](#0-4) 

### 5. AI驱动的姿态建议

- 集成大语言模型(LLM)分析姿态数据
- 提供个性化的姿态改进建议
- 基于历史数据的长期姿态趋势分析 [6](#0-5) 

## 技术栈

- **后端**：C# (.NET 8.0)
- **前端**：Vue.js
- **姿态检测**：Python + MediaPipe
- **数据库**：MySQL
- **实时通信**：SignalR
- **AI集成**：阿里云Qwen大语言模型 [7](#0-6) 

## 安装与配置

### 系统要求

- Windows操作系统
- .NET 8.0 SDK
- Python 3.8+（含MediaPipe库）
- MySQL数据库
- 网络摄像头

### 数据库配置

1. 创建MySQL数据库
2. 修改`WebAccess/appsettings.json`中的数据库连接字符串 [8](#0-7) 


## 使用指南

### 实时姿态评估

1. 打开"实时体态评估"页面
2. 点击"开始评估"按钮
3. 系统将实时显示您的姿态状态
4. 根据反馈调整坐姿 [9](#0-8) 

### AI姿态分析

1. 打开"AI分析"页面
2. 输入您想了解的姿态问题
3. 系统将基于您的历史数据提供个性化建议 [10](#0-9) 

## 项目结构

- **PostureChecker**：主应用程序和Windows UI
- **Camera**：摄像头集成模块
- **Analysis**：姿态检测算法
- **Common**：共享常量和工具类
- **WebAccess**：Web界面和API
- **front**：Vue.js前端应用
