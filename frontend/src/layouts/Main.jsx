import {Outlet, useLocation, matchPath, useParams} from "react-router-dom";
import Header from "../components/Header.jsx";
import Footer from "../components/Footer.jsx";

const Main = () => {
    const location = useLocation();
    const params = useParams()
    
    // Determine if the path is for history or AccountGym to apply specific CSS classes
    const match = params.username && location.pathname !== '/';

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
