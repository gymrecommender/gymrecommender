import LocationControls from "./LocationControls.jsx";
import GoogleMap from "./simple/GoogleMap.jsx";
import {getLocation} from "../services/helpers.jsx";
import {useCoordinates} from "../context/CoordinatesProvider.jsx";

const MapSection = () => {
	//#TODO these variables must be within the GoogleMap component (use context to share these variables with each other)
	const { setCoordinates } = useCoordinates();

	const handleSubmitSearch = (value) => {
		console.log(value)
	}
	const handleGetLocation = async () => {
		//getLocation has to be an async function since the flow goes further after executing a getCurrentLocation function
		//hence, to get the result we need to wait for the execution of that function to be finished
		const result = await getLocation()
		if (result.error) {
			alert(result.error.toString()) //TODO the error should be displayed in the pop up or sth
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
			<GoogleMap />
		</div>
	);
};

export default MapSection;
