import {Outlet, useLocation, matchPath, useParams} from "react-router-dom";
import Header from "../components/Header.jsx";
import Footer from "../components/Footer.jsx";

const Main = () => {
    const location = useLocation();
    const params = useParams()

    const match = params.username && location.pathname !== `/account/${params.username}/`;
    return (
        <div className="container">
            <Header username={params.username} />
            <div className={`content ${match ? 'content-single-column' : ''}`}>
                <Outlet />
            </div>
            <Footer />
        </div>
    );
}

export default Main;
