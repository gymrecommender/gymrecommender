import GymRequest from "./pages/GymRequest.jsx";
import History from "./pages/History.jsx";
import {useFirebase} from "./context/FirebaseProvider.jsx";
import {Navigate, Route, Routes} from "react-router-dom";
import NotFound from "./pages/NotFound.jsx";
import GymManagement from "./pages/GymManagement.jsx";
import Account from "./pages/Account.jsx";
import UserRating from "./pages/UserRating.jsx";
import AdminRequests from "./pages/AdminRequests.jsx";

const roles = {
	gym: {
		routes: [
			{path: "management", component: GymManagement},
			{path: "management/add", component: GymRequest}
		],
		defaultComponent: Account
	},
	user: {
		routes: [
			{path: "rating", component: UserRating},
			{path: "history", component: History}
		],
		defaultComponent: Account
	},
	admin: {
		routes: [
			{path: "requests", component: AdminRequests}
		],
		defaultComponent: Account
	}
}

const RoleBasedRoutes = () => {
	const {getUser} = useFirebase();
	const user = getUser();

	if (!user) {
		return <Navigate to={'/login'}/>;
	}

	const roleRoutes = roles[user.role]
	if (!roleRoutes) {
		return <Navigate to={'/'}/>;
	}

	const {routes, defaultComponent: DefaultComponent} = roleRoutes;
	return (
		<Routes>
			{routes.map(({path, component: Component}) => (
				<Route
					key={path}
					path={path}
					element={<Component/>}
				/>
			))}
			<Route index element={<DefaultComponent/>}/>
			<Route path={"*"} element={<NotFound/>}/> //TODO make sure that this absolutely must be duplicated in this component
		</Routes>
	)
}

export default RoleBasedRoutes