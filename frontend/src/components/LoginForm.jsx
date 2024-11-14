import React, {useState} from 'react';
import {useNavigate} from "react-router-dom";
import Button from "./simple/Button.jsx";
import Input from "./simple/Input.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faUserPlus} from "@fortawesome/free-solid-svg-icons";

const LoginForm = ({isLogin}) => {
	const [formValues, setFormValues] = useState({
		email: '',
		password: '',
		...(!isLogin ? {
			username: '',
			passwordRepeat: '',
			firstName: '',
			lastName: ''
		} : {})
	})
	const navigate = useNavigate();

	const handleSubmit = (e) => {
		e.preventDefault();
		// TODO: Handle login logic here

		navigate(`/account/${username}`);
	};

	const handleChange = (name, value) => {
		setFormValues({...formValues, [name]: value});
	}

	return (
		<form onSubmit={handleSubmit}>
			{!isLogin ?
				<>
					<Input type="text"
					       label={"First Name"}
					       name="firstName"
					       required
					       value={formValues["firstName"]}
					       wClassName={"form-group"}
					       className={"input-login"}
					       onChange={handleChange}/>

					<Input type="text"
					       label={"Last name"}
					       name="lastName"
					       required
					       value={formValues["lastName"]}
					       wClassName={"form-group"}
					       className={"input-login"}
					       onChange={handleChange}/>

					<Input type="text"
					       label={"Username"}
					       name="username"
					       required
					       value={formValues["username"]}
					       wClassName={"form-group"}
					       className={"input-login"}
					       onChange={handleChange}/>
				</> : ''}

			<Input type="email"
			       label={"Email"}
			       name="email"
			       required
			       value={formValues["email"]}
			       wClassName={"form-group"}
			       className={"input-login"}
			       onChange={handleChange}/>

			<Input type="password"
			       label={"Password"}
			       name="password"
			       required
			       value={formValues["password"]}
			       wClassName={"form-group"}
			       className={"input-login"}
			       onChange={handleChange}/>

			{!isLogin ?
				<>
					<Input type="password"
					       label={"Repeat Password"}
					       name="passwordRepeat"
					       required
					       value={formValues["passwordRepeat"]}
					       wClassName={"form-group"}
					       className={"input-login"}
					       onChange={handleChange}/>
				</> : ''}

			<Button className={"btn-login"} type={"submit"} onSubmit={handleSubmit}>Log in</Button>
		</form>
	);
};

export default LoginForm;
