using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace OzonDomains.Models;

[Table("orders_history")]
public class OrderHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("Id")]
    public int Id { get; set; }

    [Required]
    [Column("OrderId")]
    public int OrderId { get; set; }

    [Column("ColumnName")]
    [MaxLength(100)]
    public string? ColumnName { get; set; }

    [Column("ColumnDisplayName")]
    [MaxLength(200)]
    public string? ColumnDisplayName { get; set; }

    [Column("OldValue")]
    public string? OldValue { get; set; }

    [Column("NewValue")]
    public string? NewValue { get; set; }

    [Required]
    [Column("ChangedAt")]
    public DateTime ChangedAt { get; set; }

    [Column("ChangedBy")]
    [MaxLength(255)]
    public string? ChangedBy { get; set; }

    // Навигационное свойство
    [ForeignKey("OrderId")]
    public virtual Order? Order { get; set; }

    // Методы для форматирования значений
    [NotMapped]
    public string FormattedOldValue => FormatValue(OldValue);

    [NotMapped]
    public string FormattedNewValue => FormatValue(NewValue);

    [NotMapped]
    public string FormattedChangedAt => ChangedAt.ToString("dd.MM.yyyy HH:mm:ss");

    private string FormatValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalValue))
        {
            return decimalValue.ToString("0.##", CultureInfo.InvariantCulture);
        }

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue))
        {
            return doubleValue.ToString("0.##", CultureInfo.InvariantCulture);
        }
        return value;
    }
    [NotMapped]
    public bool IsOldValueNumeric => IsNumeric(OldValue);

    [NotMapped]
    public bool IsNewValueNumeric => IsNumeric(NewValue);

    private bool IsNumeric(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _) ||
               double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
    }
}