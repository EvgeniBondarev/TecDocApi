namespace OzonOrdersWeb.Areas.PartsInfo.Models;

public enum ImageSource
{
    Database = 0,
    S3 = 1,
    VolnaParser = 2,
    Internet = 3,
    Unknown = 4
}