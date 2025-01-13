import {AdvancedMarker, Pin, InfoWindow, useAdvancedMarkerRef} from "@vis.gl/react-google-maps";
import {useEffect, useState} from "react";
import {useSelectedGym} from "../../context/SelectedGymProvider.jsx";

const Marker = ({lat, lng, background, borderColor, glyphColor, id, onClick, infoWindow}) => {
	const [markerRef, marker] = useAdvancedMarkerRef();
	const [showPopup, setShowPopup] = useState(false);
	const { gymId } = useSelectedGym() || { gymId: false };
	const [scale, setScale] = useState(gymId === id ? 1.5 : 1);

	useEffect(() => {
		setScale(gymId === id ? 1.5 : 1);
	}, [gymId])

	return <AdvancedMarker ref={markerRef} onClick={() => {
		if (onClick) {
			onClick(gymId !== id)
		} else {
			setScale(1.5)
		}

		if (infoWindow) {
			setShowPopup(true)
		}
	}} position={{lat, lng}}>
		<Pin
			{...(background && {background})}
			{...(borderColor && {borderColor})}
			{...(glyphColor && {glyphColor})}
			scale={scale}
		/>
		{showPopup ? <InfoWindow anchor={marker} onClose={() => {
			if (!onClick) setShowPopup(false)
			setScale(1)
		}}>
			<div>
				{infoWindow ?? "Current position"}
			</div>
		</InfoWindow> : null}
	</AdvancedMarker>
}

export default Marker;