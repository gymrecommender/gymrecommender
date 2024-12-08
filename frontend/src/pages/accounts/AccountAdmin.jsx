import {useState, useEffect} from "react";
import Button from "../../components/simple/Button.jsx";
import Modal from "../../components/simple/Modal.jsx";
import "../../styles/admin.css";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {
	faPlus,
	faX,
	faCheck,
	faUserGear,
	faSave,
} from "@fortawesome/free-solid-svg-icons";
import {emailRegEx} from "../../services/helpers.jsx";
import Form from "../../components/simple/Form.jsx";
import Accordion from "../../components/simple/Accordion.jsx";

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
	wClassName: "form-row",
	button: {
		type: "submit",
		text: "Create account",
		className: "btn-submit",
	}
}

const AccountAdmin = () => {
	const [showCreateAdminModal, setShowCreateAdminModal] = useState(false);
	const [groupedGyms, setGroupedGyms] = useState({});

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
					"gym_account_id_1": { requestTime: "2024-11-06T14:45:00+05:30", email: "thebestgym@gmail.com"},
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

	const handleOnSubmit = (values, gymId, accountId) => {
		console.log(values, gymId, accountId);
	}
	const handleAccountSubmit = (values) => {
		console.log(values);
	}

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
					Object.keys(groupedGyms)?.map((gymId, index) => {
						const {name, address, requests} = groupedGyms[gymId];
						return <Accordion onSubmit={handleOnSubmit} gymId={gymId} key={index} name={name} address={address} requests={requests}/>
					})
				}
			</section>

			{/* Use Modal component for the Create Admin form */}
			{showCreateAdminModal && (
				<Modal onClick={handleCloseModal} headerText="New Admin Account">
					<Form className="admin-form" data={accountData} onSubmit={handleAccountSubmit}/>
				</Modal>
			)}
		</div>
	);
};

export default AccountAdmin;




			
