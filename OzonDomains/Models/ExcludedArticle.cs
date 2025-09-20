using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OzonDomains.Models;

public class ExcludedArticle
{
    public int Id { get; set; }

    [MaxLength(100)]
    [Display(Name = "Артикул")]
    public string Article { get; set; }

    [Required]
    public int OzonClientId { get; set; }

    [ForeignKey(nameof(OzonClientId))]
    public OzonClient OzonClient { get; set; }
}