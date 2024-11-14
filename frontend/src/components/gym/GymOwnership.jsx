import {useState, useEffect} from "react";
import {displayTimestamp} from '../../services/helpers.jsx'
import Button from "../simple/Button.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faTrashCan} from "@fortawesome/free-solid-svg-icons";

const GymOwnership = ({}) => {
	const [requests, setRequests] = useState([]);
	useEffect(() => {
		//TODO retrieve all the requests for the current gym
		setRequests([
			{
				"id": "uuid11",
				"requestedAt": "2024-11-07T08:15:30+00:00",
				"respondedAt": "2024-11-07T09:00:45+00:00",
				"decision": "approved",
				"message": null,
				"gym": {
					"name": "Mountain Valley",
					"address": "123 Mountain Rd, Boulder, CO"
				}
			},
			{
				"id": "uuid12",
				"requestedAt": "2024-11-06T14:45:00+05:30",
				"respondedAt": "2024-11-06T15:30:00+05:30",
				"decision": "rejected",
				"message": "Request rejected due to insufficient documentation.",
				"gym": {
					"name": "Lakeside Fitness",
					"address": "456 Lakeview Ave, Chicago, IL"
				}
			},
			{
				"id": "uuid13",
				"requestedAt": "2024-11-07T01:10:15-05:00",
				"respondedAt": null,
				"decision": null,
				"message": "Your request is being reviewed.",
				"gym": {
					"name": "Urban Core Gym",
					"address": "789 Downtown St, New York, NY"
				}
			}
		])
	}, []);

	const handleDelete = (id) => {
		//TODO handle delete logic
		alert(`${id} is deleted`);
	}

	const content = (
		<table>
			<thead>
			<tr className={"gym-own-hd"}>
				<th className={"gym-own-hd-item"}>
					Gym
				</th>
				<th className={"gym-own-hd-item"}>
					Address
				</th>
				<th className={"gym-own-hd-item"}>
					Status
				</th>
				<th className={"gym-own-hd-item"}>
					Message
				</th>
				<th className={"gym-own-hd-item"}>
					Time of the request
				</th>
				<th className={"gym-own-hd-item"}>
					Time of the response
				</th>
			</tr>
			</thead>
			<tbody>
			{requests?.map(item => {
				return (
					<tr key={item.id} className={"gym-own-req"}>
						<td className={"gym-own-req-item"}>
							{item.gym.name}
						</td>
						<td className={"gym-own-req-item left"}>
							{item.gym.address}
						</td>
						<td className={"gym-own-req-item"}>
							{item.decision ?? 'pending'}
						</td>
						<td className={"gym-own-req-item left"}>
							{item.message}
						</td>
						<td className={"gym-own-req-item"}>
							{displayTimestamp(item.requestedAt)}
						</td>
						<td className={"gym-own-req-item"}>
							{displayTimestamp(item.respondedAt)}
						</td>
						<td className={"gym-own-req-item "}>
							{
								!item.decision ? (
									<Button title={"Delete the row"} className={"btn-icon"} onClick={() => handleDelete(item.id)}>
										<FontAwesomeIcon className={"icon"} size={"xs"} icon={faTrashCan}/>
									</Button>
								) : ''
							}
						</td>
					</tr>
				)
			})}
			</tbody>
		</table>
	)
	return (
		<div className={"gym-own"}>
			{content}
		</div>
	);
}

export default GymOwnership;