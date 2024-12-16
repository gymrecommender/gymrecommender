import {useState, useEffect} from "react";
import Button from "../components/simple/Button.jsx";
import Modal from "../components/simple/Modal.jsx";
import "../styles/admin.css";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {
	faPlus,
	faUserGear,
} from "@fortawesome/free-solid-svg-icons";
import {emailRegEx} from "../services/helpers.jsx";
import Form from "../components/simple/Form.jsx";
import AccordionRequests from "../components/simple/AccordionRequests.jsx";
import {toast} from "react-toastify";
import {useConfirm} from "../context/ConfirmProvider.jsx";

const statuses = [
	{value: "approved", label: "Approved"},
	{value: "rejected", label: "Rejected"}
]

const accountTypes = [
	{value: "gym", label: "Gym"},
	{value: "admin", label: "Admin"},
]
const accountData = {
	fields: [
		{pos: 1, type: "text", minLength: 2, label: "First name", required: true, name: "firstName"},
		{pos: 2, type: "text", label: "Last name", required: true, name: "lastName"},
		{pos: 3, type: "text", required: true, label: "Username", name: "username"},
		{
			pos: 6,
			type: "password",
			required: true,
			sameAs: {fieldName: "password", message: "The passwords do not match"},
			label: "Repeat the password",
			name: "passwordRepeat"
		},
		{
			pos: 4,
			type: "email",
			required: true,
			pattern: {regEx: emailRegEx, message: "Invalid email format"},
			label: "Email",
			name: "email"
		},
		{pos: 5, type: "password", required: true, label: "Password", name: "password"},
		{pos: 7, type: "select", required: true, label: "Account type", name: "type", data: accountTypes},
	],
	button: {
		type: "submit",
		text: "Create account",
		className: "btn-submit",
	}
}

const AdminRequests = () => {
	const [showCreateAdminModal, setShowCreateAdminModal] = useState(false);
	const [groupedGyms, setGroupedGyms] = useState({});
	const {flushData, setValues} = useConfirm();

	useEffect(() => {
		setGroupedGyms({
			"gym_uuid_1": {
				name: "Maplewood gym",
				address: "1234 Maplewood Avenue Springfield, IL 62701, United States",
				requests: {
					"gym_account_id_1": {requestTime: "2024-11-06T14:45:00+05:30", email: "thebestgym@gmail.com"},
					"gym_account_id_2": {requestTime: "2024-11-06T14:45:00+05:30", email: "nickname@gmail.com"},
					"gym_account_id_3": {requestTime: "2024-11-06T14:45:00+05:30", email: "anothergym@gmail.com"},
				}
			},
			"gym_uuid_2": {
				name: "Summit Wellness Center",
				address: "789 Summit Street, New York, NY 10001, United States",
				requests: {
					"gym_account_id_1": {requestTime: "2024-11-06T14:45:00+05:30", email: "thebestgym@gmail.com"},
					"gym_account_id_2": {requestTime: "2024-11-06T14:45:00+05:30", email: "nickname@gmail.com"},
					"gym_account_id_3": {requestTime: "2024-11-06T14:45:00+05:30", email: "anothergym@gmail.com"},
				}
			},
			"gym_uuid_3": {
				name: "Riverside Fitness Center",
				address: "4567 Riverside Drive, Los Angeles, CA 90012, United States",
				requests: {
					"gym_account_id_1": {requestTime: "2024-11-06T14:45:00+05:30", email: "thebestgym@gmail.com"},
				}
			},
		})
	}, [])

	const handleShowModal = () => setShowCreateAdminModal(true);
	const handleCloseModal = () => setShowCreateAdminModal(false);

	const onAccept = (gymId, acceptedAccountId, acceptedMessage) => {
		flushData();
		Object.keys(groupedGyms[gymId].requests).forEach(gymAccountId => {
			const data = groupedGyms[gymId].requests[gymAccountId]
			if (gymAccountId === acceptedAccountId) {
				data.message = acceptedMessage;
				data.status = statuses[0].value;
			} else {
				data.status = statuses[1].value;
			}
			// TODO save the response for the current gym and handle the response
		})

		const copy = {...groupedGyms}
		toast(`Responses for '${copy[gymId].name}' has been successfully submitted`)
		delete copy[gymId];
		setGroupedGyms(copy);
	}

	const handleOnSubmit = (values, gymId, accountId) => {
		const copy = {...groupedGyms}
		const changedRequests = copy[gymId].requests;
		const isFinish = values.status && values.message;

		changedRequests[accountId] = {
			...changedRequests[accountId],
			message: values.message,
			status: values.status
		};
		copy[gymId].requests = changedRequests;

		if (isFinish && values.status === statuses[0].value) {
			if (Object.keys(changedRequests).length > 1) {
				setValues(
					true,
					`Accepting this ownership request for '${copy[gymId].name}' will automatically reject all the other ownership requests for this gym. Continue?`,
					() => onAccept(gymId, accountId, values.message),
					flushData
				)
			} else {
				onAccept(gymId, accountId, values.message);
			}
			return;
		}

		//TODO save data in the db. Proceed if the data has been stored successfully
		if (isFinish) {
			delete changedRequests[accountId]
			if (Object.keys(copy[gymId]).length === 0) {
				delete copy[gymId];
			}
		}
		setGroupedGyms(copy);
		toast("The response has been saved successfully")
	}

	const handleAccountSubmit = (values) => {
		console.log(values);
	}

	const requests = Object.keys(groupedGyms)?.map((gymId) => {
		const {name, address, requests} = groupedGyms[gymId];
		return <AccordionRequests onSubmit={handleOnSubmit} gymId={gymId} key={gymId} name={name}
		                          address={address} statuses={statuses} requests={requests}/>
	})

	return (
		<div className="section-body">
			<div className="body-header">
				<h2>Gym Ownership Requests</h2>
				<Button onClick={handleShowModal} className="btn-icon btn-show-modal">
					<FontAwesomeIcon className={"icon"} size={"2x"} icon={faPlus}/>
					<FontAwesomeIcon className={"icon"} size={"2x"} icon={faUserGear}/>
				</Button>
			</div>
			<section className="gym-requests">
				{
					requests.length > 0 ? requests : <div className={"no-content"}>
						There are no ownership requests at the moment
					</div>
				}
			</section>

			{/* Use Modal component for the Create Admin form */}
			{showCreateAdminModal && (
				<Modal onClick={handleCloseModal} headerText="New Admin Account">
					<Form className={"modal-form"} data={accountData} onSubmit={handleAccountSubmit}/>
				</Modal>
			)}
		</div>
	);
};

export default AdminRequests;