import {APIProvider, Map, AdvancedMarker} from "@vis.gl/react-google-maps";
import {useCoordinates} from "../../context/CoordinatesProvider.jsx";

const GoogleMap = () => {
	const {coordinates, setCoordinates} = useCoordinates();

	return (
		<section className="section section-map">
			<div className={"map"}>
				<APIProvider apiKey={import.meta.env.VITE_GOOGLE_API_KEY}>
					<Map
						defaultZoom={13}
						defaultCenter={coordinates}
						center={coordinates}
						mapId={"aux"}
						options={{
							streetViewControl: false, // Disable Street View control
							gestureHandling: "greedy"
						}}
					>
						<AdvancedMarker position={coordinates} />
					</Map>
				</APIProvider>
			</div>
		</section>
	)
}

export default GoogleMap;