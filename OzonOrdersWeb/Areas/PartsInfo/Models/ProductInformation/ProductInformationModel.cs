namespace OzonOrdersWeb.Areas.PartsInfo.Models.ProductInformation;

public class ProductInformationModel
{
    public string TOW_KOD { get; set; }
    public string IC_INDEX { get; set; }
    public string TEC_DOC { get; set; }
    public int TEC_DOC_PROD { get; set; }
    public string ARTICLE_NUMBER { get; set; }
    public string MANUFACTURER { get; set; }
    public string SHORT_DESCRIPTION { get; set; }
    public string DESCRIPTION { get; set; }
    public string BARCODES { get; set; }
    public decimal? PACKAGE_WEIGHT { get; set; }
    public decimal? PACKAGE_LENGTH { get; set; }
    public decimal? PACKAGE_WIDTH { get; set; }
    public decimal? PACKAGE_HEIGHT { get; set; }
    public string CUSTOM_CODE { get; set; }
}