using System.ComponentModel.DataAnnotations;

namespace FinalProject.MVC.ViewModels.Auths;

public class RegisterVM
{
    [Required, MaxLength(64)]
    public string Fullname { get; set; } = null!;
    [Required, MaxLength(128), EmailAddress]
    public string Email { get; set; } = null!;
    [Required, MaxLength(128)]
    public string Username { get; set; } = null!;
    [Required, MaxLength(32), DataType(DataType.Password)]
    public string Password { get; set; }
    [Required, MaxLength(32), DataType(DataType.Password), Compare(nameof(Password))]
    public string RePassword { get; set; }
}
