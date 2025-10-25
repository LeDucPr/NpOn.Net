using Microsoft.AspNetCore.SignalR.Client;

namespace Test.TestZones;

public class SignalRTest
{
    public static async Task RunClientAsync()
    {
        // Địa chỉ của Hub server mà chúng ta đã cấu hình ở project SignalRExtCm
        const string hubUrl = "http://localhost:7000/chathub";

        // 1. Tạo một kết nối tới Hub
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect() // Tự động kết nối lại nếu mất kết nối
            .Build();

        Console.WriteLine("Dang chuan bi ket noi toi SignalR Hub...");

        // 2. Định nghĩa hành động khi nhận được tin nhắn từ Hub
        // Tên "ReceiveMessage" phải khớp với tên được gọi từ server
        connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            Console.WriteLine($"[TIN NHAN MOI] {user}: {message}");
        });

        try
        {
            // 3. Bắt đầu kết nối
            await connection.StartAsync();
            Console.WriteLine("Client da ket noi toi Hub thanh cong!");
            Console.WriteLine("Dang lang nghe tin nhan tu server...");
            Console.WriteLine("Nhan phim bat ky de thoat.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Loi khi ket noi toi Hub: {ex.Message}");
            Console.WriteLine("=> Vui long dam bao project 'SignalRExtCm' dang chay!");
        }
    }
}