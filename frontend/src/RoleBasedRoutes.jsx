import GymRequest from "./pages/GymRequest.jsx";
import History from "./pages/History.jsx";
import {useFirebase} from "./context/FirebaseProvider.jsx";
import {Navigate, Route, Routes} from "react-router-dom";
import NotFound from "./pages/NotFound.jsx";
import GymManagement from "./pages/GymManagement.jsx";
import Account from "./pages/Account.jsx";
import UserRating from "./pages/UserRating.jsx";
import AdminRequests from "./pages/AdminRequests.jsx";
import TitleSetter from "./TitleSetter.jsx";
import Recommendation from "./pages/Recommendation.jsx";

const roles = {
	gym: {
		routes: [
			{path: "management", component: GymManagement, title: "Managed gyms"},
			{path: "management/request", component: GymRequest, title: "Request management"},
		],
		title: "My account - Gym",
		defaultComponent: Account
	},
	user: {
		routes: [
			{path: "rating", component: UserRating, title: "Rating gyms"},
			{path: "history", component: History, title: "Recommendations history"},
			{path: "history/recommendations/:id", component: Recommendation, title: "Recommendations History"}
		],
		title: "My account - User",
		defaultComponent: Account
	},
	admin: {
		routes: [
			{path: "requests", component: AdminRequests, title: "Ownership requests"}
		],
		title: "My account - Admin",
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

	const {routes, title: indexTitle, defaultComponent: DefaultComponent} = roleRoutes;
	return (
		<Routes>
			{routes.map(({path, title: routeTitle, component: Component}) => (
				<Route
					key={path}
					path={path}
					element={
						<TitleSetter title={routeTitle}>
							<Component/>
						</TitleSetter>
					}
				/>
			))}
			<Route index element={
				<TitleSetter title={indexTitle}>
					<DefaultComponent/>
				</TitleSetter>
			}/>
			<Route path={"*"} element={
				<TitleSetter title={"404|Not Found"}>
					<NotFound/>
				</TitleSetter>
			}/> //TODO make sure that this absolutely must be duplicated in this
			component
		</Routes>
	)
}

export default RoleBasedRoutes