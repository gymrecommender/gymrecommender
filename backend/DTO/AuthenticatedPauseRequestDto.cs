namespace backend.DTOs
{
    public class AuthenticatedPauseRequestDto
    {
        public Guid UserId { get; set; }
        public DateTime? StartedAt { get; set; }
    }
}