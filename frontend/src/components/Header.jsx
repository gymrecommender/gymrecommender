import Menu from "./simple/Menu.jsx";
import Button from "./simple/Button.jsx";
import InfoSection_acc from "../components/simple/InfoSection_acc.jsx";
import {useNavigate, useLocation} from "react-router-dom";
import logo from "../logo.png";
import React from "react";

const Header = ({username}) => {
	const navigate = useNavigate();
	const location = useLocation();

	const navigationHandler = (path) => {
		//we do not want to register multiple instances of the same page in row in the navigation's history
		//so we use navigate only when we want to redirect to another page
		if (!location.pathname.startsWith(path)) {
			navigate(path);
		}
		//scroll to the top of the page if the requested page is the same as the current one
		window.scrollTo({top: 0, left: 0, behavior: "smooth"});
	}

	const menu = [
		{name: "Home", onClick: () => navigationHandler("/")}, //we want to have the same filter for clicking on menu buttons
		...username ? [
			{name: "Account", onClick: () => navigationHandler(`/account/${username}`)},
			{name: "History", onClick: () => navigationHandler(`/account/${username}/history`)}
		] : [] //conditional adding of the elements to the menu array. ... extracts all the elements of the subarray and adds them to the main (menu) array
	]

	//conditional rendering of the button - if we are logged in, we need a "Log out" button, not a "Sign up" one
	//#TODO the condition should be different once we implement logging in (the one that can't be changed through React Dev panel or in any other way)
	const authButton = username ?
		<Button type={"button"}
		        className={"button-logout"}
		        onClick={() => alert('Logged out')} //#TODO this should be substituted with respective logic
		> Log out </Button> :
		<Button type={"button"}
		        onClick={() => navigationHandler("/login")}
		> Sign Up / Login </Button>

	return (
		<header className={"header"}>
			<div className="logo">
				<img src={logo} onClick={() => navigationHandler("/")} alt="Gym Finder Logo"/>
			</div>
			<div className="title">
				GYM FINDER
			</div>
			<Menu data={menu}/>
			<div className="auth-button">
				{authButton}
			</div>
			<InfoSection_acc/>
		</header>
	)
}

export default Header;