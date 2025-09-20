using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace OzonDomains.Models
{
    public class ColumnMapping
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string MappingName { get; set; }

        public int? SelectedClientId { get; set; }
        public virtual OzonClient? SelectedClient { get; set; } = null;

        public string? SelectedStatus { get; set; }

        public int? SelectedManufacturerId { get; set; }
        public virtual Manufacturer? SelectedManufacturer { get; set; } = null;

        public int? SelectedWarehouseId { get; set; }
        public virtual Warehouse? SelectedWarehouse { get; set; }  = null;

        public int? SelectedSupplierId { get; set; }
        public virtual Supplier? SelectedSupplier { get; set; } = null;

        public CurrencyCode? SelectedCurrencyCode { get; set; }

        [NotMapped]
        public Dictionary<string, string> ColumnMappings { get; set; }

        public string ColumnMappingsSerialized
        {
            get => JsonSerializer.Serialize(ColumnMappings);
            set => ColumnMappings = JsonSerializer.Deserialize<Dictionary<string, string>>(value);
        }

        public bool ManufacturerFromArticle {  get; set; } = false;

    }

}
