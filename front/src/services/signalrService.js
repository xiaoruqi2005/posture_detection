import * as signalR from "@microsoft/signalr";

const postureHubUrl = "/postureHub"; // Vite 会代理这个路径
let connection = null;
let onDataReceivedCallback = null;
let onConnectionStatusChangeCallback = null;

export function createSignalRConnection() {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        console.log("SignalR already connected.");
        if (onConnectionStatusChangeCallback) onConnectionStatusChangeCallback(true, "已连接");
        return connection;
    }

    connection = new signalR.HubConnectionBuilder()
        .withUrl(postureHubUrl)
        .withAutomaticReconnect([0, 2000, 10000, 30000]) // 尝试重连的时间间隔
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.on("ReceivePostureData", (data) => {
        if (onDataReceivedCallback) {
            onDataReceivedCallback(data);
        }
    });

    connection.onreconnecting(error => {
        console.warn(`SignalR connection lost. Attempting to reconnect: ${error}`);
        if (onConnectionStatusChangeCallback) onConnectionStatusChangeCallback(false, "正在尝试重新连接...");
    });

    connection.onreconnected(connectionId => {
        console.log(`SignalR reconnected with ID: ${connectionId}`);
        if (onConnectionStatusChangeCallback) onConnectionStatusChangeCallback(true, "已重新连接");
    });

    connection.onclose(error => {
        console.error(`SignalR connection closed: ${error}`);
        if (onConnectionStatusChangeCallback) onConnectionStatusChangeCallback(false, "连接已断开");
        // 可选：尝试在一段时间后再次启动连接
        // setTimeout(() => startSignalRConnection(), 5000);
    });

    return connection;
}

export async function startSignalRConnection() {
    if (!connection) {
        createSignalRConnection();
    }
    if (connection.state === signalR.HubConnectionState.Disconnected) {
        try {
            await connection.start();
            console.log("SignalR Connected.");
            if (onConnectionStatusChangeCallback) onConnectionStatusChangeCallback(true, "已连接");
        } catch (err) {
            console.error("SignalR Connection Error: ", err);
            if (onConnectionStatusChangeCallback) onConnectionStatusChangeCallback(false, `连接失败: ${err.message}`);
        }
    } else {
        console.log("SignalR connection already in progress or connected.");
        if (onConnectionStatusChangeCallback) onConnectionStatusChangeCallback(connection.state === signalR.HubConnectionState.Connected, "状态: " + connection.state);
    }
}

export async function stopSignalRConnection() {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        try {
            await connection.stop();
            console.log("SignalR Disconnected.");
            if (onConnectionStatusChangeCallback) onConnectionStatusChangeCallback(false, "连接已手动断开");
        } catch (err) {
            console.error("SignalR Disconnection Error: ", err);
        }
    }
}

export function registerDataHandler(callback) {
    onDataReceivedCallback = callback;
}

export function registerConnectionStatusHandler(callback) {
    onConnectionStatusChangeCallback = callback;
}

// 确保在应用卸载或不需要时清理
export function cleanupSignalR() {
    if (connection) {
        connection.off("ReceivePostureData");
        connection.off("onreconnecting");
        connection.off("onreconnected");
        connection.off("onclose");
        if (connection.state === signalR.HubConnectionState.Connected) {
            connection.stop();
        }
        connection = null;
    }
    onDataReceivedCallback = null;
    onConnectionStatusChangeCallback = null;
}