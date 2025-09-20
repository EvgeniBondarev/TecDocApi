using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OzonDomains.Models;

public class FileUploadRecord
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string FileName { get; set; }

    [Required]
    [StringLength(255)]
    public string FolderName { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string Result { get; set; }
}