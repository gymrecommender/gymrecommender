import {useState, useEffect} from "react";
import Button from "../simple/Button.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faClose} from "@fortawesome/free-solid-svg-icons";
import {displayTimestamp} from "../../services/helpers.jsx";
import {useCoordinates} from "../../context/CoordinatesProvider.jsx";

const GymRequested = ({showMarker}) => {
	const {coordinates} = useCoordinates();
	const [requests, setRequests] = useState([]);
	useEffect(() => {
		//TODO retrieve pending requests
		setRequests([{
				"id": "uuid11",
				"requestedAt": "2024-11-07T08:15:30+00:00",
				"respondedAt": "2024-11-07T09:00:45+00:00",
				"decision": "approved",
				"message": null,
				"gym": {
					"name": "Lakeside Fitness",
					"address": "456 Lakeview Ave, Chicago, IL",
					"latitude": 41.881832,
					"longitude": -87.623177
				}
			},
			{
				"id": "uuid12",
				"requestedAt": "2024-11-06T14:45:00+05:30",
				"respondedAt": "2024-11-06T15:30:00+05:30",
				"decision": "rejected",
				"message": "Request rejected due to insufficient documentation.",
				"gym": {
					"name": "Uptown Strength",
					"address": "2101 N Halsted St, Chicago, IL",
					"latitude": 41.920784,
					"longitude": -87.648333
				}
			},
			{
				"id": "uuid13",
				"requestedAt": "2024-11-07T01:10:15-05:00",
				"respondedAt": null,
				"decision": null,
				"message": "Your request is being reviewed.",
				"gym": {
					"name": "South Loop Performance",
					"address": "1325 S State St, Chicago, IL",
					"latitude": 41.865666,
					"longitude": -87.627579
				}
			},
			{
				"id": "uuid14",
				"requestedAt": "2024-11-06T14:45:00+05:30",
				"respondedAt": "2024-11-06T15:30:00+05:30",
				"decision": "rejected",
				"message": "Request rejected due to insufficient documentation.",
				"gym": {
					"name": "River North Athletic Club",
					"address": "500 W Erie St, Chicago, IL",
					"latitude": 41.894512,
					"longitude": -87.642197
				}
			},
			{
				"id": "uuid14",
				"requestedAt": "2024-11-06T14:45:00+05:30",
				"respondedAt": "2024-11-06T15:30:00+05:30",
				"decision": "rejected",
				"message": "Request rejected due to insufficient documentation.",
				"gym": {
					"name": "River North Athletic Club",
					"address": "500 W Erie St, Chicago, IL",
					"latitude": 41.894512,
					"longitude": -87.642197
				}
			},
			{
				"id": "uuid14",
				"requestedAt": "2024-11-06T14:45:00+05:30",
				"respondedAt": "2024-11-06T15:30:00+05:30",
				"decision": "rejected",
				"message": "Request rejected due to insufficient documentation.",
				"gym": {
					"name": "River North Athletic Club",
					"address": "500 W Erie St, Chicago, IL",
					"latitude": 41.894512,
					"longitude": -87.642197
				}
			},
			{
				"id": "uuid14",
				"requestedAt": "2024-11-06T14:45:00+05:30",
				"respondedAt": "2024-11-06T15:30:00+05:30",
				"decision": "rejected",
				"message": "Request rejected due to insufficient documentation.",
				"gym": {
					"name": "River North Athletic Club",
					"address": "500 W Erie St, Chicago, IL",
					"latitude": 41.894512,
					"longitude": -87.642197
				}
			},
		])
	}, []);

	const handleDelete = (id) => {
		//TODO implement deletion logic
		alert(`Request ${id} is cancelled`);
	}

	const content = requests?.map((item) => {
		const isSelected = coordinates?.lat === item.gym.latitude && coordinates?.lng === item.gym.longitude;
		return (
			<div className={`gym-req ${isSelected ? 'gym-req-selected' : ''}`} onClick={() => showMarker({lat: item.gym.latitude, lng: item.gym.longitude})}>
				<div className={"gym-req-header"}>
					<span className={"gym-req-reqtime"}>{displayTimestamp(item.requestedAt, true)}</span>
					<Button type={"button"} className={"btn-icon"} onClick={() => handleDelete(item.id)}>
						<FontAwesomeIcon className={"icon"} size={"lg"} icon={faClose}/>
					</Button>
				</div>
				<div className={"gym-req-body"}>
					<span className={"gym-req-title"}>{item.gym.name}</span>
					<span className={"gym-req-address"}>{item.gym.address}</span>
				</div>
			</div>
		)
	})
	return (
		<div className={"gyms-req"}>
			{content}
		</div>
	)
}

export default GymRequested;