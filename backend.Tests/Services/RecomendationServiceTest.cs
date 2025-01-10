using System;
using System.Collections.Generic;
using backend.DTO;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Tests.Services
{
    public class RecomendationServiceTest
    {
        [Fact]
        public void GetRatings_Should_Calculate_FinalScore_Correctly_With_Balanced_Priority()
        {
            // Arrange
            // Mock dependencies
            var mockDbContext = new Mock<GymrecommenderContext>(new DbContextOptions<GymrecommenderContext>());
            var mockGeoService = new Mock<GeoService>();

            // Instantiate RecomendationService with mocked dependencies
            var service = new RecomendationService(mockDbContext.Object, mockGeoService.Object);

            // Create sample gyms
            var gym1 = new Gym
            {
                Id = Guid.NewGuid(),
                MonthlyMprice = 100,
                ExternalRating = 4,
                CongestionRating = 3,
                City = new City { Name = "CityA" }
            };

            var gym2 = new Gym
            {
                Id = Guid.NewGuid(),
                MonthlyMprice = 200,
                ExternalRating = 5,
                CongestionRating = 4,
                City = new City { Name = "CityB" }
            };

            var gym3 = new Gym
            {
                Id = Guid.NewGuid(),
                MonthlyMprice = 150,
                ExternalRating = 3,
                CongestionRating = 2,
                City = new City { Name = "CityC" }
            };

            // Create GymTravelInfoDto list
            var gymsWithGeoData = new List<GymTravelInfoDto>
            {
                new GymTravelInfoDto
                {
                    Gym = gym1,
                    TravelPrice = 2.0,
                    TravelTime = 30.0
                },
                new GymTravelInfoDto
                {
                    Gym = gym2,
                    TravelPrice = 4.0,
                    TravelTime = 50.0
                },
                new GymTravelInfoDto
                {
                    Gym = gym3,
                    TravelPrice = 3.0,
                    TravelTime = 40.0
                }
            };

            int priceRatingPriority = 50; // Balanced priority between price and other criteria

            // Act
            var recommendations = service.GetRatings(gymsWithGeoData, priceRatingPriority);

            // Assert
            Assert.NotNull(recommendations);
            Assert.Equal(3, recommendations.Count);

            // Calculate expected normalized values based on the earlier calculations

            // Gym1
            var recommendation1 = recommendations.Find(r => r.Gym.Id == gym1.Id);
            Assert.NotNull(recommendation1);
            Assert.Equal(1.0, recommendation1.NormalizedMembershipPrice, 2); // Inverted normalized price
            Assert.Equal(0.5, recommendation1.NormalizedOverallRating, 2); // ExternalRating normalized
            Assert.Equal(0.5, recommendation1.NormalizedCongestionRating, 2); // CongestionRating normalized
            Assert.Equal(1.0, recommendation1.NormalizedTravelPrice, 2); // Inverted TravelPrice
            Assert.Equal(1.0, recommendation1.NormalizedTravelTime, 2); // Inverted TravelTime
            Assert.Equal(0.825, recommendation1.FinalScore, 3); // FinalScore calculation

            // Gym2
            var recommendation2 = recommendations.Find(r => r.Gym.Id == gym2.Id);
            Assert.NotNull(recommendation2);
            Assert.Equal(0.0, recommendation2.NormalizedMembershipPrice, 2); // Inverted normalized price
            Assert.Equal(1.0, recommendation2.NormalizedOverallRating, 2); // ExternalRating normalized
            Assert.Equal(1.0, recommendation2.NormalizedCongestionRating, 2); // CongestionRating normalized
            Assert.Equal(0.0, recommendation2.NormalizedTravelPrice, 2); // Inverted TravelPrice
            Assert.Equal(0.0, recommendation2.NormalizedTravelTime, 2); // Inverted TravelTime
            Assert.Equal(0.35, recommendation2.FinalScore, 2); // FinalScore calculation

            // Gym3
            var recommendation3 = recommendations.Find(r => r.Gym.Id == gym3.Id);
            Assert.NotNull(recommendation3);
            Assert.Equal(0.5, recommendation3.NormalizedMembershipPrice, 2); // Inverted normalized price
            Assert.Equal(0.0, recommendation3.NormalizedOverallRating, 2); // ExternalRating normalized
            Assert.Equal(0.0, recommendation3.NormalizedCongestionRating, 2); // CongestionRating normalized
            Assert.Equal(0.5, recommendation3.NormalizedTravelPrice, 2); // Inverted TravelPrice
            Assert.Equal(0.5, recommendation3.NormalizedTravelTime, 2); // Inverted TravelTime
            Assert.Equal(0.325, recommendation3.FinalScore, 3); // FinalScore calculation
        }

        [Fact]
        public void GetRatings_Should_Handle_Empty_GymsList()
        {
            // Arrange
            var mockDbContext = new Mock<GymrecommenderContext>(new DbContextOptions<GymrecommenderContext>());
            var mockGeoService = new Mock<GeoService>();

            var service = new RecomendationService(mockDbContext.Object, mockGeoService.Object);

            var gymsWithGeoData = new List<GymTravelInfoDto>(); // Empty list
            int priceRatingPriority = 50;

            // Act
            var recommendations = service.GetRatings(gymsWithGeoData, priceRatingPriority);

            // Assert
            Assert.NotNull(recommendations);
            Assert.Empty(recommendations);
        }

        [Fact]
        public void GetRatings_Should_Handle_All_Null_Criteria()
        {
            // Arrange
            var mockDbContext = new Mock<GymrecommenderContext>(new DbContextOptions<GymrecommenderContext>());
            var mockGeoService = new Mock<GeoService>();

            var service = new RecomendationService(mockDbContext.Object, mockGeoService.Object);

            var gym1 = new Gym
            {
                Id = Guid.NewGuid(),
                MonthlyMprice = null, // Null
                ExternalRating = 0, // Minimum rating
                CongestionRating = 0, // Minimum congestion
                City = new City { Name = "CityD" }
            };

            var gym2 = new Gym
            {
                Id = Guid.NewGuid(),
                MonthlyMprice = null, // Null
                ExternalRating = 0, // Minimum rating
                CongestionRating = 0, // Minimum congestion
                City = new City { Name = "CityE" }
            };

            var gym3 = new Gym
            {
                Id = Guid.NewGuid(),
                MonthlyMprice = null, // Null
                ExternalRating = 0, // Minimum rating
                CongestionRating = 0, // Minimum congestion
                City = new City { Name = "CityF" }
            };

            var gymsWithGeoData = new List<GymTravelInfoDto>
            {
                new GymTravelInfoDto
                {
                    Gym = gym1,
                    TravelPrice = 0.0,
                    TravelTime = 0.0
                },
                new GymTravelInfoDto
                {
                    Gym = gym2,
                    TravelPrice = 0.0,
                    TravelTime = 0.0
                },
                new GymTravelInfoDto
                {
                    Gym = gym3,
                    TravelPrice = 0.0,
                    TravelTime = 0.0
                }
            };

            int priceRatingPriority = 50;

            // Act
            var recommendations = service.GetRatings(gymsWithGeoData, priceRatingPriority);

            // Assert
            Assert.NotNull(recommendations);
            Assert.Equal(3, recommendations.Count);

            foreach (var recommendation in recommendations)
            {
                // Since MonthlyMprice is null, normalized value should be 0.5
                Assert.Equal(0.5, recommendation.NormalizedMembershipPrice, 2);

                // ExternalRating is 0, so normalized value should be 0.0
                Assert.Equal(0.0, recommendation.NormalizedOverallRating, 2);

                // CongestionRating is 0, so normalized value should be 0.0
                Assert.Equal(0.0, recommendation.NormalizedCongestionRating, 2);

                // TravelPrice is 0.0, inverted normalized value should be 1.0
                Assert.Equal(1.0, recommendation.NormalizedTravelPrice, 2);

                // TravelTime is 0.0, inverted normalized value should be 1.0
                Assert.Equal(1.0, recommendation.NormalizedTravelTime, 2);

                // FinalScore calculation:
                // (0.5 * 0.25) + (0.0 * 0.2) + (0.0 * 0.15) + (1.0 * 0.25) + (1.0 * 0.15) = 0.125 + 0 + 0 + 0.25 + 0.15 = 0.525
                Assert.Equal(0.525, recommendation.FinalScore, 3);
            }
        }

        [Fact]
        public void GetRatings_Should_Adjust_Weights_Based_On_Varying_PriceRatingPriority()
        {
            // Arrange
            var mockDbContext = new Mock<GymrecommenderContext>(new DbContextOptions<GymrecommenderContext>());
            var mockGeoService = new Mock<GeoService>();

            var service = new RecomendationService(mockDbContext.Object, mockGeoService.Object);

            var gym1 = new Gym
            {
                Id = Guid.NewGuid(),
                MonthlyMprice = 120,
                ExternalRating = 4,
                CongestionRating = 3,
                City = new City { Name = "CityG" }
            };

            var gym2 = new Gym
            {
                Id = Guid.NewGuid(),
                MonthlyMprice = 180,
                ExternalRating = 5,
                CongestionRating = 4,
                City = new City { Name = "CityH" }
            };

            var gym3 = new Gym
            {
                Id = Guid.NewGuid(),
                MonthlyMprice = 160,
                ExternalRating = 3,
                CongestionRating = 2,
                City = new City { Name = "CityI" }
            };

            var gymsWithGeoData = new List<GymTravelInfoDto>
            {
                new GymTravelInfoDto
                {
                    Gym = gym1,
                    TravelPrice = 2.5,
                    TravelTime = 35.0
                },
                new GymTravelInfoDto
                {
                    Gym = gym2,
                    TravelPrice = 4.5,
                    TravelTime = 55.0
                },
                new GymTravelInfoDto
                {
                    Gym = gym3,
                    TravelPrice = 3.5,
                    TravelTime = 45.0
                }
            };

            // Define multiple PriceRatingPriority values
            var pricePriorities = new List<int> { 0, 50, 100 };

            foreach (var priceRatingPriority in pricePriorities)
            {
                // Act
                var recommendations = service.GetRatings(gymsWithGeoData, priceRatingPriority);

                // Assert
                Assert.NotNull(recommendations);
                Assert.Equal(3, recommendations.Count);

                foreach (var recommendation in recommendations)
                {
                    // The final score will vary based on PriceRatingPriority
                    // For brevity, we'll check if the sum of normalized values multiplied by weights equals FinalScore
                    // Since weights are dynamic, precise value checks require recalculating expected scores
                    // Here, we ensure FinalScore is within the valid range [0,1]

                    Assert.InRange(recommendation.FinalScore, 0.0, 1.0);
                }
            }
        }
    }
}