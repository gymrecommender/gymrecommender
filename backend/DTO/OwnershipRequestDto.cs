namespace backend.DTO;

public class OwnershipRequestDto
{
        public Guid GymId { get; set; } // The ID of the gym for which ownership is requested
        public Guid AccountId { get; set; } // The ID of the account making the request
        public string? Message { get; set; } // Optional message provided by the user
}
