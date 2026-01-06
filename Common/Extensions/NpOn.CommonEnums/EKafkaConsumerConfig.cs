using System.ComponentModel.DataAnnotations;

namespace Common.Extensions.NpOn.CommonEnums;

public enum EKafkaConsumerConfig
{
    // Bật/tắt auto commit offset (giống auto-ack trong RabbitMQ)
    [Display(Name = "enable.auto.commit", Description = "true")]
    EnableAutoCommit,

    // Chu kỳ commit offset tự động (ms)
    [Display(Name = "auto.commit.interval.ms", Description = "3000")] // 3 giây
    AutoCommitIntervalMs,

    // Chính sách reset offset khi không tìm thấy offset hợp lệ
    [Display(Name = "auto.offset.reset", Description = "latest")] // earliest / latest / none
    AutoOffsetReset,

    // Thời gian timeout session (ms) để phát hiện consumer chết
    [Display(Name = "session.timeout.ms", Description = "10000")] // 10 giây
    SessionTimeoutMs,

    // Thời gian tối đa giữa các lần poll (ms)
    [Display(Name = "max.poll.interval.ms", Description = "300000")] // 5 phút
    MaxPollIntervalMs,

    //// Số lượng bản ghi tối đa trả về mỗi lần poll
    // [Display(Name = "max.poll.records", Description = "500")]
    // MaxPollRecords,

    // Kích thước tối đa dữ liệu fetch từ broker (bytes)
    [Display(Name = "fetch.max.bytes", Description = "52428800")] // 50 MB
    FetchMaxBytes,

    // Kích thước tối đa dữ liệu fetch cho mỗi partition (bytes)
    [Display(Name = "max.partition.fetch.bytes", Description = "1048576")] // 1 MB
    MaxPartitionFetchBytes,

    // Thời gian tối đa broker giữ connection khi không có request (ms)
    [Display(Name = "connections.max.idle.ms", Description = "540000")] // 9 phút
    ConnectionsMaxIdleMs,

    // Thời gian heartbeat gửi tới broker (ms)
    [Display(Name = "heartbeat.interval.ms", Description = "3000")] // 3 giây
    HeartbeatIntervalMs
}