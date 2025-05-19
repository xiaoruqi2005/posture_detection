// src/signalr-connection.js
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:YOUR_BACKEND_PORT/postureHub") // 替换为你的后端地址
    .withAutomaticReconnect()
    .build();

export async function startSignalR(onDataReceivedCallback) {
    try {
        await connection.start();
        console.log("SignalR Connected.");
        connection.on("ReceivePostureData", onDataReceivedCallback);
    } catch (err) {
        console.error("SignalR Connection Error: ", err);
        setTimeout(() => startSignalR(onDataReceivedCallback), 5000); // 简单重试
    }
}

export default connection; // 如果 App.vue 需要直接访问