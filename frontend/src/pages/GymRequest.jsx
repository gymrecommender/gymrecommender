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

const GymRequest = () => {
	const {coordinates, setCoordinates} = useCoordinates();
	const [markers, setMarkers] = useState([]);
	const [loading, setLoading] = useState(false);
	const {requests, setRequests} = useMarkersOwnership();

	const handleOnClick = (gym) => {
		//TODO store the request, store the response of the POST function in the requests array
		const data = {
			"id": Math.floor(Math.random() * 100),
			"requestedAt": "2024-11-07T08:15:30+00:00",
			"respondedAt": null,
			"decision": null,
			"message": null,
			"gym": {
				"name": gym.name,
				"address": gym.address,
				"latitude": gym.latitude,
				"longitude": gym.longitude,
			}
		}

		setRequests([...requests, data])
	}

	useEffect(() => {
		//TODO retrieve pending requests
		// setRequests([
		// 	{
		// 		"id": "uuid11",
		// 		"requestedAt": "2024-11-07T08:15:30+00:00",
		// 		"respondedAt": "2024-11-07T09:00:45+00:00",
		// 		"decision": "approved",
		// 		"message": null,
		// 		"gym": {
		// 			"name": "Lakeside Fitness",
		// 			"address": "456 Lakeview Ave, Chicago, IL",
		// 			"latitude": 41.881832,
		// 			"longitude": -87.623177
		// 		}
		// 	},
		// 	{
		// 		"id": "uuid12",
		// 		"requestedAt": "2024-11-06T14:45:00+05:30",
		// 		"respondedAt": "2024-11-06T15:30:00+05:30",
		// 		"decision": "rejected",
		// 		"message": "Request rejected due to insufficient documentation.",
		// 		"gym": {
		// 			"name": "Uptown Strength",
		// 			"address": "2101 N Halsted St, Chicago, IL",
		// 			"latitude": 41.920784,
		// 			"longitude": -87.648333
		// 		}
		// 	},
		// 	{
		// 		"id": "uuid13",
		// 		"requestedAt": "2024-11-07T01:10:15-05:00",
		// 		"respondedAt": null,
		// 		"decision": null,
		// 		"message": "Your request is being reviewed.",
		// 		"gym": {
		// 			"name": "South Loop Performance",
		// 			"address": "1325 S State St, Chicago, IL",
		// 			"latitude": 41.865666,
		// 			"longitude": -87.627579
		// 		}
		// 	},
		// 	{
		// 		"id": "uuid187",
		// 		"requestedAt": "2024-11-06T14:45:00+05:30",
		// 		"respondedAt": "2024-11-06T15:30:00+05:30",
		// 		"decision": "rejected",
		// 		"message": "Request rejected due to insufficient documentation.",
		// 		"gym": {
		// 			"name": "River North Athletic Club",
		// 			"address": "500 W Erie St, Chicago, IL",
		// 			"latitude": 41.894512,
		// 			"longitude": -87.642197
		// 		}
		// 	},
		// 	{
		// 		"id": "uuid1",
		// 		"requestedAt": "2024-11-06T14:45:00+05:30",
		// 		"respondedAt": "2024-11-06T15:30:00+05:30",
		// 		"decision": "rejected",
		// 		"message": "Request rejected due to insufficient documentation.",
		// 		"gym": {
		// 			"name": "River North Athletic Club",
		// 			"address": "500 W Erie St, Chicago, IL",
		// 			"latitude": 41.894512,
		// 			"longitude": -87.642197
		// 		}
		// 	},
		// 	{
		// 		"id": "uuid14",
		// 		"requestedAt": "2024-11-06T14:45:00+05:30",
		// 		"respondedAt": "2024-11-06T15:30:00+05:30",
		// 		"decision": "rejected",
		// 		"message": "Request rejected due to insufficient documentation.",
		// 		"gym": {
		// 			"name": "River North Athletic Club",
		// 			"address": "500 W Erie St, Chicago, IL",
		// 			"latitude": 41.894512,
		// 			"longitude": -87.642197
		// 		}
		// 	},
		// 	{
		// 		"id": "uuid15",
		// 		"requestedAt": "2024-11-06T14:45:00+05:30",
		// 		"respondedAt": "2024-11-06T15:30:00+05:30",
		// 		"decision": "rejected",
		// 		"message": "Request rejected due to insufficient documentation.",
		// 		"gym": {
		// 			"name": "River North Athletic Club",
		// 			"address": "500 W Erie St, Chicago, IL",
		// 			"latitude": 41.894512,
		// 			"longitude": -87.642197
		// 		}
		// 	},
		// ])
	}, []);

	useEffect(() => {
		const getGyms = async () => {
			setLoading(true);
			const result = await axiosInternal("GET", "/gym/location", {}, {lat: coordinates.lat, lng: coordinates.lng});
			if (result.data.error) {
				toast(result.error.message);
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