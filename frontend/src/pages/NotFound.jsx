import React from "react";
import { useNavigate } from "react-router-dom";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDumbbell, faHome } from "@fortawesome/free-solid-svg-icons";
import "../styles/notfound.css";

const NotFound = () => {
  const navigate = useNavigate();

  const goHome = () => {
    navigate("/");
  };

  return (
    <div className="not-found-container">
      <FontAwesomeIcon icon={faDumbbell} className="not-found-icon" />
      <h1 className="not-found-header">404</h1>
      <p className="not-found-message">
        Oops! The page you're looking for isn't here. Maybe it skipped leg day?
      </p>
      <button onClick={goHome} className="not-found-button">
        <FontAwesomeIcon icon={faHome} className="not-found-button-icon" /> Back to Home
      </button>
    </div>
  );
};

export default NotFound;
