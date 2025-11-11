using Grpc.Core;
using Grpc.Net.Client;
using IFaCareTestService;
using ProtoBuf.Grpc.Client;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;

namespace Test.TestZones;

public class FaCareListenSignalTest
{
    
    public static async Task RunClientAsync()
    {
        // Địa chỉ của Hub server và gRPC service
        const string hubUrl = "http://localhost:40003/FaCareTSgnRPushing";
        const string grpcUrl = "http://localhost:40003";

        // 1. Tạo một kết nối tới Hub
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect() // Tự động kết nối lại nếu mất kết nối
            .Build();
        
        // 2. Định nghĩa hành động khi nhận được tin nhắn từ Hub
        // Tên "ReceiveIdStream" phải khớp với tên được gọi từ server (FaCareTService)
        connection.On<JsonElement>("ReceiveIdStream", (ids) =>
        {
            // Dữ liệu nhận về có thể là một đối tượng phức tạp, dùng JsonSerializer để xem
            Console.WriteLine($"[DU LIEU MOI] Nhan duoc chunk du lieu: {JsonSerializer.Serialize(ids)}");
        });

        try
        {
            // 3. Bắt đầu kết nối
            await connection.StartAsync();
            Console.WriteLine("Client da ket noi toi Hub thanh cong!");
            Console.WriteLine($"Connection ID: {connection.ConnectionId}");

            // 4. Tạo gRPC client và gọi service
            Console.WriteLine("Dang chuan bi goi gRPC service...");
            using var channel = GrpcChannel.ForAddress(grpcUrl);
            var grpcClient = channel.CreateGrpcService<IFaCareTService>();

            // 5. Tạo metadata và thêm ConnectionId vào header
            var headers = new Metadata
            {
                { "X-Connection-Id", connection.ConnectionId }
            };

            // 6. Gọi phương thức gRPC với metadata đã tạo
            var response = await grpcClient.TestCallSgnR(new CallOptions(headers));

            Console.WriteLine($"Goi gRPC thanh cong: {response.Status}. Server bat dau qua trinh streaming...");
            Console.WriteLine("Dang lang nghe du lieu tu server... Nhan phim bat ky de thoat.");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Loi khi ket noi toi Hub: {ex.Message}");
            Console.WriteLine("=> Vui long dam bao project 'FaCareTestService' dang chay!");
        }
    }
}