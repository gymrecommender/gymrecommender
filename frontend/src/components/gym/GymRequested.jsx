import {useState, useEffect} from "react";
import Button from "../simple/Button.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faClose} from "@fortawesome/free-solid-svg-icons";
import {displayTimestamp} from "../../services/helpers.jsx";
import {useCoordinates} from "../../context/CoordinatesProvider.jsx";
import classNames from "classnames";
import {toast} from "react-toastify";
import {useConfirm} from "../../context/ConfirmProvider.jsx";

const GymRequested = ({showMarker}) => {
	const {coordinates} = useCoordinates();
	const [requests, setRequests] = useState([]);
	const {setValues, flushData} = useConfirm();

	useEffect(() => {
		//TODO retrieve pending requests
		setRequests([
			{
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
				"id": "uuid187",
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
				"id": "uuid1",
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
				"id": "uuid15",
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

	const onConfirm = (id, name) => {
		flushData();
		//TODO request deletion of the request from the table
		setRequests(requests?.reduce((acc, request) => {
			if (request.id !== id) {
				acc.push(request);
			}
			return acc;
		}, []))
		toast(`The ownership request for ${name} has been withdrawn`);
	}

	const content = requests?.map((item) => {
		const isSelected = coordinates?.lat === item.gym.latitude && coordinates?.lng === item.gym.longitude;
		return (
			<div key={item.id} className={classNames('gym-req', isSelected ? 'gym-req-selected' : '')} onClick={() => showMarker({lat: item.gym.latitude, lng: item.gym.longitude})}>
				<div className={"gym-req-header"}>
					<span className={"gym-req-reqtime"}>{displayTimestamp(item.requestedAt, true)}</span>
					<Button type={"button"} className={"btn-icon"} onClick={() => {
						setValues(
							true,
							`Are you sure that you want to withdraw the ownership request for '${item.gym.name}'?'`,
							() => onConfirm(item.id, item.gym.name),
							flushData
						)
					}}>
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
	console.log(content)
	return (
		<div className={"gyms-req"}>
			{content.length > 0 ? content : <div className={"no-content"}>
				You have no pending ownership requests
			</div>
			}
		</div>
	)
}

export default GymRequested;