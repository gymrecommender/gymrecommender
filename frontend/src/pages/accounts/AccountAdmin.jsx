import Input from "../../components/simple/Input.jsx";
import Button from "../../components/simple/Button.jsx";
import React from "react";

const AccountAdmin = () => {
	return (
		<div className="section">
			<h2>Create Admin Account</h2>
			<section className="create-admin">
				<form>
					<div className="form-row">
						<label htmlFor="admin-name">Admin Name:</label>
						<Input
							type="text"
							id="admin-name"
							name="admin-name"
							className="unavailable-form input-box"
							required
						/>
					</div>
					<div className="form-row">
						<label htmlFor="admin-email">Admin Email:</label>
						<Input
							type="email"
							id="admin-email"
							name="admin-email"
							className="unavailable-form input-box"
							required
						/>
					</div>
					<Button type="submit" className="btn-submit">Create Admin</Button>
				</form>
			</section>
			<section className="gym-requests">
				<h2>Gym Ownership Requests</h2>
				<div className="requests-list">
					{/* This will be populated with the list of requests */}
				</div>
				{/* Add pagination controls here */}
			</section>
		</div>
	);
};

export default AccountAdmin;