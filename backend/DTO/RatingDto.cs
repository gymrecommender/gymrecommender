namespace backend.DTO;

public class RatingDto {
    public Guid Id { get; set; }
    public int Rating1 { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UserId { get; set; }
    public Guid GymId { get; set; }
}