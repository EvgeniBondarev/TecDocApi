using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OzonDomains.Models.BitrixModels;

[Table("b_catalog_store")]
public class BCatalogStore
{
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("TITLE")]
        [StringLength(75)]
        public string? Title { get; set; }

        [Column("ACTIVE")]
        [StringLength(1)]
        public string Active { get; set; } = "Y";

        [Column("ADDRESS")]
        [StringLength(245)]
        public string Address { get; set; } = string.Empty;

        [Column("DESCRIPTION", TypeName = "mediumtext")]
        public string? Description { get; set; }

        [Column("GPS_N")]
        [StringLength(15)]
        public string? GpsN { get; set; } = "0";

        [Column("GPS_S")]
        [StringLength(15)]
        public string? GpsS { get; set; } = "0";

        [Column("IMAGE_ID")]
        [StringLength(45)]
        public string? ImageId { get; set; }

        [Column("LOCATION_ID")]
        public int? LocationId { get; set; }

        [Column("DATE_MODIFY")]
        public DateTime? DateModify { get; set; }

        [Column("DATE_CREATE")]
        public DateTime? DateCreate { get; set; }

        [Column("USER_ID")]
        public int? UserId { get; set; }

        [Column("MODIFIED_BY")]
        public int? ModifiedBy { get; set; }

        [Column("PHONE")]
        [StringLength(45)]
        public string? Phone { get; set; }

        [Column("SCHEDULE")]
        [StringLength(255)]
        public string? Schedule { get; set; }

        [Column("XML_ID")]
        [StringLength(255)]
        public string? XmlId { get; set; }

        [Column("SORT")]
        public int Sort { get; set; } = 100;

        [Column("EMAIL")]
        [StringLength(255)]
        public string? Email { get; set; }

        [Column("ISSUING_CENTER")]
        [StringLength(1)]
        public string IssuingCenter { get; set; } = "Y";

        [Column("SHIPPING_CENTER")]
        [StringLength(1)]
        public string ShippingCenter { get; set; } = "Y";

        [Column("SITE_ID")]
        [StringLength(2)]
        public string? SiteId { get; set; }

        [Column("CODE")]
        [StringLength(255)]
        public string? Code { get; set; }

        [Column("IS_DEFAULT")]
        [StringLength(1)]
        public string IsDefault { get; set; } = "N";
}