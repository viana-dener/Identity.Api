namespace EBL.FIG.Process.Identity.Application.Dto.Request.Auth;

public class RefreshRequest
{
    public int TenantId { get; set; }
    public string RefreshToken { get; set; }
}
