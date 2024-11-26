import '../styles/login.css'
import Button from "../components/simple/Button.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faUserPlus, faRightToBracket, faHome, faDumbbell, faUserGear, faUser} from "@fortawesome/free-solid-svg-icons";
import React from "react";
import {useLocation, useMatch, useNavigate} from "react-router-dom";
import Form from "../components/simple/Form.jsx";
import {useFirebase} from "../context/FirebaseProvider.jsx";

const buttons = [
	{icon: faUser, title: (action) => `${action} as a user`, role: "user"},
	{icon: faDumbbell, title: (action) => `${action} as a gyms manager`, role: "gym"},
	{icon: faUserGear, title: (action) => `${action} as an administrator`, role: "admin"},
];

const Auth = () => {
	const location = useLocation();
	const navigate = useNavigate();

	//TODO check whether there is better way to implement these 3 rows
	const isLogin = location.pathname.startsWith("/login")
	const base = isLogin ? '/login' : '/signup';
	const titleText = isLogin ? "Login" : "Sign up";

	const match = useMatch(`/${base}/:role`);
	const role = match ? match.params.role : 'user'

	const {signUp, signIn} = useFirebase();
	const functor = isLogin ? signIn : signUp;


	const getFormValues = async (values, flushForm) => {
		const {password_repeat, ...rest} = values;
		const result = await functor(rest, role);
		if (result.error) {
			//TODO the error should be added to the specific area of the form
			alert(result.error);
		} else {
			//TODO make some information regarding the necessity to verify the email to login
			flushForm();
		}
	}

	const data = {
		fields: [
			...(!isLogin ?
				[
					{pos: 1, type: "text", label: "First name", required: true, name: "firstName"},
					{pos: 2, type: "text", label: "Last name", required: true, name: "lastName"},
					{pos: 3, type: "text", required: true, label: "Username", name: "username"},
					{pos: 6, type: "password", required: true, label: "Repeat the password", name: "passwordRepeat"},
				] : []),
			{pos: 4, type: "email", required: true, label: "Email", name: "email"},
			{pos: 5, type: "password", required: true, label: "Password", name: "password"},
		].sort(function (a, b) {
			return a.pos - b.pos;
		}),
		fieldClass: "input-login",
		wClassName: "form-group",
		button: {
			type: "submit",
			text: isLogin ? titleText : "Register",
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
					<h2 className={"login-title"}>{titleText}</h2>
					<div className={"login-buttons"}>
						{role === "admin" ? ''
							: <Button onClick={() => {
								//forms the link for the button of the respective role
								navigate(`/${isLogin ? "signup" : "login"}/${role === "user" ? "" : role}`);
								//TODO flush the form here
							}} type={"button"}
							          title={isLogin ? "Sign up" : "Log in"} className={"btn-icon btn-action"}>
								<FontAwesomeIcon className={"icon"} size={"lg"}
								                 icon={isLogin ? faUserPlus : faRightToBracket}/>
							</Button>
						}
						{
							buttons.map(({title, icon, isLogInOnly, ...rest}) => {
								//We must not show a sign-up button on admin login page
								if (role !== rest.role && !(rest.role === "admin" && !isLogin)) {
									return <Button type={"button"}
									               onClick={() => navigate(`${base}/${rest.role === "user" ? "" : rest.role}`)}
									               className={"btn-icon btn-action"} title={title(titleText)}>
										<FontAwesomeIcon className={"icon"} size={"lg"} icon={icon}/>
									</Button>
								}
							})
						}
					</div>
				</div>
				<Form data={data} onSubmit={getFormValues}/>
			</div>
		</div>
	)
}

export default Auth;