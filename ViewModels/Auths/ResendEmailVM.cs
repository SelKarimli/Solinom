using System.ComponentModel.DataAnnotations;

namespace FinalProject.MVC.ViewModels.Auths;

public class ResendEmailVM { 
[Required]
[EmailAddress]
public string Email { get; set; }
}
