using System.ComponentModel.DataAnnotations;

namespace JobPortal.API.DTOs;

public class CreateVnPayPaymentRequest
{
    [Required]
    public long PostingPackageId { get; set; }
}

