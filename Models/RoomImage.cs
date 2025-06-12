namespace FinalProject.MVC.Models;
public class RoomImage:BaseEntity
{
    public string ImageUrl { get; set; }
    public int RoomId { get; set; }
    public Room Room { get; set; }
}
