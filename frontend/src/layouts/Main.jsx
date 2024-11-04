import { Outlet, useLocation, matchPath } from "react-router-dom";
import Header from "../components/Header.jsx";
import Footer from "../components/Footer.jsx";

const Main = () => {
    const location = useLocation();
    
    // Determine if the path is for history or AccountGym to apply specific CSS classes
    const isHistory = matchPath({ path: "/account/:username/history" }, location.pathname);
    const isAccountGym = matchPath({ path: "/account/:username/gym" }, location.pathname);
	const isAccountAdmin = matchPath({ path: "/account/:username/admin" }, location.pathname);

    return (
        <div className="container">
            <Header />
            <div className={`content ${isHistory ? 'content-history' : ''} ${isAccountGym ? 'content-account-gym' : ''} ${isAccountAdmin ? 'content-account-admin' : ''}`}>
                <Outlet />
            </div>
            <Footer />
        </div>
    );
}

export default Main;
