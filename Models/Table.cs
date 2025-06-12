
namespace FinalProject.MVC.Models;

public class Table : BaseEntity
{
    public decimal Seat { get; set; }
    public string Title { get; set; }
    public bool Reserved { get; set; }
    public ICollection<TableReservation> Reservations { get; set; } = new List<TableReservation>();
}
public class TableReservation : BaseEntity
{
    public int TableId { get; set; }
    public Table Table { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }
    public DateTime ReservationDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal DepositAmount { get; set; } = 20.00m;
    public bool IsConfirmed { get; set; }
}