using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OzonDomains.Models.MatchedRowSys
{
    public class MatchedRow
    {
        public int Id { get; set; }

        [NotMapped]
        public Dictionary<string, string> File1Data { get; set; }

        public string File1DataSerialized
        {
            get => JsonSerializer.Serialize(File1Data);
            set => File1Data = JsonSerializer.Deserialize<Dictionary<string, string>>(value);
        }

        [NotMapped]
        public List<Dictionary<string, string>> File2Data { get; set; }

        public string File2DataSerialized
        {
            get => JsonSerializer.Serialize(File2Data);
            set => File2Data = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(value);
        }
    }
}
