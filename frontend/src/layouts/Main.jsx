import {Outlet, useLocation, matchPath} from "react-router-dom";
import Header from "../components/Header.jsx";
import Footer from "../components/Footer.jsx";

const Main = () => {
	const location = useLocation();
	const isHistory = matchPath({ path: "/account/:username/history" }, location.pathname)

	return (
		<div className="container">
			<Header/>
			<div className={`content ${isHistory ? 'content-history' : ''}`}>
				<Outlet/>
			</div>
			<Footer/>
		</div>
	)
}

export default Main;