FROM postgres:latest

# Cài đặt gói timezone data
RUN apt-get update && apt-get install -y tzdata && rm -rf /var/lib/apt/lists/*

# Đặt timezone mặc định
ENV TZ=Asia/Ho_Chi_Minh
