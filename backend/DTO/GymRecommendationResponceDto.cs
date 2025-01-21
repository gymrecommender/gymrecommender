namespace backend.DTO;

public class GymRecommendationResponceDto {
    public Guid? RequestId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public List<GymRecommendationDto> MainRecommendations { get; set; }
    public List<GymRecommendationDto> AdditionalRecommendations { get; set; }

    public GymRecommendationResponceDto(Guid? requestId, double latitude, double longitude,
                                        List<GymRecommendationDto> mainRecommendations,
                                        List<GymRecommendationDto> additionalRecommendations) {
        RequestId = requestId;
        Latitude = latitude;
        Longitude = longitude;
        MainRecommendations = mainRecommendations;
        AdditionalRecommendations = additionalRecommendations;
    }
}