namespace OzonRepositories.Data.Bitrix;

using System;
using System.ComponentModel.DataAnnotations;

    public class PriceHistory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "BitrixId обязателен")]
        public int BitrixId { get; set; }

        [Required(ErrorMessage = "Артикул обязателен")]
        public string Article { get; set; }

        public DateTime CreateDateTime { get; set; }

        [Required(ErrorMessage = "Цена обязательна")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше 0")]
        public decimal PriceValue { get; set; }

        [Range(0.0, double.MaxValue, ErrorMessage = "Registration Price должна быть >= 0")]
        public decimal RegistrationPrice { get; set; }  

        public string? ValueSource { get; set; }

        public string? Author { get; set; }

        public string? Currency { get; set; }
    }
