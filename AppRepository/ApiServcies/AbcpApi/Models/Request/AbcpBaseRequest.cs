namespace Servcies.ApiServcies.AbcpApi.Models.Request;


public class AbcpBaseRequest : IRequestModel
{
    public string UserLogin { get; set; }
    public string UserPsw { get; set; }
}