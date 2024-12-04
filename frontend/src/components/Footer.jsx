import InfoSection from "./InfoSection.jsx";
import {pingBackend} from "../scripts/ping.js";

const Footer = () => {
	pingBackend()
	return (
		<footer className="footer">
			<InfoSection/>
			<p className="footer-note">© 2024 Gym Finder. All rights reserved.</p>
		</footer>
	);
};

export default Footer;
