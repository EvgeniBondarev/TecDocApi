using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OzonDomains;

public class ExcelMapping
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } // Название маппинга

    [Required]
    [MaxLength(500)]
    public string SourceFileName { get; set; }

    [Required]
    [MaxLength(500)]
    public string DestinationFileName { get; set; }

    [Required]
    [MaxLength(100)]
    public string SourceSheet { get; set; }

    [Required]
    [MaxLength(100)]
    public string DestinationSheet { get; set; }

    public int SourceHeaderRow { get; set; } = 1;
    public int DestinationHeaderRow { get; set; } = 1;
    public int SourceDataStartRow { get; set; } = 2;
    public int DestinationDataStartRow { get; set; } = 2;

    // Сериализованные данные маппинга
    [Column(TypeName = "nvarchar(max)")]
    public string ColumnMappingsJson { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }

    // Навигационное свойство для пользователя (если нужно)
    public string CreatedBy { get; set; }
}