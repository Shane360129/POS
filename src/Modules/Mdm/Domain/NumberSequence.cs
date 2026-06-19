namespace InvenFlow.Modules.Mdm.Domain;

/// <summary>
/// 編號計數器（對齊 docs/02 §3.2 / §7.5）。每 (Key, Period) 一列，
/// 取代既有以 <c>Count()+1</c> 產號（併發/作廢會重號）的做法。
/// </summary>
public sealed class NumberSequence
{
    public long Id { get; set; }
    public string Key { get; set; } = default!;
    public string Period { get; set; } = default!;
    public long NextValue { get; set; } = 1;
}
