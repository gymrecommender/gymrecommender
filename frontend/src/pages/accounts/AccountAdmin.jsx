import React, {useState} from "react";
import Input from "../../components/simple/Input.jsx";
import Button from "../../components/simple/Button.jsx";
import Modal from "../../components/simple/Modal.jsx";
import '../../styles/admin.css';

const AccountAdmin = () => {
	const [showCreateAdminModal, setShowCreateAdminModal] = useState(false);

	const handleShowModal = () => setShowCreateAdminModal(true);
	const handleCloseModal = () => setShowCreateAdminModal(false);

	return (
		<div className="section-body">
			<div className="body_header">
				<h2>Gym Ownership Requests</h2>
				<Button onClick={handleShowModal} className="btn-show-modal">+ New Admin</Button>
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
					{/* Ownership requests will be listed here, each as a table row */}
					<tr>
						<td>Mountain Valley / ID 123</td>
						<td>requester1@example.com</td>
						<td>Mountain Valley / ID 123</td>
						<td>approved</td>
						<td>No message</td>
						<td>7 Nov 2024, 9:15:30 CET</td>
						<td>13 Nov 2024, 10:15:30 CET</td>
						<td>
							<Button className="btn-icon approve" aria-label="Approve">✓</Button>
							<Button className="btn-icon reject" aria-label="Reject">✗</Button>
						</td>
					</tr>
					<tr>
						<td>Lakeside Fitness / ID 456</td>
						<td>requester2@example.com</td>
						<td>Mountain Valley / ID 123</td>
						<td>approved</td>
						<td>No message</td>
						<td>6 Nov 2024, 10:15:00 CET</td>
						<td>13 Nov 2024, 10:15:30 CET</td>
						<td>
							<Button className="btn-icon approve" aria-label="Approve">✓</Button>
							<Button className="btn-icon reject" aria-label="Reject">✗</Button>
						</td>
					</tr>
					<tr>
						<td>Urban Core Gym / ID 789</td>
						<td>requester3@example.com</td>
						<td>Mountain Valley / ID 123</td>
						<td>approved</td>
						<td>No message</td>
						<td>7 Nov 2024, 7:10:15 CET</td>
						<td>13 Nov 2024, 10:15:30 CET</td>
						<td>
							<Button className="btn-icon approve" aria-label="Approve">✓</Button>
							<Button className="btn-icon reject" aria-label="Reject">✗</Button>
						</td>
					</tr>
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
