import LoginForm from '../components/LoginForm.jsx';
import '../styles/login.css'
import Button from "../components/simple/Button.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faUserPlus, faRightToBracket} from "@fortawesome/free-solid-svg-icons";
import Input from "../components/simple/Input.jsx";
import React from "react";
import {useState} from "react";
import {useLocation, useNavigate} from "react-router-dom";

const LogIn = () => {
	const location = useLocation();
	const navigate = useNavigate();
	const isLogin = location.pathname === "/login";

	return (
		<div className="container-login">
			<div className="form-login">
				<div className="login-header">
					<h2 className={"login-title"}>{isLogin ? "Login" : "Sign up"}</h2>
					<Button onClick={() => navigate(`${isLogin ? '/signup' : '/login'}`)} type={"button"} title={isLogin ? "Sign up" : "Login"} className={"btn-icon btn-action"}>
						<FontAwesomeIcon className={"icon"} size={"lg"}
						                 icon={isLogin ? faUserPlus : faRightToBracket}/>
					</Button>
				</div>
				<LoginForm isLogin={isLogin} />
			</div>
		</div>
	)
}

export default LogIn;