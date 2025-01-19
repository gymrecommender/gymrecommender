namespace backend.DTO;

public class RatingDto {
    public int Rating { get; set; }
    public TimeOnly VisitTime { get; set; }
    public int WaitingTime { get; set; }
    public int Crowdedness { get; set; }
}