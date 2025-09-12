namespace CommonDb.Connections;

public class DbDriver<T> : IDbDriver, IAsyncDisposable where T : class
{
    private T? _connection;
    private bool _disposed = false;

    public DbDriver(T? connection)
    {
        _connection = connection;
    }

    // Triển khai từ IDbDriver
    public Task Connect()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DbDriver<T>));
        }

        // Logic kết nối thực tế sẽ ở đây
        // Ví dụ: await _connection.OpenAsync();
        Console.WriteLine("Connecting...");
        return Task.CompletedTask;
    }

    public Task<bool> DisConnect()
    {
        throw new NotImplementedException();
    }

    // Đổi tên từ DisConnect và thay đổi kiểu trả về
    public async Task Disconnect()
    {
        if (_disposed)
        {
            return; // Đã được dọn dẹp, không làm gì cả
        }

        // Logic ngắt kết nối thực tế
        // Ví dụ: await _connection.CloseAsync();
        Console.WriteLine("Disconnecting...");
        await Task.Delay(100); // Giả lập công việc bất đồng bộ
    }

    // --- Triển khai IAsyncDisposable ---

    // Đây là phương thức chính được gọi bởi `await using`
    public async ValueTask DisposeAsync()
    {
        // Gọi phương thức dọn dẹp và ngăn chặn finalizer (nếu có)
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    // Phương thức cốt lõi để thực hiện việc dọn dẹp
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (!_disposed)
        {
            // Dọn dẹp tài nguyên managed (ví dụ: đối tượng connection)
            // bằng cách gọi Disconnect()
            await Disconnect();

            // Đánh dấu là đã dọn dẹp
            _disposed = true;
        }
    }

    // Nếu lớp của bạn có quản lý tài nguyên không được quản lý (unmanaged resources),
    // bạn sẽ cần thêm một finalizer (destructor).
    // Tuy nhiên, trong hầu hết các trường hợp với .NET hiện đại, điều này là không cần thiết.
    // ~DbDriver()
    // {
    //     // Không gọi code async ở đây!
    //     // Chỉ dọn dẹp tài nguyên unmanaged.
    // }
    Task<bool> IDbDriver.Connect()
    {
        throw new NotImplementedException();
    }
}