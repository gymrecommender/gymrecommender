import {useState, useEffect} from "react";
import Button from "../simple/Button.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faClose} from "@fortawesome/free-solid-svg-icons";
import {displayTimestamp} from "../../services/helpers.jsx";
import {useCoordinates} from "../../context/CoordinatesProvider.jsx";
import classNames from "classnames";
import {toast} from "react-toastify";
import {useConfirm} from "../../context/ConfirmProvider.jsx";
import {useMarkersOwnership} from "../../context/MarkersOwnershipProvider.jsx";
import {axiosInternal} from "../../services/axios.jsx";

const GymRequested = ({}) => {
	const {coordinates} = useCoordinates();
	const {requests, setRequests} = useMarkersOwnership();
	const {setValues, flushData} = useConfirm();

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

	const content = requests.length > 0 ? requests.map((item) => {
		const isSelected = coordinates?.lat === item.gym.latitude && coordinates?.lng === item.gym.longitude;
		return (
			<div key={item.id} className={classNames('gym-req', isSelected ? 'gym-req-selected' : '')}>
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
	}) : <div className={"no-content"}>
		You have no pending ownership requests
	</div>;
	return (
		<div className={"gyms-req"}>
			{content}
		</div>
	)
}

export default GymRequested;