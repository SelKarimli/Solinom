using FinalProject.MVC.Models;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.MVC.ViewModels;

public class ReservationCreateVM
{
    public int RoomId { get; set; }
    public string? RoomName { get; set; }
    public decimal? Price { get; set; }  // Add this
    public int Capacity { get; set; }    // Add this

    [Required]
    [Display(Name = "Check-In Date")]
    [DataType(DataType.Date)]
    public DateTime CheckInDate { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending; // Add this

    // Add validation for check-out after check-in
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CheckOutDate <= CheckInDate)
        {
            yield return new ValidationResult(
                "Check-out date must be after check-in date",
                new[] { nameof(CheckOutDate) });
        }

        if (NumberOfGuests > Capacity)
        {
            yield return new ValidationResult(
                $"Number of guests exceeds room capacity of {Capacity}",
                new[] { nameof(NumberOfGuests) });
        }
    }
    [Required]
    [Display(Name = "Check-Out Date")]
    [DataType(DataType.Date)]
    public DateTime CheckOutDate { get; set; }

    [Range(1, 10, ErrorMessage = "Number of guests must be between 1 and {2}")]
    public int NumberOfGuests { get; set; } = 1;

    [Display(Name = "Special Requests")]
    public string? SpecialRequests { get; set; }

    public DateTime MinDate { get; set; } = DateTime.Today.AddDays(1);
    public enum ReservationStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
