import {APIProvider, Map} from "@vis.gl/react-google-maps";
import {useCoordinates} from "../../context/CoordinatesProvider.jsx";

const GoogleMap = () => {
	const {coordinates, setCoordinates} = useCoordinates();

	const updateCoordinates = (data) => {
		setCoordinates(data)
	}

	return (
		<section className="section section-map">
			<div className={"map"}>Google maps</div>
			{/*<APIProvider apiKey={import.meta.env.VITE_GOOGLE_API_KEY}>*/}
			{/*	<Map*/}
			{/*		defaultZoom={13}*/}
			{/*		defaultCenter={ { lat: -33.860664, lng: 151.208138 } }*/}
			{/*	/>*/}
			{/*</APIProvider>*/}
		</section>
	)
}

export default GoogleMap;