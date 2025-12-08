namespace CommonDb.DbResults.Grpc;

public abstract class NpOnBaseGrpcObject
{
    #region Field Config

    public abstract Dictionary<string, string>? FieldMap { get; protected set; }

    public void CreateDefaultFieldMapper()
    {
        if (FieldMap is { Count: > 0 })
            throw new ArgumentNullException(nameof(FieldMap) + "is created");
        BaseFieldMapper();
    }

    private void BaseFieldMapper()
    {
        FieldMap ??= new();
        FieldMapper();
    }

    #endregion Field Config

    protected abstract void FieldMapper();
}