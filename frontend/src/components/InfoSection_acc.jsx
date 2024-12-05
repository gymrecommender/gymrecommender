const InfoSection = () => {
	return (
		<section className="section">
			<div className="how-it-works">
				<h3>How It Works</h3>
				<p>On this page, you can recommend gyms and provide ratings based on your experience. Pinpoint your
					location or a gym you'd like to rate, fill out the dropdown menus with your preferences, and submit
					your recommendation!</p>
			</div>

			<div className="contact-info">
				<h3>Contact Us</h3>
				<p>Email: support@gymfinder.com</p>
				<p>Phone: (385) 456-7890</p>
			</div>

			<div className="user-tips">
				<h3>User Tips</h3>
				<p>To get the most out of Gym Finder, consider the following:</p>
				<ul>
					<li>Check the congestion ratings during peak hours.</li>
					<li>Use the travel time slider to find gyms within a reasonable distance.</li>
					<li>Be honest in your ratings to help others make informed decisions.</li>
				</ul>
			</div>
		</section>
	);
};

export default InfoSection;
  