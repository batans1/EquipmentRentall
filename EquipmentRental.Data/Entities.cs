using System.ComponentModel.DataAnnotations;

namespace EquipmentRental.Data;

public class Role
{
    public int Id { get; set; }

    [MaxLength(32)]
    public required string Name { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}

public class User
{
    public int Id { get; set; }

    [MaxLength(32)]
    public required string Username { get; set; }

    [MaxLength(256)]
    public required string PasswordHash { get; set; }

    [MaxLength(64)]
    public required string FirstName { get; set; }

    [MaxLength(64)]
    public required string LastName { get; set; }

    public int RoleId { get; set; }
    public Role? Role { get; set; }

    public ICollection<RentalRequest> RentalRequests { get; set; } = new List<RentalRequest>();
}

public enum EquipmentCondition
{
    [Display(Name = "Ново")]
    New = 1,
    [Display(Name = "Използвано")]
    Used = 2,
    [Display(Name = "За ремонт")]
    ForRepair = 3
}

public class EquipmentItem
{
    public int Id { get; set; }

    [MaxLength(64)]
    public required string Name { get; set; }

    [MaxLength(255)]
    public required string Description { get; set; }

    public int AvailableQuantity { get; set; }

    [MaxLength(512)]
    public required string ImageUrl { get; set; }

    public EquipmentCondition Condition { get; set; }

    public ICollection<RentalRequestItem> RentalRequestItems { get; set; } = new List<RentalRequestItem>();
}

public enum RentalRequestStatus
{
    [Display(Name = "В изчакване")]
    Pending = 1,
    [Display(Name = "Одобрена")]
    Approved = 2,
    [Display(Name = "Отказана")]
    Rejected = 3
}

public class RentalRequest
{
    public int Id { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    [MaxLength(255)]
    public required string Purpose { get; set; }

    public RentalRequestStatus Status { get; set; } = RentalRequestStatus.Pending;

    public int UserId { get; set; }
    public User? User { get; set; }

    public ICollection<RentalRequestItem> RentalRequestItems { get; set; } = new List<RentalRequestItem>();
}

public class RentalRequestItem
{
    public int RentalRequestId { get; set; }
    public RentalRequest? RentalRequest { get; set; }

    public int EquipmentItemId { get; set; }
    public EquipmentItem? EquipmentItem { get; set; }

    public int Quantity { get; set; }
}
