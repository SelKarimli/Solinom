using FinalProject.MVC.Views.Account.Enums;

namespace FinalProject.MVC.Extensions
{
    public static class RolesExtension
    {
        public static string GetRole(this Roles role)
        {
            return role switch
            {
                Roles.User => nameof(Roles.User),
                Roles.Admin => nameof(Roles.Admin),
                Roles.Moderator => nameof(Roles.Moderator),
                Roles.HotelOwner => nameof(Roles.HotelOwner),
                Roles.Cashier => nameof(Roles.Cashier)
            };
        }
    }
}
