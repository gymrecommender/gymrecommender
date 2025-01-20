import React, {useEffect, useState} from "react";
import Button from "../components/simple/Button";
import Modal from "../components/simple/Modal";
import "../styles/main.css";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faUser, faLock, faTrash} from "@fortawesome/free-solid-svg-icons";
import {axiosInternal} from "../services/axios.jsx";
import {toast} from "react-toastify";
import Form from "../components/simple/Form.jsx";
import Loader from "../components/simple/Loader.jsx";
import {useFirebase} from "../context/FirebaseProvider.jsx";

const Account = () => {
	const [isPasswordModalOpen, setIsPasswordModalOpen] = useState(false);
	const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
	const [account, setAccount] = useState({});
	const [loader, setLoader] = useState(false);
	const {logout} = useFirebase();
	const [formFields, setFormFields] = useState({
		fields: [
			{pos: 1, type: "text", label: [<FontAwesomeIcon icon={faUser}/>, 'First Name'], name: "firstName"},
			{pos: 2, type: "text", label: [<FontAwesomeIcon icon={faUser}/>, 'Last Name'], name: "lastName"},
			{pos: 3, type: "email", readOnly: true, label: [<FontAwesomeIcon icon={faUser}/>, 'Email'], name: "email"},
			{
				pos: 4,
				type: "text",
				readOnly: true,
				label: [<FontAwesomeIcon icon={faUser}/>, 'Username'],
				name: "username"
			},
		],
		wClassName: "form-group",
		button: {
			type: "submit",
			text: "Save",
			className: "btn-submit"
		}
	});

	useEffect(() => {
		const retrieveInformation = async () => {
			const result = await axiosInternal("GET", "account");
			if (result.error) toast(result.error.message);
			else {
				const fields = formFields.fields.map((field) => {
					return {...field, value: result.data[field.name]};
				})
				setFormFields({...formFields, fields});
				setAccount(result.data);
			}
		}

		retrieveInformation();
	}, []);

	const handleSubmit = async (values) => {
		const {firstName, lastName, ...rest} = values;

		const result = await axiosInternal("PUT", "account", {firstName, lastName});
		if (result.error) toast(result.error.message);
		else {
			toast("Information is successfully updated")
			setAccount(result.data);
		}
	}

	const handlePasswordSubmit = async (values) => {
		const {passwordRepeat, ...rest} = values;
		setLoader(true);
		const result = await axiosInternal("PUT", "account/password", {...rest});
		if (result.error) toast(result.error.message);
		else {
			toast("Your password has been updated successfully");
			setIsPasswordModalOpen(false);
			logout();
		}
		setLoader(false);
	};

	const handleDeleteAccount = (e) => {
		e.preventDefault();
		console.log("Account deleted");
		setIsDeleteModalOpen(false);
	};

	return (
		<div className="account-container">
			<h1>Account Settings</h1>
			{Object.keys(account).length > 0 ?
				<Form className="account-form" data={formFields} onSubmit={handleSubmit}/> : null}
			<div className={"account-actions"}>
				<Button
					type="button"
					className="btn-primary"
					onClick={() => setIsPasswordModalOpen(true)}
				>
					<FontAwesomeIcon icon={faLock}/> Change Password
				</Button>
				<Button
					type="button"
					className="btn-danger"
					onClick={() => setIsDeleteModalOpen(true)}
				>
					<FontAwesomeIcon icon={faTrash}/> Delete Account
				</Button>
			</div>

			{/* Modal for Change Password */}
			{isPasswordModalOpen && (
				<Modal
					headerText="Change Password"
					onClick={() => setIsPasswordModalOpen(false)}
				>
					<Form onSubmit={handlePasswordSubmit} data={{
						fields: [
							{pos: 1, type: "password", placeholder: "Enter your current password", required: true, label: "Current password", name: "currentPassword"},
							{pos: 2, type: "password", minLength: 6, placeholder: "Enter a new password", required: true, label: "New password", name: "newPassword"},
							{
								pos: 3,
								type: "password",
								required: true,
								minLength: 6,
								sameAs: {fieldName: "newPassword", message: "The passwords do not match"},
								label: "Repeat the password",
								placeholder: "Repeat your new password",
								name: "passwordRepeat"
							}
						],
						wClassName: "form-group",
						button: {
							type: "submit",
							text: "Change Password",
							className: "btn-submit",
						}
					}}/>
				</Modal>
			)}

			{/* Modal for Delete Account */}
			{isDeleteModalOpen && (
				<Modal
					headerText="Confirm Deletion"
					onClick={() => setIsDeleteModalOpen(false)}
				>
					<form onSubmit={handleDeleteAccount}>
						<p>To confirm, please enter your password:</p>
						<div className="form-group">
							<label htmlFor="deletePassword">Password</label>
							<input
								id="deletePassword"
								name="deletePassword"
								type="password"
								placeholder="Enter your password"
								required
							/>
						</div>
						<div className="modal-actions">
							<Button type="submit" className="btn-danger">
								Confirm
							</Button>
							<Button
								type="button"
								className="btn-secondary"
								onClick={() => setIsDeleteModalOpen(false)}
							>
								Cancel
							</Button>
						</div>
					</form>
				</Modal>
			)}
			{loader ? <Loader/> : null}
		</div>
	);
};

export default Account;
