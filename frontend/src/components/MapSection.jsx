import LocationControls from "./LocationControls.jsx";
import GoogleMap from "./simple/GoogleMap.jsx";
import {getLocation} from "../services/helpers.jsx";
import {useCoordinates} from "../context/CoordinatesProvider.jsx";
import {toast} from "react-toastify";

//expected structure of markers
// [
// 	{
// 		lat: -33.860664,
// 		lng: 150.808138,
// 		...mainRatingMarker,
// 		id: "gym1_uuid",
// 		infoWindow: <div>This is pop up</div>,
// 	},
// 	{
// 		lat: -33.860235,
// 		lng: 151.208138,
// 		...secRatingMarket,
// 		id: "gym2_uuid",
// 		infoWindow: <div>A pop</div>,
// 	},
// 	{
// 		lat: -33.760235,
// 		lng: 151.208138,
// 		...forRatings,
// 		id: "gym3_uuid",
// 		infoWindow: <div>A pop</div>,
// 	},
// 	{
// 		lat: -33.860235,
// 		lng: 150.508138,
// 		...startMarker,
// 		id: "gym4_uuid",
// 		infoWindow: <div>A pop</div>,
// 	},
// ]
const MapSection = ({markers, showStartMarker=true, forceClick=false}) => {
	//#TODO these variables must be within the GoogleMap component (use context to share these variables with each other)
	const {setCoordinates} = useCoordinates();

	const handleSubmitSearch = (value) => {
		console.log(value)
	}
	const handleGetLocation = async () => {
		//getLocation has to be an async function since the flow goes further after executing a getCurrentLocation function
		//hence, to get the result we need to wait for the execution of that function to be finished
		const result = await getLocation()
		if (result.error) {
			toast(result.error.message) //TODO the error should be displayed in the pop up or sth
		} else {
			setCoordinates(result.data)
		}
	}
	return (
		<div className={"section main"}>
			<LocationControls onSubmitSearch={handleSubmitSearch} onGetLocation={handleGetLocation}/>
			<GoogleMap markers={markers} showStartMarker={showStartMarker} forceClick={forceClick}/>
		</div>
	);
};

export default MapSection;
