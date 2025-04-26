import cv2
import mediapipe as mp
import time
import socket
import json
import struct  # 用于打包和解包长度前缀
import sys  # 用于检查 Python 版本

# --- 配置 ---
# Socket 配置
HOST = '127.0.0.1'  # 本地回环地址
PORT = 65432  # 选择一个未被占用的端口 (1024-65535 之间)
# MediaPipe 配置
MIN_DETECTION_CONFIDENCE = 0.5
MIN_TRACKING_CONFIDENCE = 0.5
# 消息长度前缀的字节数 (例如，4 bytes 表示一个32位整数)
# 使用网络字节序 (大端序) 以确保跨平台兼容性
MESSAGE_LENGTH_BYTES = 4
# --- 配置结束 ---

# 确保使用支持 struct.pack/unpack 的 Python 版本 (通常 Python 3+)
if sys.version_info < (3, 3):
    print("Error: This script requires Python 3.3 or later for struct packing.")
    sys.exit(1)


# 函数：将关键点数据打包成 JSON 字符串
def package_pose_data(landmarks):
    """
    将 MediaPipe landmarks 对象转换为列表，并序列化为 JSON 字符串。
    MediaPipe landmarks 是归一化坐标 [0, 1]。
    """
    keypoints_list = []
    if landmarks:
        for landmark in landmarks:
            # 提取关键点信息
            keypoints_list.append({
                'x': landmark.x,
                'y': landmark.y,
                'z': landmark.z,
                'visibility': landmark.visibility
            })

    # 打包成一个字典，包含关键点列表
    data = {'keypoints': keypoints_list}

    # 转换为 JSON 字符串
    json_string = json.dumps(data)
    return json_string


# 函数：发送消息（带长度前缀）
def send_message(sock, message_string):
    """发送一个字符串消息，前缀为消息长度（4字节）。"""
    try:
        message_bytes = message_string.encode('utf-8')
        message_length = len(message_bytes)

        # 打包消息长度为4字节的大端序整数
        # '>I' 表示大端序 (>) 无符号整数 (I, 4 bytes)
        length_prefix = struct.pack('>I', message_length)

        # 发送长度前缀
        sock.sendall(length_prefix)
        # 发送实际消息数据
        sock.sendall(message_bytes)
        # print(f"Sent message of length: {message_length}") # Debugging

    except (BrokenPipeError, ConnectionResetError):
        print("Client disconnected during send.")
        return False
    except Exception as e:
        print(f"Error sending message: {e}")
        return False
    return True


# --- 主程序 ---
def main():
    # 初始化 MediaPipe 姿态检测模块
    mp_pose = mp.solutions.pose
    pose = mp_pose.Pose(
        min_detection_confidence=MIN_DETECTION_CONFIDENCE,
        min_tracking_confidence=MIN_TRACKING_CONFIDENCE)

    # 打开摄像头
    cap = cv2.VideoCapture(0)
    if not cap.isOpened():
        print("错误：无法打开摄像头！请检查摄像头是否连接或是否被其他程序占用。")
        pose.close()
        sys.exit(1)

    # 设置 Socket 服务器
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    # 允许端口重用 (在某些情况下可以帮助快速重启服务器)
    server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

    try:
        server_socket.bind((HOST, PORT))
        server_socket.listen(1)  # 监听一个连接
        print(f"Python server listening on {HOST}:{PORT}")

    except socket.error as e:
        print(f"Socket binding error: {e}")
        pose.close()
        cap.release()
        server_socket.close()
        sys.exit(1)

    conn = None
    addr = None

    try:
        print("Waiting for C# client connection...")
        conn, addr = server_socket.accept()  # 等待 C# 客户端连接 (阻塞)
        print(f"Connected by {addr}")

        # 可选：用于计算帧率
        # pTime = 0

        while cap.isOpened():
            success, image = cap.read()
            if not success:
                print("警告：无法读取帧（可能视频流结束）。")
                break  # 退出循环

            # 将 BGR 图像转换为 RGB
            image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

            # 为了提高性能，将图像标记为不可写，以便按引用传递
            image_rgb.flags.writeable = False

            # 对图像进行姿态检测
            results = pose.process(image_rgb)

            # 将图像标记为可写（如果之后需要修改，虽然本例不需要）
            # image_rgb.flags.writeable = True

            # 将检测结果打包成 JSON 字符串
            json_data_string = package_pose_data(results.pose_landmarks.landmark if results.pose_landmarks else None)

            # 通过 Socket 发送数据
            if not send_message(conn, json_data_string):
                # 如果发送失败 (客户端断开)，尝试重新接受连接
                print("Attempting to reconnect after client disconnect.")
                conn.close()  # 关闭旧连接
                conn = None  # 清空连接
                # 等待新的连接 (可以加一个小的延时避免CPU空转)
                print("Waiting for new C# client connection...")
                conn, addr = server_socket.accept()
                print(f"Reconnected by {addr}")

            #可选：在 Python 端显示视频 (可能会影响性能和帧率)
            image = cv2.cvtColor(image_rgb, cv2.COLOR_RGB2BGR) # 如果之前设置了 writeable = False
            if results.pose_landmarks:
                mp_drawing = mp.solutions.drawing_utils
                mp_drawing.draw_landmarks(
                    image, results.pose_landmarks, mp_pose.POSE_CONNECTIONS)
            cv2.imshow('Python Pose (Sending)', image)
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break # 退出循环


    except Exception as e:
        print(f"An error occurred in the main loop: {e}")

    finally:
        # 清理资源
        print("Closing resources...")
        if conn:
            conn.close()
            print("Client connection closed.")
        if server_socket:
            server_socket.close()
            print("Server socket closed.")
        if cap.isOpened():
            cap.release()
            print("Camera released.")
        pose.close()  # 释放 MediaPipe 姿态对象
        cv2.destroyAllWindows()
        print("Program finished.")


if __name__ == "__main__":
    main()