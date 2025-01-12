namespace backend.DTO;

public class GymRecommendationResponceDto
{
    public Guid RequestId { get; set; }
    public List<GymRecommendationDto> MainRecommendations { get; set; }
    public List<GymRecommendationDto> AdditionalRecommendations { get; set; }

    public GymRecommendationResponceDto(Guid requestId, List<GymRecommendationDto> mainRecommendations,
        List<GymRecommendationDto> additionalRecommendations)
    {
        RequestId = requestId;
        MainRecommendations = mainRecommendations;
        AdditionalRecommendations = additionalRecommendations;
    }
}