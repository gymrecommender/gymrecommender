import {useState, useEffect} from "react";
import {displayTimestamp} from '../../services/helpers.jsx'
import Button from "../simple/Button.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faTrashCan} from "@fortawesome/free-solid-svg-icons";
import {toast} from "react-toastify";
import {useConfirm} from "../../context/ConfirmProvider.jsx";
import {axiosInternal} from "../../services/axios.jsx";

const GymOwnership = ({}) => {
	const [requests, setRequests] = useState([]);
	const {setValues, flushData} = useConfirm();

	useEffect(() => {
		const retrieveOwnership = async () => {
			const result = await axiosInternal("GET", "gymaccount/ownership")

			if (result.error) toast(result.error.message);
			else setRequests(result.data);
		}

		retrieveOwnership();
	}, []);

	const onConfirm = async (id, name) => {
		flushData();
		const result = await axiosInternal("DELETE", `gymaccount/ownership/${id}`)
		if (result.error) toast(result.error.message);
		else {
			setRequests(requests?.reduce((acc, request) => {
				if (request.id !== id) {
					acc.push(request);
				}
				return acc;
			}, []))
			toast(`The ownership request for ${name} has been withdrawn`);
		}
	}

	const content = requests?.length > 0 ? (
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
									<Button title={"Cancel the ownership request"} className={"btn-icon"} onClick={() => {
										setValues(
											true,
											`Are you sure that you want to withdraw the ownership request for '${item.gym.name}'?'`,
											() => onConfirm(item.id, item.gym.name),
											flushData
										)
									}}>
										<FontAwesomeIcon className={"icon"} size={"lg"} icon={faTrashCan}/>
									</Button>
								) : ''
							}
						</td>
					</tr>
				)
			})}
			</tbody>
		</table>
	) : "You do not have any ownership requests submitted";
	return (
		<div className={"gym-own"}>
			{content}
		</div>
	);
}

export default GymOwnership;