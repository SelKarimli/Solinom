using System.ComponentModel.DataAnnotations;

namespace FinalProject.MVC.Models;

// Reservation.cs
public class Reservation:BaseEntity
{

    [Required]
    public int RoomId { get; set; }
    public Room Room { get; set; }

    [Required]
    public string UserId { get; set; }
    public User User { get; set; }

 
    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    public int NumberOfGuests { get; set; }

    public decimal TotalPrice { get; set; }
    public string? SpecialRequests { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
}

public enum ReservationStatus
{
    Pending,
    Approved,
    Rejected,
    Completed
}