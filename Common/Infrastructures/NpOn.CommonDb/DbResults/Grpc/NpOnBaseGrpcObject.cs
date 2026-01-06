namespace Common.Infrastructures.NpOn.CommonDb.DbResults.Grpc;

public abstract class NpOnBaseGrpcObject
{
    #region Field Config

    public abstract Dictionary<string, string>? FieldMap { get; protected set; }

    /// <summary>
    /// call in first requisition 
    /// </summary>
    public void CreateDefaultFieldMapper()
    {
        if (FieldMap is { Count: > 0 })
            throw new ArgumentNullException(nameof(FieldMap) + "is created");
        BaseFieldMapper();
    }
    
    protected virtual void BaseFieldMapper()
    {
        FieldMap ??= new();
        FieldMapper();
    }

    #endregion Field Config

    protected abstract void FieldMapper();
}