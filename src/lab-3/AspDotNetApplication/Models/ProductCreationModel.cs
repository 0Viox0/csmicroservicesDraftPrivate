using System.ComponentModel.DataAnnotations;

namespace Task1.Models;

public class ProductCreationModel
{
    [Required]
    public string? Name { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "The value must be a positive number greater than 0.")]
    public decimal Price { get; set; }
}