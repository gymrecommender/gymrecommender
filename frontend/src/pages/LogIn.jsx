import '../styles/login.css'
import Button from "../components/simple/Button.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faUserPlus, faRightToBracket} from "@fortawesome/free-solid-svg-icons";
import React from "react";
import {useLocation, useNavigate} from "react-router-dom";
import Form from "../components/simple/Form.jsx";

const LogIn = () => {
	const location = useLocation();
	const navigate = useNavigate();
	const isLogin = location.pathname === "/login";

	const getFormValues = (values) => {
		console.log(values)
	}

	const data = {
		fields: [
			...(!isLogin ?
				[
					{pos: 1, type: "text", label: "First name", name: "firstName"},
					{pos: 2, type: "text", label: "Last name", name: "lastName"},
					{pos: 3, type: "text", required: true, label: "Username", name: "username"},
					{pos: 6, type: "password", required: true, label: "Repeat the password", name: "passwordRepeat"},
				] : []),
			{pos: 4, type: "email", required: true, label: "Email", name: "email"},
			{pos: 5, type: "password", required: true, label: "Password", name: "password"},
		],
		fieldClass: "input-login",
		wClassName: "form-group",
		button: {
			type: "submit",
			text: isLogin ? "Login" : "Register",
			className: "btn-login",
		}
	};
	return (
		<div className="container-login">
			<div className="form-login">
				<div className="login-header">
					<h2 className={"login-title"}>{isLogin ? "Login" : "Sign up"}</h2>
					<Button onClick={() => navigate(`${isLogin ? '/signup' : '/login'}`)} type={"button"}
					        title={isLogin ? "Sign up" : "Login"} className={"btn-icon btn-action"}>
						<FontAwesomeIcon className={"icon"} size={"lg"}
						                 icon={isLogin ? faUserPlus : faRightToBracket}/>
					</Button>
				</div>
				<Form data={data} onSubmit={getFormValues} />
			</div>
		</div>
	)
}

export default LogIn;