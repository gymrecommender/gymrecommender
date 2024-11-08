import React from "react";

const History = () => {
	const recommendations = [
		{ name: "Mountain Valley", rating: 4.5, location: "1234 Maplewood Avenue Springfield, IL 62701, United States" },
		{ name: "Lakeside Fitness", rating: 4.2, location: "4567 Riverside Drive, Los Angeles, CA 90012, United States" },
		{ name: "Urban Core Gym", rating: 4.0, location: "789 Summit Street, New York, NY 10001, United States" },
		{ name: "Maplewood gym", rating: 3.8, location: "1234 Maplewood Avenue Springfield, IL 62701, United States" },
		{ name: "Sparta", rating: 3.5, location: "548 Spartan Avenue, TX 62701, United States" },
	];

	return (
		<aside className="section">
			<div>
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
			</div>
		</aside>
	);
}

export default History;