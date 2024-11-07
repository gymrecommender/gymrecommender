import {useState} from "react";
import Input from "../../components/simple/Input.jsx";
import Map from "../../components/simple/Map.jsx";
import LocationControls from "../../components/LocationControls.jsx";
import Button from "../../components/simple/Button.jsx";

const AccountGymRequest = () => {
	const [lat, setLat] = useState(0);
	const [lon, setLon] = useState(0);

	const handleSubmitSearch = (value) => {
		console.log(value);
	};

	const handleGetLocation = () => {
		console.log("Get location");
		// Add logic to get current location here
	};

	return (
		<div className="section section-request">
			<h3>Request to Own a GymEdit:</h3>
			<form className="request-form">
				<div className="form-row">
					<div className="form-item">
						<label htmlFor="gym-name">GymEdit Name:</label>
						<Input
							type="text"
							name="gym-name"
							placeholder="Enter gym name"
							className="input-box"
							required
						/>
					</div>
					<div className="form-item">
						<label htmlFor="gym-location">GymEdit Location:</label>
						<LocationControls onGetLocation={handleGetLocation} onSubmitSearch={handleSubmitSearch}/>
						<Map coordinates={{lon, lat}}/>
						<Button type="submit" className="btn-submit">Submit Request</Button>
					</div>
				</div>
			</form>
		</div>
	)
}

export default AccountGymRequest