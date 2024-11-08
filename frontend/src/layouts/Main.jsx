import {Outlet, useLocation, matchPath, useParams} from "react-router-dom";
import Header from "../components/Header.jsx";
import Footer from "../components/Footer.jsx";

const Main = () => {
    const params = useParams()

    return (
        <div className="container">
            <Header username={params.username} />
            <div className={`content`}>
                <Outlet />
            </div>
            <Footer />
        </div>
    );
}

export default Main;
