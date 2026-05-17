namespace EBL.FIG.Process.Identity.Application.Dto.Request.User;

public class UpdateSecretRequest
{
    public string CurrentSecret { get; set; }
    public string NewSecret { get; set; }
}
