import MapSection from "../components/MapSection.jsx";
import GymRequested from "../components/gym/GymRequested.jsx";
import {useCoordinates} from "../context/CoordinatesProvider.jsx";

const GymRequest = () => {
	const { setCoordinates } = useCoordinates();

	const showMarker = (coordinates) => {
		setCoordinates(coordinates);
	}
	return (
		<>
			<GymRequested showMarker={showMarker}/>
			<MapSection/>
		</>
	)
}

export default GymRequest