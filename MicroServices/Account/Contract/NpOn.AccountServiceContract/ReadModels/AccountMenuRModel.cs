using Definitions.NpOn.ProjectEnums.AccountEnums;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

[ProtoContract]
public class AccountMenuRModel : BaseAccountRModelFromGrpcTable
{
    [ProtoMember(1)] public required Guid Id { get; set; }
    [ProtoMember(2)] public MenuItemType? Type { get; set; }
    [ProtoMember(3)] public required string TitleKey { get; set; }
    [ProtoMember(4)] public required string Icon { get; set; }
    [ProtoMember(5)] public string? Path { get; set; }
    [ProtoMember(6)] public Guid? ParentId { get; set; }
    [ProtoMember(7)] public int DisplayOrder { get; set; }
    [ProtoMember(8)] public required bool Disabled { get; set; }
    [ProtoMember(9)] public MenuScope? Scope { get; set; }
    [ProtoMember(10)] public string? Module { get; set; }
    [ProtoMember(11)] public AccountMenuStatus? MenuStatus { get; set; }
    [ProtoMember(12)] public DateTime? CreatedAt { get; set; }
    [ProtoMember(13)] public DateTime? UpdatedAt { get; set; }
    [ProtoMember(14)] public AccountMenuRModel[]? Children { get; set; }

    public static AccountMenuRModel[] ToTree(IEnumerable<AccountMenuRModel>? models)
    {
        if (models == null) return [];

        var lookup = models.ToDictionary(
            x => x.Id,
            x => new AccountMenuRModel
            {
                Id = x.Id,
                Icon = x.Icon,
                TitleKey = x.TitleKey,
                Type = x.Type,
                Path = x.Path,
                ParentId = x.ParentId,
                DisplayOrder = x.DisplayOrder,
                Disabled = x.Disabled,
                Scope = x.Scope,
                Module = x.Module,
                MenuStatus = x.MenuStatus,
                Children = []
            });
        var rootNodes = new List<AccountMenuRModel>();

        foreach (var item in lookup.Values)
        {
            if (item.ParentId.HasValue && lookup.TryGetValue(item.ParentId.Value, out var parent))
            {
                parent.Children = (parent.Children)?.Append(item).ToArray();
            }
            else
            {
                rootNodes.Add(item);
            }
        }

        // Recursively sort
        return rootNodes
            .OrderBy(x => x.DisplayOrder)
            .Select(x =>
            {
                SortChildren(x);
                return x;
            })
            .ToArray();
    }

    private static void SortChildren(AccountMenuRModel node)
    {
        if (node.Children is not { Length: > 0 }) return;
        node.Children = node.Children.OrderBy(c => c.DisplayOrder).ToArray();
        foreach (var child in node.Children)
        {
            SortChildren(child);
        }
    }

    protected override void FieldMapper()
    {
        FieldMap ??= new Dictionary<string, string>();
        FieldMap.Add(nameof(Id), "id");
        FieldMap.Add(nameof(Type), "type");
        FieldMap.Add(nameof(TitleKey), "title_key");
        FieldMap.Add(nameof(Icon), "icon");
        FieldMap.Add(nameof(Path), "path");
        FieldMap.Add(nameof(ParentId), "parent_id");
        FieldMap.Add(nameof(DisplayOrder), "display_order");
        FieldMap.Add(nameof(Disabled), "disabled");
        FieldMap.Add(nameof(Scope), "scope");
        FieldMap.Add(nameof(Module), "module");
        FieldMap.Add(nameof(MenuStatus), "menu_status");
    }
}