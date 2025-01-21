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
import {axiosInternal} from "../services/axios.jsx";
import {accountSignUp} from "../services/accountHelpers.jsx";
import Loader from "../components/simple/Loader.jsx";

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
	const [loader, setLoader] = useState(false);
	const {setValues, flushData} = useConfirm();

	useEffect(() => {
		const retrieveOwnershipRequests = async () => {
			const result = await axiosInternal("GET", `adminaccount/requests`);
			if (result.error) toast(result.error.message);
			else setGroupedGyms(result.data)
		}

		retrieveOwnershipRequests();
	}, [])

	const handleShowModal = () => setShowCreateAdminModal(true);
	const handleCloseModal = () => setShowCreateAdminModal(false);

	const onAccept = async (gymId, acceptedGymRequestId, acceptedMessage) => {
		flushData();
		const copy = {...groupedGyms}

		await Promise.all(Object.keys(groupedGyms[gymId].requests).map(async (gymRequestId) => {
			const isAccepted = gymRequestId === acceptedGymRequestId;
			const result = await axiosInternal("PUT", `adminaccount/requests/${gymRequestId}`, {
				message: isAccepted ? acceptedMessage : "The request has been declined",
				decision: isAccepted ? "approved" : "rejected"
			});

			if (result.error) toast(result.error.message);
			else delete copy[gymId].requests[gymRequestId];
		}))

		if (Object.keys(copy[gymId].requests).length === 0) {
			toast(`Responses for '${copy[gymId].name}' has been successfully submitted`)
			delete copy[gymId];
		}
		setGroupedGyms(copy);
	}

	const handleOnSubmit = async (values, gymId, requestId) => {
		const copy = {...groupedGyms}
		const changedRequests = copy[gymId].requests;
		const isComplete = values.status && values.message;

		if (isComplete && values.status === "approved") {
			if (Object.keys(changedRequests).length > 1) {
				setValues(
					true,
					`Accepting this ownership request for '${copy[gymId].name}' will automatically reject all the other ownership requests for this gym. Continue?`,
					() => onAccept(gymId, requestId, values.message),
					flushData
				)
			} else await onAccept(gymId, requestId, values.message);
			return;
		}

		const result = await axiosInternal("PUT", `adminaccount/requests/${requestId}`, {
			message: values.message,
			decision: values.status
		});
		if (result.error) toast(result.error.message);
		else {
			if (isComplete) delete changedRequests[requestId];
			else changedRequests[requestId].message = result.data.message;
			if (Object.keys(copy[gymId].requests).length === 0) delete copy[gymId];

			setGroupedGyms(copy);
			toast("The response has been saved successfully")
		}
	}

	const handleAccountSubmit = async (values) => {
		const {passwordRepeat, ...rest} = values;
		setLoader(true)
		const result = await axiosInternal("POST", `adminaccount/create/${rest.type}`, {...rest, provider: "local"})
		if (result.error) toast(result.error.message);
		else {
			toast("The new account has been successfully created");
			setShowCreateAdminModal(false);
		}
		setLoader(false);
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

			{loader ? <Loader/> : null}
		</div>
	);
};

export default AdminRequests;