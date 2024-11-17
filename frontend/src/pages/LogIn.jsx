import '../styles/login.css'
import Button from "../components/simple/Button.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faUserPlus, faRightToBracket, faHome} from "@fortawesome/free-solid-svg-icons";
import React from "react";
import {useLocation, useNavigate} from "react-router-dom";
import Form from "../components/simple/Form.jsx";
import {useFirebase} from "../context/FirebaseProvider.jsx";

const LogIn = () => {
	const location = useLocation();
	const navigate = useNavigate();
	const isLogin = location.pathname === "/login";
	const {signUp, signIn} = useFirebase();
	const functor = isLogin ? signIn : signUp;

	const getFormValues = async (values, flushForm) => {
		const {password_repeat, ...rest} = values;
		const result = await functor(rest);
		//TODO the error should be added to the specific area of the form
		if (result.error) {
			alert(result.error);
		}

		flushForm();
	}

	const data = {
		fields: [
			...(!isLogin ?
				[
					{pos: 1, type: "text", label: "First name", required: true, name: "first_name"},
					{pos: 2, type: "text", label: "Last name", required: true, name: "last_name"},
					{pos: 3, type: "text", required: true, label: "Username", name: "username"},
					{pos: 6, type: "password", required: true, label: "Repeat the password", name: "password_repeat"},
				] : []),
			{pos: 4, type: "email", required: true, label: "Email", name: "email"},
			{pos: 5, type: "password", required: true, label: "Password", name: "password"},
		].sort(function(a, b) {return a.pos - b.pos;}),
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
					<Button onClick={() => navigate(`/`)} type={"button"}
					        title={"Home"} className={"btn-icon btn-login-home"}>
						<FontAwesomeIcon className={"icon"} size={"2x"}
						                 icon={faHome}/>
					</Button>
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