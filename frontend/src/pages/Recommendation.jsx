import React, { useState, useEffect } from "react";
import MapSection from "../components/MapSection.jsx";
import { CoordinatesProvider } from "../context/CoordinatesProvider.jsx";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faMapMarkerAlt,
  faCalendarAlt,
  faDollarSign,
  faStar,
  faRoute,
  faMoneyBillWave,
  faClock
} from "@fortawesome/free-solid-svg-icons";

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

  /*useEffect(() => {
    const fetchRecommendations = async () => {
      setLoading(true);
      setError(null);

      try {
        const response = await axiosInternal.get("/api/recommendations");
        setRecommendations(response.data);
      } catch (err) {
        setError(err.message || "An error occurred while fetching data.");
      } finally {
        setLoading(false);
      }
    };

    fetchRecommendations();
  }, []);*/

  // Separate gyms with complete and incomplete data
  const completeDataGyms = recommendations.filter(
    (gym) =>
      gym.overallRating &&
      gym.timeRating &&
      gym.costRating &&
      gym.travellingTime &&
      gym.totalCost
  );

  const incompleteDataGyms = recommendations.filter(
    (gym) =>
      !gym.overallRating ||
      !gym.timeRating ||
      !gym.costRating ||
      !gym.travellingTime ||
      !gym.totalCost
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
              <ul className="gym-list">
                {completeDataGyms.slice(0, 5).map((gym) => (
                  <li key={gym.id} className="gym-item">
                    <h4>{gym.name}</h4>
                    <p><FontAwesomeIcon icon={faMapMarkerAlt} /> {gym.address}</p>
                    <p><FontAwesomeIcon icon={faCalendarAlt} /> {gym.date}</p>
                    <p><FontAwesomeIcon icon={faStar} /> Overall Rating: {gym.overallRating}</p>
                    <p><FontAwesomeIcon icon={faClock} /> Time Rating: {gym.timeRating}</p>
                    <p><FontAwesomeIcon icon={faDollarSign} /> Cost Rating: {gym.costRating}</p>
                    <p><FontAwesomeIcon icon={faRoute} />Travelling Time: {gym.travellingTime} min</p>
                    <p><FontAwesomeIcon icon={faMoneyBillWave} /> Total Cost: {gym.totalCost} {gym.currency}</p>
                  </li>
                ))}
              </ul>
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
              <ul className="gym-list">
                {incompleteDataGyms.slice(0, 3).map((gym) => (
                  <li key={gym.id} className="gym-item">
                    <h4>{gym.name}</h4>
                    <p><FontAwesomeIcon icon={faMapMarkerAlt} /> {gym.address}</p>
                    <p><FontAwesomeIcon icon={faCalendarAlt} /> {gym.date}</p>
                    <p><FontAwesomeIcon icon={faStar} /> Overall Rating: {gym.overallRating || "N/A"}</p>
                    <p><FontAwesomeIcon icon={faClock} /> Time Rating: {gym.timeRating || "N/A"}</p>
                    <p><FontAwesomeIcon icon={faDollarSign} /> Cost Rating: {gym.costRating || "N/A"}</p>
                    <p><FontAwesomeIcon icon={faRoute} /> Travelling Time: {gym.travellingTime || "N/A"} min</p>
                    <p><FontAwesomeIcon icon={faMoneyBillWave} /> Total Cost: {gym.totalCost || "N/A"} {gym.currency || "N/A"}</p>
                  </li>
                ))}
              </ul>
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
