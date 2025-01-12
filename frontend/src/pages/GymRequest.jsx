import MapSection from "../components/MapSection.jsx";
import GymRequested from "../components/gym/GymRequested.jsx";
import {useCoordinates} from "../context/CoordinatesProvider.jsx";
import {useEffect, useState} from "react";
import {axiosInternal} from "../services/axios.jsx";
import {toast} from "react-toastify";
import {forRatings, ownedGyms} from "../services/markers.jsx";
import Loader from "../components/simple/Loader.jsx";
import GymPopup from "../components/gym/GymPopup.jsx";
import Button from "../components/simple/Button.jsx";

const GymRequest = () => {
	const {coordinates, setCoordinates} = useCoordinates();
	const [markers, setMarkers] = useState([]);
	const [loading, setLoading] = useState(false);

	const showMarker = (coordinates) => {
		setCoordinates(coordinates);
	}

	const handleOnClick = (gym) => {
		console.log("clicked")
	}

	useEffect(() => {
		const getGyms = async () => {
			setLoading(true);
			const result = await axiosInternal("GET", "/gym/location", {}, {lat: coordinates.lat, lng: coordinates.lng});
			if (result.data.error) {
				toast(result.error.message);
				return;
			}

			setMarkers(result.data.value.map((gym) => {
				const markerType = gym.isOwned ? ownedGyms : forRatings;
				return ({
					lat: gym.latitude,
					lng: gym.longitude,
					...markerType,
					id: gym.id,
					infoWindow: <GymPopup gym={gym}>
						{!gym.isOwned ?
						<Button type={"submit"} className={"btn btn-submit"} onClick={() => handleOnClick(gym)}>Request ownership</Button> : null}
					</GymPopup>,
				})
			}, []))

			setLoading(false);
		}

		if (coordinates.lat && coordinates.lng) getGyms();
	}, [coordinates])

	return (
		<>
			<GymRequested showMarker={showMarker}/>
			<MapSection markers={markers} showStartMarker={false} forceClick={true}/>
			{loading ? <Loader type={"hover"}/> : null}
		</>
	)
}

export default GymRequest