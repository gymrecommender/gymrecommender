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
						name={`message-${id}`} // Assign a unique name per row
						placeholder="Enter a message"
						value={editableMessages[id] || ""}
						onChange={(name, value) => handleInputChange(id, value)} // Use `value` directly
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
					<form className="admin-form">
						<div className="form-row">
							<label htmlFor="username">Username:</label>
							<Input type="text" id="username" name="username" required/>
						</div>
						<div className="form-row">
							<label htmlFor="email">Email:</label>
							<Input type="email" id="email" name="email" required/>
						</div>
						<div className="form-row">
							<label htmlFor="first_name">First Name:</label>
							<Input type="text" id="first_name" name="first_name" required/>
						</div>
						<div className="form-row">
							<label htmlFor="last_name">Last Name:</label>
							<Input type="text" id="last_name" name="last_name" required/>
						</div>
						<div className="form-row">
							<label htmlFor="provider">Provider:</label>
							<select id="provider" name="provider" required>
								<option value="local">Local</option>
								<option value="google">Google</option>
							</select>
						</div>
						<div className="form-row">
							<label htmlFor="password">Password:</label>
							<Input type="password" id="password" name="password" required/>
						</div>
						<div className="form-row">
							<label htmlFor="type">Account Type:</label>
							<select id="type" name="type" required>
								<option value="admin">Admin</option>
								<option value="user">User</option>
								<option value="gym">Gym</option>
							</select>
						</div>
						<div className="form-row">
							<label htmlFor="created_by">Created By:</label>
							<Input type="text" id="created_by" name="created_by" placeholder="Creator's UUID" required/>
						</div>
						<Button type="submit" className="btn-submit">Create Admin</Button>
					</form>
				</Modal>
			)}
		</div>
	);
};

export default AccountAdmin;




			
