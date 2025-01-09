import React, { useState, useEffect } from "react";
import MapSection from "../components/MapSection.jsx";
import { CoordinatesProvider } from "../context/CoordinatesProvider.jsx";
import { axiosInternal } from "../services/axios.jsx";

const Recommendation = () => {
  const [recommendations, setRecommendations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchRecommendations = async () => {
      setLoading(true);
      setError(null);

      try {
        const dummyRecommendations = [
          { 
            id: 1, 
            name: "Lakeside Fitness", 
            address: "456 Lakeview Ave, Chicago, IL", 
            date: "7 Nov 2024", 
            overallRating: 4.5, 
            timeRating: 4, 
            costRating: 4.5, 
            travellingTime: 15, 
            totalCost: 30, 
            currency: "USD" 
          },
          { 
            id: 2, 
            name: "Uptown Strength", 
            address: "2101 N Halsted St, Chicago, IL", 
            date: "6 Nov 2024", 
            overallRating: 4, 
            timeRating: 3.5, 
            costRating: 4, 
            travellingTime: 20, 
            totalCost: 25, 
            currency: "USD" 
          },
          { 
            id: 3, 
            name: "South Loop Performance", 
            address: "1325 S State St, Chicago, IL", 
            date: "7 Nov 2024", 
            overallRating: 5, 
            timeRating: 5, 
            costRating: 4, 
            travellingTime: 10, 
            totalCost: 35, 
            currency: "USD" 
          },
          { 
            id: 4, 
            name: "Central Fitness", 
            address: "123 Central Ave, Chicago, IL", 
            date: "5 Nov 2024", 
            overallRating: 4, 
            timeRating: null, 
            costRating: 3.5, 
            travellingTime: 25, 
            totalCost: 20, 
            currency: "USD" 
          },
          { 
            id: 5, 
            name: "West End Gym", 
            address: "789 West End St, Chicago, IL", 
            date: "4 Nov 2024", 
            overallRating: null, 
            timeRating: null, 
            costRating: 4, 
            travellingTime: 30, 
            totalCost: 40, 
            currency: "USD" 
          },
        ];

        setRecommendations(dummyRecommendations);
      } catch (err) {
        setError(err.message || "An error occurred while fetching data.");
      } finally {
        setLoading(false);
      }
    };

    fetchRecommendations();
  }, []);

  // Separate gyms with complete and incomplete data
  const completeDataGyms = recommendations.filter(gym => 
    gym.overallRating && gym.timeRating && gym.costRating && gym.travellingTime && gym.totalCost
  );

  const incompleteDataGyms = recommendations.filter(gym => 
    !gym.overallRating || !gym.timeRating || !gym.costRating || !gym.travellingTime || !gym.totalCost
  );

  return (
    <CoordinatesProvider>
      <div className="recommendation-container">
        <aside className="recommendation-lists">
          <div className="recommendation-list">
            <h3>Top 5 Recommended Gyms (Complete Data)</h3>
            {loading ? (
              <p>Loading recommendations...</p>
            ) : error ? (
              <p>Error: {error}</p>
            ) : completeDataGyms.length > 0 ? (
                <table className="gym-table">
                <thead>
                  <tr>
                    <th>Gym Name</th>
                    <th>Overall Rating</th>
                    <th>Time Rating</th>
                    <th>Cost Rating</th>
                    <th>Travelling Time (min)</th>
                    <th>Total Cost</th>
                    <th>Address</th>
                  </tr>
                </thead>
                <tbody>
                  {completeDataGyms.slice(0, 5).map((gym) => (
                    <tr key={gym.id}>
                      <td>{gym.name}</td>
                      <td>{gym.overallRating}</td>
                      <td>{gym.timeRating}</td>
                      <td>{gym.costRating}</td>
                      <td>{gym.travellingTime}</td>
                      <td>{gym.totalCost} {gym.currency}</td>
                      <td className="address">{gym.address}</td> 
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : (
              <p>No complete data available.</p>
            )}
          </div>

          <div className="recommendation-list">
            <h3>Top 3 Recommended Gyms (Incomplete Data)</h3>
            {loading ? (
              <p>Loading recommendations...</p>
            ) : error ? (
              <p>Error: {error}</p>
            ) : incompleteDataGyms.length > 0 ? (
              <table className="gym-table">
                <thead>
                  <tr>
                    <th>Gym Name</th>
                    <th>Overall Rating</th>
                    <th>Time Rating</th>
                    <th>Cost Rating</th>
                    <th>Travelling Time (min)</th>
                    <th>Total Cost</th>
                    <th>Address</th>
                  </tr>
                </thead>
                <tbody>
                  {incompleteDataGyms.slice(0, 3).map((gym) => (
                    <tr key={gym.id}>
                      <td>{gym.name}</td>
                      <td>{gym.overallRating || "N/A"}</td>
                      <td>{gym.timeRating || "N/A"}</td>
                      <td>{gym.costRating || "N/A"}</td>
                      <td>{gym.travellingTime || "N/A"}</td>
                      <td>{gym.totalCost || "N/A"} {gym.currency || "N/A"}</td>
                      <td className="address">{gym.address}</td> 
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : (
              <p>No gyms with incomplete data.</p>
            )}
          </div>
        </aside>
        <MapSection />
      </div>
    </CoordinatesProvider>
  );
};

export default Recommendation;
