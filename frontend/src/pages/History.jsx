import React from "react";

const History = () => {
	const recommendations = [
		{ name: "Gym A", rating: 4.5, location: "Location A" },
		{ name: "Gym B", rating: 4.2, location: "Location B" },
		{ name: "Gym C", rating: 4.0, location: "Location C" },
		{ name: "Gym D", rating: 3.8, location: "Location D" },
		{ name: "Gym E", rating: 3.5, location: "Location E" },
	];

	return (
		<aside className="section-body">
			<h3> These are your past recommendations: </h3>
			<ul className="recommendation-list">
				{recommendations.map((gym, index) => (
					<li key={index} className="recommendation-item">
						<h4>{gym.name}</h4>
						<p>Rating: {gym.rating}</p>
						<p>Location: {gym.location}</p>
					</li>
				))}
			</ul>
		</aside>
	);
}

export default History;