import LocationControls from "./LocationControls.jsx";
import GoogleMap from "./simple/GoogleMap.jsx";
import {getLocation} from "../services/helpers.jsx";
import {useCoordinates} from "../context/CoordinatesProvider.jsx";
import {toast} from "react-toastify";

const MapSection = ({markers, showStartMarker=true, forceClick=false}) => {
	const {setCoordinates} = useCoordinates();

	const handleGetLocation = async () => {
		//getLocation has to be an async function since the flow goes further after executing a getCurrentLocation function
		//hence, to get the result we need to wait for the execution of that function to be finished
		const result = await getLocation()
		if (result.error) toast(result.error.message)
		else setCoordinates(result.data)
	}
	return (
		<div className={"section main"}>
			<LocationControls onGetLocation={handleGetLocation}/>
			<GoogleMap markers={markers} showStartMarker={showStartMarker} forceClick={forceClick}/>
		</div>
	);
};

export default MapSection;
