import {useEffect, useState} from "react";
import "../styles/header.css";
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
	faBell,
	faDumbbell,
	faStarHalfStroke,
	faClipboard
} from "@fortawesome/free-solid-svg-icons";
import {useFirebase} from "../context/FirebaseProvider.jsx";
import {axiosInternal} from "../services/axios.jsx";
import {toast} from "react-toastify";

const Header = () => {
	const {getUser, logout} = useFirebase();
	const user = getUser();
	const navigate = useNavigate();
	const location = useLocation();
	const [showNotifications, setShowNotifications] = useState(false);
	const [notifications, setNotifications] = useState([]);

	useEffect(() => {
		if (getUser()?.role === "user") {
			const retrieveNotifications = async () => {
				const result = await axiosInternal("GET", "useraccount/notifications");
				if (result.error) toast(result.error.message);
				else setNotifications(result.data);
			}

			retrieveNotifications()
		}
	}, []);

	const handleToggleNotifications = async () => {
		setShowNotifications((prev) => !prev);
		// Mark all notifications as read
		await Promise.all(notifications.filter(not => !not.isRead).map(async (notification) => {
			await axiosInternal("PUT", `useraccount/notifications/${notification.id}`);
		}))

		setNotifications((prev) => prev.map((notif) => ({...notif, isRead: true})));
	};

	const navigationHandler = (path) => {
		if ((path === '/' && path !== location.pathname) || location.pathname !== path) {
			navigate(path);
		}
		window.scrollTo({top: 0, left: 0, behavior: "smooth"});
	};

	const unreadNotifications = notifications.some((notif) => !notif.isRead);

	const buttons = [
		{title: "Notifications", icon: faBell, role: "user", action: handleToggleNotifications},
		{title: "Gyms", icon: faDumbbell, role: "gym", action: () => navigationHandler(`/account/management`)},
		{
			title: "Ownership requests",
			icon: faClipboard,
			role: "admin",
			action: () => navigationHandler(`/account/requests`)
		},
		{title: "Rate gyms", icon: faStarHalfStroke, role: "user", action: () => navigationHandler(`/account/rating`)},
		{title: "History", icon: faClockRotateLeft, role: "user", action: () => navigationHandler(`/account/history`)},
		{title: "Account", icon: faCircleUser, action: () => navigationHandler(`/account`)},
		{title: "Log out", icon: faRightFromBracket, action: () => logout()},
	];

	const authButton = user ? (
		<>
			{buttons.map(({action, title, icon, role}) => {
				if (!role || role === user.role) {
					return (
						<div className="notification-container" key={title}>
							<Button
								type={"btn"}
								title={title}
								className={`btn-panel btn-icon ${title === "Notifications" && unreadNotifications ? "unread" : ""}`}
								onClick={action}
							>
								<FontAwesomeIcon
									className={"icon"}
									size={"lg"}
									icon={icon}
								/>
								{title === "Notifications" && unreadNotifications && (
									<span className="notification-badge">
										{notifications.filter((notif) => !notif.isRead).length}
									</span>
								)}

							</Button>
						</div>
					);
				}
			})}
		</>
	) : (
		<Button
			type={"button"}
			className={"btn-panel btn-icon"}
			title={"Log in"}
			onClick={() => navigationHandler("/login")}
		>
			<FontAwesomeIcon className={"icon"} size={"lg"} icon={faRightToBracket}/>
		</Button>
	);


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
			{showNotifications && (
				<div className="notifications-dropdown">
					{notifications.length > 0 ?
						(
							notifications.map((notif) => (
								<div key={notif.id} className={`notification ${notif.read ? "read" : "unread"}`}>
									{notif.message}
								</div>
							))
						) : (
							<div className="notification no-content">You do not have any notification</div>
						)}
				</div>
			)}
		</header>
	);
};

export default Header;