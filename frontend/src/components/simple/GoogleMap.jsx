import {APIProvider, Map} from "@vis.gl/react-google-maps";
import {useCoordinates} from "../../context/CoordinatesProvider.jsx";
import {useCallback, useEffect, useState} from "react";
import Marker from "./Marker.jsx";
import {calculateCenter} from "../../services/helpers.jsx";
import {startMarker} from "../../services/markers.jsx";

const startingCamera = {
	center: {lat: 45.8263845, lng: 16.0432862},
	zoom: 10,
}

//if we have markers set, then we want to just show a bunch of markers without adding new ones, removing any or using geolocation
const GoogleMap = ({markers, showStartMarker=true, forceClick=false}) => {
	const {coordinates, setCoordinates} = useCoordinates();
	const [markersData, setMarkersData] = useState([]);
	const [cameraProps, setCameraProps] = useState(startingCamera);

	useEffect(() => {
		// when there is no location changed, there is no sense in forcefully centering anything
		// !markers will make sure that we indeed use the markers statically and disable all addition, deletion and edition of any markers
		if (showStartMarker && (!markers || markers.length === 0) && coordinates.lat && coordinates.lng) {
			setCameraProps({zoom: 15, center: coordinates});

			// We want to remove the previous marker as there is no case in which we can add more than 1 marker
			const newMarker = {...coordinates, ...startMarker, id: Math.round(Math.random() * 800)}
			setMarkersData([newMarker])
		}
	}, [coordinates])

	useEffect(() => {
		if (markers && markers.length > 0) {
			setMarkersData(markers);
			const centerCoord = calculateCenter(markers);
			if (centerCoord) {
				setCameraProps({zoom: 10, center: centerCoord});
			}
		}
	}, [markers]);

	const handleCameraChange = useCallback((event) => setCameraProps(event.detail))

	const showMarkers = markersData?.map(marker => {
		return <Marker key={marker.id} {...marker} />
	})
	return (
		<section className="section section-map">
			<div className={"map"}>
				<APIProvider apiKey={import.meta.env.VITE_GOOGLE_API_KEY}>
					<Map
						{...cameraProps}
						onCameraChanged={handleCameraChange}
						mapId={"aux"}
						{...(forceClick || (!markers || markers.length === 0) ? {onClick: (e) => setCoordinates(e.detail.latLng)} : {})} //disable adding markers via clicking
						options={{
							streetViewControl: false, // Disable Street View control
							gestureHandling: "greedy",
							mapTypeControl: false,
						}}
					>
						{showMarkers}
					</Map>
				</APIProvider>
			</div>
		</section>
	)
}

export default GoogleMap;