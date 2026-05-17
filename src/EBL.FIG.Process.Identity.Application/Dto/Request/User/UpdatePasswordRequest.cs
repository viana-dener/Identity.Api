namespace EBL.FIG.Process.Identity.Application.Dto.Request.User;

public class UpdatePasswordRequest
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}
