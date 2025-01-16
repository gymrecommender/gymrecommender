import React, { useEffect, useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faClock,
  faCalendarAlt,
  faDollarSign,
  faStar,
  faChartBar,
  faFrown,
  faSearch,
  faArrowLeft,
  faArrowRight
} from "@fortawesome/free-solid-svg-icons";
import {useParams} from "react-router-dom";
import { axiosInternal } from "../services/axios";
import Button from "../components/simple/Button";

const History = () => {
  const [requests, setRequests] = useState([]);
  const [searchQuery, setSearchQuery] = useState("");
  const {username} = useParams();
  const [editingId, setEditingId] = useState(null);
  const [newName, setNewName] = useState("");
  const [bookmarkedGyms, setBookmarkedGyms] = useState([]);

  // Actual useEffect for fetching data from backend
  /*useEffect(() => {
    const fetchRequests = async () => {
      const { data, error } = await axiosInternal("GET", "requests");
      if (error) {
        console.error("Error fetching requests:", error);
        setRequests([]); // Handle errors gracefully
      } else {
        setRequests(data || []);
      }
    };

    fetchRequests();
  }, []);*/

  /*useEffect(() => {
    const fetchBookmarkedGyms = async () => {
      try {
        const { data } = await axiosInternal("GET", `/users/${username}/bookmarked-gyms`);
        setBookmarkedGyms(data || []);
      } catch (error) {
        console.error("Error fetching bookmarked gyms:", error);
      }
    };
  
    fetchBookmarkedGyms();
  }, [username]);*/
  

  // Dummy data preview/styling purposes, delete when implementing backend
  useEffect(() => {
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
        id: 4, // Edge case
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
  }, []); 

  // Dummy data for bookmarked gyms
  useEffect(() => {
    const dummyGyms = [
      {
        id: 1,
        name: "Fitness Hub",
        address: "123 Main Street, Cityville",
        email: "contact@fitnesshub.com",
        workingHours: "6 AM - 10 PM",
        website: "www.fitnesshub.com",
        monthly: "$50",
        sixMonths: "$270",
        yearly: "$500",
      },
      {
        id: 2,
        name: "Iron Paradise",
        address: "456 Elm Street, Townsville",
        email: "info@ironparadise.com",
        workingHours: "5 AM - 11 PM",
        website: "www.ironparadise.com",
        monthly: "$60",
        sixMonths: "$320",
        yearly: "$600",
      },
      {
        id: 3,
        name: "Fitness Barn",
        address: "125 Main Street, Cityville",
        email: "contact@fitnessbarn.com",
        workingHours: "6 AM - 11 PM",
        website: "www.fitnessbarn.com",
        monthly: "$50",
        sixMonths: "$270",
        yearly: "$500",
      },
      {
        id: 4,
        name: "Iron Legs",
        address: "459 Elm Street, Townsville",
        email: "info@ironlegs.com",
        workingHours: "4 AM - 11 PM",
        website: "www.ironlegs.com",
        monthly: "$60",
        sixMonths: "$320",
        yearly: "$600",
      },
      {
        id: 5,
        name: "Fitness joy",
        address: "765 Main Street, Cityville",
        email: "contact@fitnessjoy.com",
        workingHours: "7 AM - 10 PM",
        website: "www.fitnessjoy.com",
        monthly: "$50",
        sixMonths: "$270",
        yearly: "$500",
      },
      {
        id: 6,
        name: "Sparta",
        address: "467 Elm Street, Townsville",
        email: "info@sparta.com",
        workingHours: "6 AM - 11 PM",
        website: "www.sparta.com",
        monthly: "$60",
        sixMonths: "$320",
        yearly: "$600",
      },
    ];

    setBookmarkedGyms(dummyGyms);
  }, []);


const handleRequestClick = (id) => {
  const url = `/account/${username}/history/recommendations/${id}`;
  // Open in a new tab
  window.open(url, '_blank', 'noopener,noreferrer');
};

const handleEditClick = (id, currentName) => {
  setEditingId(id);
  setNewName(currentName);
};

const handleSave = async (id) => {
  setRequests((prevRequests) =>
    prevRequests.map((request) =>
      request.id === id ? { ...request, name: newName } : request
    )
  );
  setEditingId(null); // Exit edit mode
};

const filteredRequests = requests.filter((request) =>
  request.name.toLowerCase().includes(searchQuery.toLowerCase())
);

const handleDemarkGym = async (gymId) => {
  try {
    await axiosInternal("DELETE", `/users/${username}/bookmarked-gyms/${gymId}`);
    setBookmarkedGyms((prev) => prev.filter((gym) => gym.id !== gymId));
  } catch (error) {
    console.error("Error removing gym from bookmarks:", error);
    alert("Failed to remove gym. Please try again.");
  }
};


const scrollLeft = () => {
  const container = document.getElementById("gym-cards-container");
  container.scrollBy({ left: -300, behavior: "smooth" });
};

const scrollRight = () => {
  const container = document.getElementById("gym-cards-container");
  container.scrollBy({ left: 300, behavior: "smooth" });
};



  return (
    <section className="section">
      <h2 style={{ color: "#ffffff" }}>Your Past Requests:</h2>
      <div className="search-bar">
        <FontAwesomeIcon icon={faSearch} className="icon"/>
        <input
          type="text"
          placeholder="Search requests..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
        />
      </div>
      {filteredRequests.length > 0 ? (
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
          {filteredRequests.map((request) => (
              <tr
                key={request.id}
                onClick={(e) => {
                  if (!e.target.closest(".edit-section")) {
                    handleRequestClick(request.id);
                  }
                }}
                style={{ cursor: "pointer" }}
              >
                <td>
                  {editingId === request.id ? (
                    <div className="edit-section" onClick={(e) => e.stopPropagation()}>
                      <input
                        type="text"
                        value={newName}
                        onChange={(e) => setNewName(e.target.value)}
                        onKeyDown={(e) => {
                          if (e.key === "Enter") handleSave(request.id);
                        }}
                      />
                     <Button
                      onClick={(e) => {
                        e.stopPropagation();
                        handleSave(request.id);
                      }}
                      className="save-btn"> Save </Button>
                    </div>
                  ) : (
                    <span>
                      {request.name || "Unnamed Request"}
                      <Button
                        onClick={(e) => {
                          e.stopPropagation();
                          handleEditClick(request.id, request.name);
                        }}
                        className="edit-btn"
                      >
                        Edit
                      </Button>
                    </span>
                  )}
                </td>
                <td>{request.requestTime || "N/A"}</td>
                <td>{request.preferences?.departureTime || "N/A"}</td>
                <td>{request.preferences?.arrivalTime || "N/A"}</td>
                <td>{request.preferences?.minPrice || "N/A"}</td>
                <td>{request.preferences?.minRating || "N/A"}</td>
                <td>{request.preferences?.minCongestion || "N/A"}</td>
                <td>{request.preferences?.timePriceRatio || "N/A"}</td>
                <td>{request.preferences?.membershipLength || "N/A"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : (
        <div className="no-requests">
          <FontAwesomeIcon icon={faFrown} />
          <p>Oops! You don't have any requests yet.</p>
		  <p>Why not start by making your first one? We promise it won't bite!</p>
        </div>
      )}

<h2 style={{ color: "#ffffff", marginTop: "40px" }}>Bookmarked Gyms:</h2>
      {bookmarkedGyms.length > 0 ? (
        <div className="bookmarked-gyms-section">
          <div id="gym-cards-container" className="bookmarked-gyms-container">
            {bookmarkedGyms.map((gym) => (
              <div key={gym.id} className="bookmarked-gyms-card">
                <h3>{gym.name}</h3>
                <p>
                  <strong>Address:</strong> {gym.address}
                </p>
                <p>
                  <strong>Email:</strong> {gym.email}
                </p>
                <p>
                  <strong>Working Hours:</strong> {gym.workingHours}
                </p>
                <p>
                  <strong>Website:</strong>{" "}
                  <a
                    href={`https://${gym.website}`}
                    target="_blank"
                    rel="noopener noreferrer"
                  >
                    {gym.website}
                  </a>
                </p>
                  <div className="membership-info">
                    <strong>Memberships:</strong>
                    <ul>
                      <li>Monthly: {gym.monthly}</li>
                      <li>6-Months: {gym.sixMonths}</li>
                      <li>Yearly: {gym.yearly}</li>
                    </ul>
                  </div>
                  <button
                    className="demark-button"
                    onClick={() => handleDemarkGym(gym.id)}>
                    Remove Bookmark
                  </button>
              </div>
            ))}
          </div>
          <div className="arrow-buttons">
            <button className="scroll-button" onClick={scrollLeft}>
              <FontAwesomeIcon icon={faArrowLeft} />
            </button>
            <button className="scroll-button" onClick={scrollRight}>
              <FontAwesomeIcon icon={faArrowRight} />
            </button>
          </div>
        </div>
      ) : (
        <div className="no-bookmarked-gyms">
          <FontAwesomeIcon icon={faFrown} className="icon"/>
          <p>You haven't bookmarked any gyms yet!</p>
        </div>
      )}
    </section>
  );
};

export default History;
