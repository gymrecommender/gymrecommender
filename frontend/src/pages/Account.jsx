import React, { useState } from 'react';
import Button from '../components/simple/Button';
import Modal from '../components/simple/Modal';
import '../styles/main.css';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faUser, faEnvelope, faLock, faSave } from '@fortawesome/free-solid-svg-icons';

const Account = () => {
    const [isModalOpen, setIsModalOpen] = useState(false);

    const handleSubmit = (e) => {
        e.preventDefault();
        console.log('Form submitted');
        setIsModalOpen(true);
    };

    const closeModal = () => {
        setIsModalOpen(false);
    };

    return (
        <div className="account-container">
            <h1>Account Settings</h1>
            <form onSubmit={handleSubmit} className="account-form">
                <div className="form-group">
                    <label htmlFor="username">
                        <FontAwesomeIcon icon={faUser} /> Username
                    </label>
                    <input
                        id="username"
                        name="username"
                        type="text"
                        placeholder="Enter your username"
                    />
                </div>

                <div className="form-group">
                    <label htmlFor="email">
                        <FontAwesomeIcon icon={faEnvelope} /> Email
                    </label>
                    <input
                        id="email"
                        name="email"
                        type="email"
                        placeholder="Enter your email"
                    />
                </div>

                <div className="form-group">
                    <label htmlFor="password">
                        <FontAwesomeIcon icon={faLock} /> Password
                    </label>
                    <input
                        id="password"
                        name="password"
                        type="password"
                        placeholder="Enter a new password"
                    />
                </div>

                <div className="form-group">
                    <label htmlFor="confirmPassword">
                        <FontAwesomeIcon icon={faLock} /> Confirm Password
                    </label>
                    <input
                        id="confirmPassword"
                        name="confirmPassword"
                        type="password"
                        placeholder="Confirm your new password"
                    />
                </div>

                <Button type="submit" className="btn-primary">
                    <FontAwesomeIcon icon={faSave} /> Save Changes
                </Button>
            </form>

            {isModalOpen && (
                <Modal 
                    title="Changes Saved" 
                    message="Your account settings have been updated successfully." 
                    onClose={closeModal}
                />
            )}
        </div>
    );
};

export default Account;
