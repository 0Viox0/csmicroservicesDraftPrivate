using System.ComponentModel.DataAnnotations;

namespace GrpcClientHttpGateway.Models;

public class CreatedByModel
{
    [Required]
    public string? CreatedBy { get; set; }
}