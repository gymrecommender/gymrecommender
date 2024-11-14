import {Outlet, useLocation, matchPath, useParams} from "react-router-dom";
import Header from "../components/Header.jsx";
import Footer from "../components/Footer.jsx";
import {CoordinatesProvider} from "../context/CoordinatesProvider.jsx";

const Main = () => {
	const params = useParams()

	return (
		<div className="container">
			<Header username={params.username}/>
			<div className={`content`}>
				<CoordinatesProvider>
					<Outlet/>
				</CoordinatesProvider>
			</div>
			<Footer/>
		</div>
	);
}

export default Main;
