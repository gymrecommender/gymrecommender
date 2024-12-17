import {APIProvider, Map, AdvancedMarker} from "@vis.gl/react-google-maps";
import {useCoordinates} from "../../context/CoordinatesProvider.jsx";
import {useCallback, useEffect, useState} from "react";

const startingCamera = {
	center: {lat: -33.860664, lng: 151.208138},
	zoom: 11,
}
const GoogleMap = () => {
	const {coordinates, setCoordinates} = useCoordinates();
	const [cameraProps, setCameraProps] = useState(startingCamera);

	useEffect(() => {
		if (coordinates.lat && coordinates.lng) {
			setCameraProps({...cameraProps, center: coordinates});
		}
	}, [coordinates])

	const onMapClick = (event) => {
		setCoordinates(event.detail.latLng);
	}

	const handleCameraChange = useCallback((event) => setCameraProps(event.detail))

	return (
		<section className="section section-map">
			<div className={"map"}>
				<APIProvider apiKey={import.meta.env.VITE_GOOGLE_API_KEY}>
					<Map
						{...cameraProps}
						onCameraChanged={handleCameraChange}
						mapId={"aux"}
						onClick={onMapClick}
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