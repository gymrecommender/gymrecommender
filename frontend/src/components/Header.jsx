import Title from "./simple/Title.jsx";
import NavBar from "./simple/NavBar.jsx";
import Button from "./simple/Button.jsx";
import InfoSection from "./InfoSection.jsx";
import {useNavigate, useLocation} from "react-router-dom";
import logo from "../logo.png";
import React from "react";

const Header = () => {
	const navigate = useNavigate();
	const location = useLocation();

	const navigationHandler = (path) => {
		if (!location.pathname.startsWith(path)) {
			navigate(path);
		}
	}

	return (
		<header className={"header"}>
			<div className="logo">
				<img src={logo} onClick={() => navigationHandler("/")} alt="Gym Finder Logo"/>
			</div>
			<Title/>
			<NavBar/>
			<div className="auth-button">
				<Button type={"button"}
				        onClick={() => navigationHandler("/login")}
				>
					Sign Up / Login
				</Button>
			</div>
			<InfoSection/>
		</header>
	)
}

export default Header;