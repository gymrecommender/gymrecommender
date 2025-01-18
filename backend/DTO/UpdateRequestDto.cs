using backend.Models;

namespace backend.DTO;

public class UpdateRequestDto {
    public string? Name { get; set; } // Only this field should be allowed
}