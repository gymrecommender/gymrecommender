import {APIProvider, Map, AdvancedMarker} from "@vis.gl/react-google-maps";
import {useCoordinates} from "../../context/CoordinatesProvider.jsx";

const defaultStart = {lat: -33.860664, lng: 151.208138}
const GoogleMap = () => {
	const {coordinates, setCoordinates} = useCoordinates();

	return (
		<section className="section section-map">
			<div className={"map"}>
				<APIProvider apiKey={import.meta.env.VITE_GOOGLE_API_KEY}>
					<Map
						defaultZoom={13}
						defaultCenter={defaultStart}
						center={coordinates.lat ? coordinates : defaultStart}
						mapId={"aux"}
						options={{
							streetViewControl: false, // Disable Street View control
							gestureHandling: "greedy",
							mapTypeControl: false,
						}}
					>
						{coordinates.lat ? <AdvancedMarker position={coordinates} /> : null}
					</Map>
				</APIProvider>
			</div>
		</section>
	)
}

export default GoogleMap;