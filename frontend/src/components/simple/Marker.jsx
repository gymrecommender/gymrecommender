import {AdvancedMarker, Pin, InfoWindow, useAdvancedMarkerRef} from "@vis.gl/react-google-maps";
import {useState} from "react";

const Marker = ({lat, lng, background, borderColor, glyphColor, infoWindow}) => {
	const [markerRef, marker] = useAdvancedMarkerRef();
	const [showPopup, setShowPopup] = useState(false);

	return <AdvancedMarker ref={markerRef} onClick={() => setShowPopup(true)} position={{lat, lng}}>
		<Pin
			{...(background && {background})}
			{...(borderColor && {borderColor})}
			{...(glyphColor && {glyphColor})}
		/>
		{showPopup ? <InfoWindow anchor={marker} onClose={() => setShowPopup(false)}>
			<div>
				{infoWindow ?? "Current position"}
			</div>
		</InfoWindow> : null}
	</AdvancedMarker>
}

export default Marker;