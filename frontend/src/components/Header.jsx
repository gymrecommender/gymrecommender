import {useState} from "react";
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

const Header = () => {
    const {getUser, logout} = useFirebase();
    const user = getUser();
    const navigate = useNavigate();
    const location = useLocation();
    const [showNotifications, setShowNotifications] = useState(false);
    const [notifications, setNotifications] = useState([
        {id: 1, message: "Your gym membership is expiring soon.", read: false},
        {id: 2, message: "New gym added near your location!", read: false},
    ]);

    const handleToggleNotifications = () => {
        setShowNotifications((prev) => !prev);
        // Mark all notifications as read
        setNotifications((prev) => prev.map((notif) => ({...notif, read: true})));
    };

    const navigationHandler = (path) => {
        if ((path === '/' && path !== location.pathname) || location.pathname !== path) {
            navigate(path);
        }
        window.scrollTo({top: 0, left: 0, behavior: "smooth"});
    };

    const unreadNotifications = notifications.some((notif) => !notif.read);

    const buttons = [
        {title: "Notifications", icon: faBell, action: handleToggleNotifications},
        {title: "Gyms", icon: faDumbbell, role: "gym", action: () => navigationHandler(`/account/${user.username}/management`)},
        {title: "Ownership requests", icon: faClipboard, role: "admin", action: () => navigationHandler(`/account/${user.username}/requests`)},
        {title: "Rate gyms", icon: faStarHalfStroke, role: "user", action: () => navigationHandler(`/account/${user.username}/rating`)},
        {title: "History", icon: faClockRotateLeft, role: "user", action: () => navigationHandler(`/account/${user.username}/history`)},
        {title: "Account", icon: faCircleUser, action: () => navigationHandler(`/account/${user.username}`)},
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
										{notifications.filter((notif) => !notif.read).length}
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
			<FontAwesomeIcon className={"icon"} size={"lg"} icon={faRightToBracket} />
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
                    {notifications.length > 0 ? (
                        notifications.map((notif) => (
                            <div key={notif.id} className={`notification ${notif.read ? "read" : "unread"}`}>
                                {notif.message}
                            </div>
                        ))
                    ) : (
                        <div className="notification empty">Sorry, no new notifications...</div>
                    )}
                </div>
            )}
        </header>
    );
};

export default Header;