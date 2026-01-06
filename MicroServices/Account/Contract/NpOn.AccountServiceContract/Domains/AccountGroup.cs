using Common.Extensions.NpOn.CommonBaseDomain.Attributes;
using Common.Extensions.NpOn.CommonMode;
using Common.Extensions.NpOn.HandleFlow.Attributes;
using Definitions.NpOn.ProjectEnums.AccountEnums;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Domains;

[TableLoader("acc_srv_account_group")]
public sealed class AccountGroup : BaseAccountDomain
{
    [ProtoMember(1)]
    [Field("id")]
    [Pk("id")]
    public Guid GroupId { get; set; }

    [ProtoMember(2)]
    [Field("acc_srv_account_leader_id")]
    [Pk("acc_srv_account_leader_id")]
    public Guid Leader { get; set; }

    [ProtoMember(3)]
    [Field("acc_srv_account_member")]
    [Pk("acc_srv_account_member")]
    public Guid? Member { get; set; } // first record when create maybe null

    [ProtoMember(4)] [Field("group_name")] public string? GroupName { get; set; }
    [ProtoMember(5)] [Field("group_type")] public EAccountGroupType? GroupType { get; set; }

    [ProtoMember(9)] [Field("created_at")] public DateTime? CreatedAt { get; set; }

    [ProtoMember(10)]
    [Field("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public AccountGroup(AccountGroupAddOrChangeCommand command,
        Guid? member = null)
    {
        if (command.GroupId != null && command.GroupId != Guid.Empty)
            GroupId = command.GroupId.AsDefaultGuid();
        Leader = command.Leader;
        GroupName = command.GroupName;
        Member = member ?? Guid.Empty;
        GroupType = command.GroupTypes.CombineFlags();
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
    }

    public AccountGroup(AccountGroupDeleteCommand command,
        Guid? member = null)
    {
        GroupId = command.GroupId;
        Leader = command.Leader;
        GroupName = command.GroupName;
        Member = member;
    }

    public AccountGroup()
    {
    }
}

public static class AccountGroupExtensions
{
    public static List<AccountGroup> FromCommand(this AccountGroupAddOrChangeCommand command)
    {
        List<AccountGroup> groupMembers = [new AccountGroup(command)];
        if (command.Members is not { Length: > 0 })
            return groupMembers;

        foreach (var memberId in command.Members)
            groupMembers.Add(new AccountGroup(command, memberId));
        return groupMembers;
    }

    public static List<AccountGroup> FromCommand(this AccountGroupDeleteCommand command)
    {
        if (command.Members is not { Length: > 0 })
            return [new AccountGroup(command)];

        List<AccountGroup> groupMembers = [];
        foreach (var memberId in command.Members)
            groupMembers.Add(new AccountGroup(command, memberId));
        return groupMembers;
    }

    public static List<AccountGroup>? FromCommand(this AccountGroupCopyCommand command,
        Guid[]? accountExistedFromOldGroups)
    {
        List<AccountGroup> groupMembers = new();

        var existed = accountExistedFromOldGroups != null
            ? new HashSet<Guid>(accountExistedFromOldGroups)
            : new HashSet<Guid>();

        foreach (var component in command.Components)
        {
            EAccountGroupType groupType = component.GroupTypes.CombineFlags();
            if (groupType == EAccountGroupType.InvalidGroup)
            {
                return null;
            }
            var adds = new List<Guid>();
            var excludes = new List<Guid>();
            Guid newGroupId = IndexerMode.CreateGuid();
            groupMembers.Add(new AccountGroup()
            {
                Leader = component.Leader,
                GroupName = component.GroupName,
                GroupType = groupType,
                Member = Guid.Empty,
                GroupId = newGroupId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            });

            if (component.MemberAdds != null) // Get accounts to add (not yet in existed)
            {
                var toAdd = component.MemberAdds.Where(x => !existed.Contains(x)).ToList();
                adds.AddRange(toAdd);
                foreach (var id in toAdd)
                    existed.Add(id); // update to the current set
            }

            if (component.MemberExcludes != null) //Get accounts to remove (present in existed)
            {
                var toExclude = component.MemberExcludes.Where(x => existed.Contains(x)).ToList();
                excludes.AddRange(toExclude);
                foreach (var id in toExclude)
                    existed.Remove(id); // remove from the current set
            }

            foreach (var existedAccountId in existed)
                groupMembers.Add(new AccountGroup()
                {
                    Leader = component.Leader,
                    GroupName = component.GroupName,
                    GroupType = groupType,
                    Member = existedAccountId,
                    GroupId = newGroupId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                });

            component.MemberAdds = adds.ToArray();
            component.MemberExcludes = excludes.ToArray();
        }

        return groupMembers;
    }
}