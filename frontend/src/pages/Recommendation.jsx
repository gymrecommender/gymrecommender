import React from "react";
import { useParams } from "react-router-dom";

const Recommendation = () => {
  const { id } = useParams();

  return (
    <div className="recommendation">
      <h2>Recommendations for Request ID: {id}</h2>
      <p>Recommendations will be displayed here.</p>
    </div>
  );
};

export default Recommendation;
