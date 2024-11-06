import LocationControls from "./LocationControls.jsx";
import Map from "./simple/Map.jsx";
import {useState} from "react";

const MapSection = () => {
	//#TODO these variables must be within the Map component (use context to share these variables with each other)
	const [lon, setLon] = useState(0)
	const [lat, setLat] = useState(0)

	const handleSubmitSearch = (value) => {
		console.log(value)
	}
	const handleGetLocation = () => {
		console.log("Get location")
		//#TODO call the function that will retrieve current location
	}
	return (
		<div className={"main"}>
			<LocationControls onSubmitSearch={handleSubmitSearch} onGetLocation={handleGetLocation}/>
			<Map coordinates={{lon, lat}}/>
		</div>
	);
};

export default MapSection;
