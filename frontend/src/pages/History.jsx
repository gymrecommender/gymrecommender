import React, { useEffect, useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faClock,
  faCalendarAlt,
  faDollarSign,
  faStar,
  faChartBar,
} from "@fortawesome/free-solid-svg-icons";

const History = () => {
  const [requests, setRequests] = useState([]);

  //we will you uncomment this and replace it with out API point once we have backend
  /*useEffect(() => {
	const fetchRequests = async () => {
	  try {
		const response = await fetch("/api/requests");
		const data = await response.json();
		setRequests(data);
	  } catch (error) {
		console.error("Error fetching requests:", error);
		setRequests([]); //error handling
	  }
	};
  
	fetchRequests();
  }, []);*/
  

  useEffect(() => {
    // Simulation of fetching from backend
    const fetchRequests = async () => {
      const dummyData = [
        {
          id: 1,
          requestTime: "2025-01-09 14:30",
          name: "Evening Workout",
          preferences: {
            departureTime: "17:00",
            arrivalTime: "18:00",
            minPrice: 50,
            minRating: 4,
            minCongestion: 2,
            timePriceRatio: 60,
            membershipLength: "1 month",
          },
        },
        {
          id: 2,
          requestTime: "2025-01-08 10:15",
          name: "Morning Session",
          preferences: {
            departureTime: "08:00",
            arrivalTime: "09:00",
            minPrice: 30,
            minRating: 3.5,
            minCongestion: 1,
            timePriceRatio: 70,
            membershipLength: "6 months",
          },
        },
        {
          id: 3,
          requestTime: "2025-01-07 18:00",
          name: "Late Night Cardio",
          preferences: {
            departureTime: "22:00",
            arrivalTime: "23:00",
            minPrice: 40,
            minRating: 5,
            minCongestion: 3,
            timePriceRatio: 50,
            membershipLength: "3 months",
          },
        },
        {
          id: 4, // Border case
          requestTime: "",
          name: "",
          preferences: {
            departureTime: "",
            arrivalTime: "",
            minPrice: 0,
            minRating: 0,
            minCongestion: 0,
            timePriceRatio: 0,
            membershipLength: "",
          },
        },
      ];
      setRequests(dummyData);
    };

    fetchRequests();
  }, []);

  return (
    <section className="section">
      <h2 style={{ color: "#ffffff" }}>Your Past Requests:</h2>
      {requests.length > 0 ? (
        <table className="history-table">
          <thead>
            <tr>
              <th>
                <FontAwesomeIcon icon={faClock} /> Session Name
              </th>
              <th>
                <FontAwesomeIcon icon={faCalendarAlt} /> Request Time
              </th>
              <th>
                <FontAwesomeIcon icon={faClock} /> Preferred Departure
              </th>
              <th>
                <FontAwesomeIcon icon={faClock} /> Preferred Arrival
              </th>
              <th>
                <FontAwesomeIcon icon={faDollarSign} /> Min Price ($)
              </th>
              <th>
                <FontAwesomeIcon icon={faStar} /> Min Rating
              </th>
              <th>
                <FontAwesomeIcon icon={faChartBar} /> Min Congestion
              </th>
              <th>
                <FontAwesomeIcon icon={faChartBar} /> Time/Price Ratio (%)
              </th>
              <th>
                <FontAwesomeIcon icon={faClock} /> Membership Length
              </th>
            </tr>
          </thead>
          <tbody>
            {requests.map((request) => (
              <tr
                key={request.id}
                style={
                  request.name ? {} : { backgroundColor: "#ffd5d5", color: "#900" }
                }
              >
                <td>{request.name || "No Name Provided"}</td>
                <td>{request.requestTime || "N/A"}</td>
                <td>{request.preferences.departureTime || "N/A"}</td>
                <td>{request.preferences.arrivalTime || "N/A"}</td>
                <td>{request.preferences.minPrice || "N/A"}</td>
                <td>{request.preferences.minRating || "N/A"}</td>
                <td>{request.preferences.minCongestion || "N/A"}</td>
                <td>{request.preferences.timePriceRatio || "N/A"}</td>
                <td>{request.preferences.membershipLength || "N/A"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : (
        <p className="no-content">No requests found.</p>
      )}
    </section>
  );
};

export default History;
