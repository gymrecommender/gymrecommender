import LocationControls from "./LocationControls.jsx";
import Map from "./simple/Map.jsx";
import {useState} from "react";
import {getLocation} from "../services/helpers.jsx";

const MapSection = () => {
	//#TODO these variables must be within the Map component (use context to share these variables with each other)
	const [coordinates, setCoordinates] = useState({
		lat: null,
		lng: null,
	});

	const handleSubmitSearch = (value) => {
		console.log(value)
	}
	const handleGetLocation = async () => {
		//getLocation has to be an async function since the flow goes further after executing a getCurrentLocation function
		//hence, to get the result we need to wait for the execution of that function to be finished
		const result = await getLocation()
		if (result.error) {
			alert(result.error) //TODO the error should be displayed in the pop up or sth
		} else {
			setCoordinates({
				lat: result.lat,
				lng: result.lng,
			})
		}
	}
	return (
		<div className={"section main"}>
			<LocationControls onSubmitSearch={handleSubmitSearch} onGetLocation={handleGetLocation}/>
			<Map coordinates={coordinates}/>
		</div>
	);
};

export default MapSection;
