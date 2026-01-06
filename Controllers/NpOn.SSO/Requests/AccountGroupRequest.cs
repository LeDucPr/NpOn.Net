using Definitions.NpOn.ProjectEnums.AccountEnums;

namespace Controllers.NpOn.SSO.Requests;

public class AccountGroupAddOrChangeRequest
{
    public Guid? GroupId { get; set; }
    public Guid Leader { get; set; }
    public Guid[]? Members { get; set; }
    public string? GroupName { get; set; }
    public EAccountGroupType[]? GroupTypes { get; set; }
}

public class AccountGroupCopyRequest
{
    public Guid GroupIdNeedCopy { get; set; }
    public required AccountGroupCopyComponentRequest[] Components { get; set; }
}

public class AccountGroupCopyComponentRequest
{
    public required Guid Leader { get; set; }
    public string? GroupName { get; set; }
    public Guid[]? MemberExcludes { get; set; }
    public Guid[]? MemberAdds { get; set; }
    public required EAccountGroupType[] GroupTypes { get; set; }
}

public class AccountGroupDeleteRequest
{
    public required Guid GroupId { get; set; }
    public Guid Leader { get; set; }
    public Guid[]? Members { get; set; }
    public string? GroupName { get; set; }
}