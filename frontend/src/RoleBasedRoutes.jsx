import AccountGymRequest from "./pages/accounts/AccountGymRequest.jsx";
import AccountGym from "./pages/accounts/AccountGym.jsx";
import History from "./pages/History.jsx";
import AccountUser from "./pages/accounts/AccountUser.jsx";
import AccountAdmin from "./pages/accounts/AccountAdmin.jsx";
import {useFirebase} from "./context/FirebaseProvider.jsx";
import {Navigate, Route, Routes} from "react-router-dom";
import NotFound from "./pages/NotFound.jsx";

const roles = {
	gym: {
		routes: [
			{path: "add", component: AccountGymRequest}
		],
		defaultComponent: AccountGym
	},
	user: {
		routes: [
			{path: "history", component: History}
		],
		defaultComponent: AccountUser
	},
	admin: {
		routes: [],
		defaultComponent: AccountAdmin
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