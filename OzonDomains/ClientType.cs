using System.ComponentModel.DataAnnotations;

namespace OzonDomains
{
    public enum ClientType
    {
        [Display(Name = "All")]
        ALL = 0,

        [Display(Name = "Ozon")]
        OZON = 1,

        [Display(Name = "Yandex")]
        YANDEX =2,
    }
}
