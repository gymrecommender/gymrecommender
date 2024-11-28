import React, { useState } from "react";
import Input from "../../components/simple/Input.jsx";
import Button from "../../components/simple/Button.jsx";
import Modal from "../../components/simple/Modal.jsx";
import "../../styles/admin.css";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faPlus,
  faX,
  faCheck,
  faUserGear,
  faSave,
} from "@fortawesome/free-solid-svg-icons";
import {emailRegEx} from "../../services/helpers.jsx";
import Form from "../../components/simple/Form.jsx";

const accountTypes = [
	{value: "gym", label: "Gym"},
	{value: "admin", label: "Admin"},
]
const data = {
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
  const [editableMessages, setEditableMessages] = useState({
    123: "",
    456: "",
    789: "",
  });

  const handleShowModal = () => setShowCreateAdminModal(true);
  const handleCloseModal = () => setShowCreateAdminModal(false);

  const handleInputChange = (id, value) => {
	setEditableMessages((prev) => ({ ...prev, [id]: value }));
  };

  const handleSaveMessage = (id) => {
    console.log(`Saved message for ID ${id}:`, editableMessages[id]);
    // call an API
  };
	const handleSubmit = (values) => {
		console.log(values);
	}

  return (
    <div className="section-body">
      <div className="body_header">
        <h2>Gym Ownership Requests</h2>
        <Button onClick={handleShowModal} className="btn-show-modal">
          <FontAwesomeIcon className={"icon"} size={"2x"} icon={faPlus} />
          <FontAwesomeIcon className={"icon"} size={"2x"} icon={faUserGear} />
        </Button>
      </div>
      <section className="gym-requests">
        <table className="requests-table">
          <thead>
            <tr>
              <th>Gym Name / ID</th>
              <th>Requester Email</th>
              <th>Address</th>
              <th>Status</th>
              <th>Message</th>
              <th>Request Time</th>
              <th>Response Time</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {/* Dynamic table rows */}
            {[123, 456, 789].map((id) => (
              <tr key={id}>
                <td>Gym / ID {id}</td>
                <td>requester{id}@example.com</td>
                <td>Sample Address</td>
                <td>approved</td>
                <td>
					<div className="input-with-icon">
						<Input
						type="text"
						name={`message-${id}`}
						placeholder="Enter a message"
						value={editableMessages[id] || ""}
						onChange={(name, value) => handleInputChange(id, value)}
						/>

						<Button
						className="btn-icon save"
						onClick={() => handleSaveMessage(id)}
						>
						<FontAwesomeIcon
							className={"icon"}
							size={"lg"}
							icon={faSave}
						/>
						</Button>
					</div>
                </td>
                <td>7 Nov 2024, 9:15:30 CET</td>
                <td>13 Nov 2024, 10:15:30 CET</td>
                <td>
                  <Button className="btn-icon approve" aria-label="Approve">
                    <FontAwesomeIcon className={"icon"} size={"lg"} icon={faCheck} />
                  </Button>
                  <Button className="btn-icon reject" aria-label="Reject">
                    <FontAwesomeIcon className={"icon"} size={""} icon={faX} />
                  </Button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

     {/* Use Modal component for the Create Admin form */}
			{showCreateAdminModal && (
				<Modal onClick={handleCloseModal} headerText="New Admin Account">
					<Form className="admin-form" data={data} onSubmit={handleSubmit}/>
				</Modal>
			)}
		</div>
	);
};

export default AccountAdmin;




			
