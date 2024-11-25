using System.ComponentModel.DataAnnotations;

namespace Task1.Models;

public class CreatedByModel
{
    [Required]
    public string? CreatedBy { get; set; }
}