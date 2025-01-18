import React, { useState } from "react";
import Button from "../components/simple/Button";
import Modal from "../components/simple/Modal";
import "../styles/main.css"; // Assuming this file includes the base styles for other pages
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faUser, faLock, faTrash } from "@fortawesome/free-solid-svg-icons";

const Account = () => {
  const [isPasswordModalOpen, setIsPasswordModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);

  const handlePasswordSubmit = (e) => {
    e.preventDefault();
    console.log("Password updated");
    setIsPasswordModalOpen(false);
  };

  const handleDeleteAccount = (e) => {
    e.preventDefault();
    console.log("Account deleted");
    setIsDeleteModalOpen(false);
  };

  return (
    <div className="account-container">
      <h1>Account Settings</h1>
      <form className="account-form">
        {/* First Name Field */}
        <div className="form-group">
          <label htmlFor="firstName">
            <FontAwesomeIcon icon={faUser} /> First Name
          </label>
          <input
            id="firstName"
            name="firstName"
            type="text"
            placeholder="Enter your first name"
          />
        </div>

        {/* Last Name Field */}
        <div className="form-group">
          <label htmlFor="lastName">
            <FontAwesomeIcon icon={faUser} /> Last Name
          </label>
          <input
            id="lastName"
            name="lastName"
            type="text"
            placeholder="Enter your last name"
          />
        </div>

        {/* Email Field */}
        <div className="form-group">
          <label htmlFor="email">
            <FontAwesomeIcon icon={faUser} /> Email
          </label>
          <input
            id="email"
            name="email"
            type="email"
            value="currentEmail@example.com"
            readOnly
          />
        </div>

        {/* Change Password Button */}
        <Button
          type="button"
          className="btn-primary"
          onClick={() => setIsPasswordModalOpen(true)}
        >
          <FontAwesomeIcon icon={faLock} /> Change Password
        </Button>
      </form>

      {/* Delete Account Button */}
      <div className="delete-account-container">
        <Button
          type="button"
          className="btn-danger"
          onClick={() => setIsDeleteModalOpen(true)}
        >
          <FontAwesomeIcon icon={faTrash} /> Delete Account
        </Button>
      </div>

      {/* Modal for Change Password */}
      {isPasswordModalOpen && (
        <Modal
          headerText="Change Password"
          onClick={() => setIsPasswordModalOpen(false)}
        >
          <form onSubmit={handlePasswordSubmit}>
            <div className="form-group">
              <label htmlFor="currentPassword">Current Password</label>
              <input
                id="currentPassword"
                name="currentPassword"
                type="password"
                placeholder="Enter your current password"
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="newPassword">New Password</label>
              <input
                id="newPassword"
                name="newPassword"
                type="password"
                placeholder="Enter a new password"
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="confirmNewPassword">Confirm New Password</label>
              <input
                id="confirmNewPassword"
                name="confirmNewPassword"
                type="password"
                placeholder="Confirm your new password"
                required
              />
            </div>
            <Button type="submit" className="btn-primary">
              Save Password
            </Button>
          </form>
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
    </div>
  );
};

export default Account;
