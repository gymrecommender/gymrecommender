import "../styles/header.css"
import Button from "./simple/Button.jsx";
import {useNavigate, useLocation} from "react-router-dom";
import logo from "../logo.png";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {
	faRightFromBracket,
	faRightToBracket,
	faCircleUser,
	faHome,
	faClockRotateLeft,
	faBell
} from "@fortawesome/free-solid-svg-icons";
import {useFirebase} from "../context/FirebaseProvider.jsx";

const Header = () => {
	const {getUser, logout} = useFirebase();
	const user = getUser();
	const navigate = useNavigate();
	const location = useLocation();

	const buttons = [
		{title: "Account", icon: faCircleUser, action: () => navigationHandler(`/account/${user.username}`)},
		{title: "Notifications", icon: faBell, action: () => alert("Notifications!")},
		{title: "History", icon: faClockRotateLeft, role: "user", action: () => navigationHandler(`/account/${user.username}/history`)},
		{title: "Log out", icon: faRightFromBracket, action: () => logout()},
	]

	const navigationHandler = (path) => {
		//we do not want to register multiple instances of the same page in row in the navigation's history
		//so we use navigate only when we want to redirect to another page
		if ((path === '/' && path !== location.pathname) || location.pathname !== path) {
			navigate(path);
		}
		//scroll to the top of the page if the requested page is the same as the current one
		window.scrollTo({top: 0, left: 0, behavior: "smooth"});
	}

	//conditional rendering of the button - if we are logged in, we need a "Log out" button, not a "Sign up" one
	//#TODO the condition should be different once we implement logging in (the one that can't be changed through React Dev panel or in any other way)
	const authButton = user ? (
			<>
				{
					buttons.map(({action, title, icon, role}) => {
						if (!role || role === user.role) {
							return <Button type={"btn"}
							               title={title}
							               className={"btn-panel btn-icon"}
							               onClick={action}
							>
								<FontAwesomeIcon className={"icon"} size={"lg"} icon={icon}/>
							</Button>
						}
					})
				}
			</>
		) :
		<Button type={"button"}
		        className={"btn-panel btn-icon"}
		        title={"Log in"}
		        onClick={() => navigationHandler("/login")}
		><FontAwesomeIcon className={"icon"} size={"lg"} icon={faRightToBracket}/></Button>

	return (
		<header className={"header"}>
			<div className={"header-top"}>
				<div className="home">
					<Button type={"button"}
					        title={"Go to home page"}
					        className={"btn-panel btn-icon"}
					        onClick={() => navigationHandler(`/`)}
					>
						<FontAwesomeIcon className={"icon"} size={"lg"} icon={faHome}/>
					</Button>
				</div>
				<div className="logo">
					<img src={logo} alt="GymEdit Finder Logo"/>
					<span className={"title"}>GYM FINDER</span>
				</div>
				<div className="authentication">
					{authButton}
				</div>
			</div>
		</header>
	)
}

export default Header;