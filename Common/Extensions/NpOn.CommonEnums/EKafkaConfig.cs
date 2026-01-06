using System.ComponentModel.DataAnnotations;

namespace Common.Extensions.NpOn.CommonEnums;

public enum EKafkaTopicConfig
{
    // Thời gian giữ dữ liệu (ms). Sau thời gian này dữ liệu sẽ bị xóa nếu cleanup.policy=delete
    [Display(Name = "retention.ms", Description = "86400000")] // 1 ngày
    RetentionMs,

    // Dung lượng tối đa dữ liệu giữ lại (bytes). Khi vượt quá sẽ xóa dữ liệu cũ
    [Display(Name = "retention.bytes", Description = "-1")] // -1 = không giới hạn
    RetentionBytes,

    // Chính sách dọn dẹp dữ liệu: delete (xóa theo retention) hoặc compact (giữ bản ghi mới nhất theo key)
    [Display(Name = "cleanup.policy", Description = "delete")]
    CleanupPolicy,

    // Kiểu nén dữ liệu: none, gzip, snappy, lz4, zstd
    [Display(Name = "compression.type", Description = "producer / none / gzip / snappy / lz4 / zstd")]
    CompressionType,

    // Kích thước tối đa của một segment log (bytes)
    [Display(Name = "segment.bytes", Description = "1073741824")] // 1 GB
    SegmentBytes,

    // Thời gian tối đa giữ segment trước khi xóa (ms)
    [Display(Name = "segment.ms", Description = "259200000")] // 3 ngày
    SegmentMs,

    // Thời gian tối đa giữ segment chưa active (ms)
    [Display(Name = "segment.jitter.ms", Description = "0")]
    SegmentJitterMs,

    // Số replica tối thiểu phải đồng bộ để producer ghi thành công
    [Display(Name = "min.insync.replicas", Description = "1")]
    MinInsyncReplicas,

    // Cho phép bầu leader từ replica chưa đồng bộ (true = có thể mất dữ liệu, false = an toàn hơn)
    [Display(Name = "unclean.leader.election.enable", Description = "true")]
    UncleanLeaderElectionEnable,

    // Kích thước tối đa của một message (bytes)
    [Display(Name = "max.message.bytes", Description = "10485760")] // 10 MB
    MaxMessageBytes,

    // Thời gian giữ lại tombstone record (ms) cho compacted topic
    [Display(Name = "delete.retention.ms", Description = "86400000")] // 1 ngày
    DeleteRetentionMs,

    // Độ trễ trước khi xóa file log sau khi bị đánh dấu xóa (ms)
    [Display(Name = "file.delete.delay.ms", Description = "60000")] // 60 giây
    FileDeleteDelayMs,

    // Kiểu timestamp: CreateTime (producer gửi) hoặc LogAppendTime (broker ghi)
    [Display(Name = "message.timestamp.type", Description = "CreateTime")]
    MessageTimestampType,

    // Chênh lệch tối đa cho phép giữa timestamp message và thời gian hiện tại (ms)
    [Display(Name = "message.timestamp.difference.max.ms", Description = "9223372036854775807")] // mặc định rất lớn
    MessageTimestampDifferenceMaxMs,

    // Thời gian giữ lại index file (ms)
    [Display(Name = "index.interval.bytes", Description = "4096")]
    IndexIntervalBytes,

    // Khoảng cách giữa các offset index entry (bytes)
    [Display(Name = "flush.messages", Description = "0")] // 0 = không ép flush theo số message
    FlushMessages,

    // Thời gian ép flush log ra disk (ms)
    [Display(Name = "flush.ms", Description = "0")]
    FlushMs,

    // Thời gian giữ lại offset commit (ms)
    [Display(Name = "offsets.retention.ms", Description = "259200000")] // 3 ngày
    OffsetsRetentionMs,

    // Số lượng offset commit tối đa giữ lại cho mỗi group
    [Display(Name = "offsets.retention.minutes", Description = "4320")] // 3 ngày
    OffsetsRetentionMinutes,

    // Cho phép ghi log theo batch lớn hơn (tối ưu throughput)
    [Display(Name = "message.format.version", Description = "3.0")] // phiên bản format message
    MessageFormatVersion
}