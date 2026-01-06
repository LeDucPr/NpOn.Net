using Common.Extensions.NpOn.CommonMode;

namespace Tools.PgGenFldCode.Object;

public class ComboBoxSelectItem()
{
    public string? Label { get; set; }
    public object? ItemValue { get; set; }
    public override string ToString()
    {
        return Label.AsDefaultString();
    }
}

public class ComboBoxSelectItem<TEnum> : ComboBoxSelectItem where TEnum : struct, Enum
{
    public new TEnum ItemValue { get; set; }

    public ComboBoxSelectItem(string label, TEnum value)
    {
        Label = label;
        ItemValue = value;
    }
}
