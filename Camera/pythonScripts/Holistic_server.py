import cv2
import mediapipe as mp
import time  # 可选，用于计算帧率等
import socket
import json
import struct  # 用于打包和解包长度前缀
import sys  # 用于检查 Python 版本

# --- 配置 ---
# Socket 配置
HOST = '127.0.0.1'  # 本地回环地址
PORT = 65432  # 选择一个未被占用的端口 (1024-65535 之间)
# MediaPipe 配置
# Holistic 模型通常有更高的计算需求，可以根据需要调整置信度
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


# 函数：将 MediaPipe LandmarkList 转换为列表
def landmark_list_to_data(landmark_list):
    """
    将 MediaPipe LandmarkList 对象转换为列表，包含 x, y, z 坐标。
    如果可用，也包含 visibility (姿态) 或 presence (面部)。
    MediaPipe landmarks 是归一化坐标 [0, 1]。
    """
    points_data = []
    if landmark_list:
        # MediaPipe landmark lists have a 'landmark' attribute which is a list
        for landmark in landmark_list.landmark:
            # 提取关键点信息
            point_info = {
                'x': landmark.x,
                'y': landmark.y,
                'z': landmark.z,
                # 使用 getattr 安全地获取 visibility 或 presence，如果属性不存在则为 None
                'visibility': getattr(landmark, 'visibility', None), # 姿态关键点有 visibility
                'presence': getattr(landmark, 'presence', None)     # 面部关键点有 presence
                # 手部关键点通常没有 visibility 或 presence 属性
            }
            # 移除 None 值以减小 JSON 大小，如果不需要的话
            # point_info = {k: v for k, v in point_info.items() if v is not None}
            points_data.append(point_info)
    return points_data


# 函数：将 Holistic 结果打包成 JSON 字符串
def package_holistic_data(results):
    """
    将 MediaPipe Holistic 结果对象转换为字典，并序列化为 JSON 字符串。
    包含 pose, face, left_hand, right_hand 关键点。
    """
    data = {}

    # 打包姿态关键点
    data['pose'] = landmark_list_to_data(results.pose_landmarks)

    # 打包面部关键点 (Face Mesh)
    data['face'] = landmark_list_to_data(results.face_landmarks)

    # 打包左手关键点
    data['left_hand'] = landmark_list_to_data(results.left_hand_landmarks)

    # 打包右手关键点
    data['right_hand'] = landmark_list_to_data(results.right_hand_landmarks)

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

#将图片编码为 JPEG并通过socket发送（带长度前缀）
def send_image(sock, image):
    """将图像编码为JPEG并通过socket发送（带长度前缀）"""
    try:
        _, buffer = cv2.imencode('.jpg', image)
        image_bytes = buffer.tobytes()
        length_prefix = struct.pack('>I', len(image_bytes))
        sock.sendall(length_prefix + image_bytes)
    except Exception as e:
        print(f"Error sending image: {e}")
        return False
    return True


# --- 主程序 ---
def main():
    # 初始化 MediaPipe Holistic 模型
    mp_holistic = mp.solutions.holistic
    # mp_drawing 用于可视化，虽然发送数据不需要，但在Python端显示时有用
    mp_drawing = mp.solutions.drawing_utils

    # 创建 Holistic 对象
    # model_complexity 参数可以在 0, 1, 2 之间选择，数字越大通常精度越高但速度越慢
    # enable_segmentation=True 如果需要人体分割掩码， False 否则
    holistic = mp_holistic.Holistic(
        min_detection_confidence=MIN_DETECTION_CONFIDENCE,
        min_tracking_confidence=MIN_TRACKING_CONFIDENCE,
        model_complexity=1, # 例如，使用模型复杂度 1
        enable_segmentation=False # 通常发送关键点不需要分割
        # smooth_landmarks=True # 可选，平滑关键点抖动
    )

    # 打开摄像头
    cap = cv2.VideoCapture(0)
    if not cap.isOpened():
        print("错误：无法打开摄像头！请检查摄像头是否连接或是否被其他程序占用。")
        holistic.close()
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
        holistic.close()
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
                # 如果摄像头断开或出现问题，退出循环
                break

            # 将 BGR 图像转换为 RGB
            image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

            # 为了提高性能，将图像标记为不可写，以便按引用传递
            image_rgb.flags.writeable = False

            # 对图像进行整体关键点检测 (Holistic)
            results = holistic.process(image_rgb)

            # 将图像标记为可写（如果之后需要修改，虽然本例不需要）
            image_rgb.flags.writeable = True # 恢复可写状态，方便绘制

            # 将检测结果打包成 JSON 字符串
            json_data_string = package_holistic_data(results)

            # 通过 Socket 发送数据
            if not send_message(conn, json_data_string):
                # 如果发送失败 (客户端断开)，尝试重新接受连接
                print("Attempting to reconnect after client disconnect.")
                if conn:
                    conn.close()  # 关闭旧连接
                    conn = None   # 清空连接
                # 等待新的连接 (可以加一个小的延时避免CPU空转)
                # time.sleep(0.5) # 可选的延时
                print("Waiting for new C# client connection...")
                try:
                    conn, addr = server_socket.accept()
                    print(f"Reconnected by {addr}")
                except socket.error as e:
                    print(f"Error accepting new connection: {e}")
                    # 如果接受新连接也失败，可能服务器需要重启或退出
                    break # 退出主循环
            if not send_image(conn,image):
                print("Failed to send image.")
            #可选：在 Python 端显示视频 (可能会影响性能和帧率)
            # 在原始图像上绘制关键点
            # image = cv2.cvtColor(image_rgb, cv2.COLOR_RGB2BGR) # 已经在上面恢复可写并可能转换回 BGR 了

            # 绘制姿态关键点和连接
            if results.pose_landmarks:
                mp_drawing.draw_landmarks(
                    image, results.pose_landmarks, mp_holistic.POSE_CONNECTIONS,
                    mp_drawing.DrawingSpec(color=(245, 117, 66), thickness=2, circle_radius=2), # 关键点颜色
                    mp_drawing.DrawingSpec(color=(245, 66, 230), thickness=2, circle_radius=2) # 连接线颜色
                )
            # 绘制面部网格关键点和连接
            if results.face_landmarks:
                 mp_drawing.draw_landmarks(
                     image, results.face_landmarks, mp_holistic.FACEMESH_TESSELATION,
                     landmark_drawing_spec=None, # 不绘制单个关键点（因为太多了）
                     connection_drawing_spec=mp_drawing.DrawingSpec(color=(0, 255, 0), thickness=1, circle_radius=1) # 绿色连接线
                 )
             # 绘制左手关键点和连接
            if results.left_hand_landmarks:
                 mp_drawing.draw_landmarks(
                     image, results.left_hand_landmarks, mp_holistic.HAND_CONNECTIONS,
                     mp_drawing.DrawingSpec(color=(121, 22, 76), thickness=2, circle_radius=4), # 关键点颜色
                     mp_drawing.DrawingSpec(color=(121, 44, 250), thickness=2, circle_radius=2) # 连接线颜色
                 )
             # 绘制右手关键点和连接
            if results.right_hand_landmarks:
                 mp_drawing.draw_landmarks(
                     image, results.right_hand_landmarks, mp_holistic.HAND_CONNECTIONS,
                     mp_drawing.DrawingSpec(color=(80, 110, 10), thickness=2, circle_radius=4), # 关键点颜色
                     mp_drawing.DrawingSpec(color=(120, 245, 110), thickness=2, circle_radius=2) # 连接线颜色
                 )


            cv2.imshow('Python Holistic (Sending)', image)
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
        if cap and cap.isOpened():
            cap.release()
            print("Camera released.")
        if holistic: # 确保 holistic 对象被创建了
           holistic.close()  # 释放 MediaPipe Holistic 对象
           print("MediaPipe Holistic released.")
        cv2.destroyAllWindows()
        print("Program finished.")


if __name__ == "__main__":
    main()