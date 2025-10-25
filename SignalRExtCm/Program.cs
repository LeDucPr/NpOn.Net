using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Chỉ định server sẽ chạy trên port 7000
builder.WebHost.UseUrls("http://localhost:7000");

// 1. Thêm dịch vụ SignalR vào server
builder.Services.AddSignalR();

var app = builder.Build();

// 2. Map Hub của bạn tới một endpoint, ví dụ "/chathub"
app.MapHub<ChatHub>("/chathub");

Console.WriteLine("May chu SignalR dang khoi chay, nhan Ctrl+C de dung lai.");
app.Run(); // Khởi động server và giữ cho nó chạy

// Định nghĩa một Hub đơn giản
public class ChatHub : Hub
{
    // Server có thể gọi phương thức này để gửi tin nhắn tới tất cả client
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public override async Task OnConnectedAsync()
    {
        // Gửi tin nhắn chào mừng tới client vừa kết nối
        await Clients.Caller.SendAsync("ReceiveMessage", "He thong", "Chao mung ban da den voi ChatHub!");

        // Lấy HubContext để gửi tin nhắn từ server bất cứ lúc nào
        var hubContext = this.Context.GetHttpContext()?.RequestServices.GetRequiredService<IHubContext<ChatHub>>();
        
        // Mô phỏng server gửi tin nhắn định kỳ
        _ = Task.Run(async () =>
        {
            var i = 0;
            while (true)
            {
                await Task.Delay(5000); // Gửi mỗi 5 giây
                await hubContext!.Clients.All.SendAsync("ReceiveMessage", "Server Time", $"Bay gio la: {DateTime.Now}");
            }
        });

        await base.OnConnectedAsync();
    }
}