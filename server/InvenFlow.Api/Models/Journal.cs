using System.ComponentModel.DataAnnotations;

namespace InvenFlow.Api.Models;

public class JournalEntry
{
    public int Id { get; set; }
    [Required, MaxLength(30)] public string Number { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.UtcNow;
    [MaxLength(500)] public string Description { get; set; } = "";
    [MaxLength(30)] public string RefType { get; set; } = "";
    public int? RefId { get; set; }
    public bool IsReversal { get; set; }
    public List<JournalLine> Lines { get; set; } = new();
}

public class JournalLine
{
    public int Id { get; set; }
    public int JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
    [Required, MaxLength(10)] public string AccountCode { get; set; } = "";
    public Account? Account { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    [MaxLength(500)] public string Description { get; set; } = "";
}
