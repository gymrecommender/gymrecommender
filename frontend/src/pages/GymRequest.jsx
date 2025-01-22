import MapSection from "../components/MapSection.jsx";
import GymRequested from "../components/gym/GymRequested.jsx";
import {useCoordinates} from "../context/CoordinatesProvider.jsx";
import {useEffect, useState} from "react";
import {axiosInternal} from "../services/axios.jsx";
import {toast} from "react-toastify";
import {forRatings, ownedGyms, pendingGyms} from "../services/markers.jsx";
import Loader from "../components/simple/Loader.jsx";
import GymPopup from "../components/gym/GymPopup.jsx";
import Button from "../components/simple/Button.jsx";
import {useMarkersOwnership} from "../context/MarkersOwnershipProvider.jsx";
import {result} from "lodash";

const GymRequest = () => {
	const {coordinates, setCoordinates} = useCoordinates();
	const [markers, setMarkers] = useState([]);
	const [loading, setLoading] = useState(false);
	const {requests, setRequests} = useMarkersOwnership();

	const handleOnClick = async (gym) => {
		const result = await axiosInternal("POST", `gymaccount/ownership/${gym.id}`);
		if (result.error) toast(result.error.message);
		else setRequests([...requests, result.data])
	}

	useEffect(() => {
		const retrievePendingRequests = async () => {
			const result = await axiosInternal("GET", `gymaccount/ownership`, {}, {type: "unanswered"});
			if (result.error) toast(result.error.message);
			else setRequests(result.data)
		}

		retrievePendingRequests();
	}, []);

	useEffect(() => {
		const getGyms = async () => {
			setLoading(true);
			const result = await axiosInternal("GET", "gym/location", {}, {lat: coordinates.lat, lng: coordinates.lng});
			if (result.data.error) {
				toast(result.error.message);
				setLoading(false)
				return;
			}

			setMarkers(result.data.value.map((gym) => {
				const isInRequests = requests.some(gymRequest =>
					gymRequest.gym.latitude === gym.latitude && gymRequest.gym.longitude === gym.longitude
				);

				let markerType = forRatings;
				if (gym.isOwned) markerType = ownedGyms
				else if (isInRequests) markerType = pendingGyms

				return ({
					lat: gym.latitude,
					lng: gym.longitude,
					...markerType,
					id: gym.id,
					infoWindow: <GymPopup gym={gym}>
						{!gym.isOwned && !isInRequests ?
						<Button type={"submit"} className={"btn btn-submit"} onClick={() => handleOnClick(gym)}>Request ownership</Button> : null}
						{isInRequests ? "You have requested the ownership" : null}
					</GymPopup>,
				})
			}, []))

			setLoading(false);
		}

		if (coordinates.lat && coordinates.lng) getGyms();
	}, [coordinates, requests]);

	return (
		<>
			<GymRequested/>
			<MapSection markers={markers} showStartMarker={false} forceClick={true}/>
			{loading ? <Loader type={"hover"}/> : null}
		</>
	)
}

export default GymRequest