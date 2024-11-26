import {Routes, Route} from "react-router-dom";
import Index from "./pages/Index";
import NotFound from "./pages/NotFound.jsx";
import Auth from "./pages/Auth.jsx";
import Main from "./layouts/Main.jsx";
import RoleBasedRoutes from "./RoleBasedRoutes.jsx";
import {useFirebase} from "./context/FirebaseProvider.jsx";

const App = () => {
	const {getLoading} = useFirebase();
	//#TODO implement permissions for the page for different roles
	return (
		<>
			{!getLoading() ?
				<Routes>
					<Route path="/" element={<Main/>}>
						<Route index element={<Index/>}/>
						{/*TODO make the route the same for all the components + add conditional rendering of Account pages*/}
						<Route path='account/:username/*' element={<RoleBasedRoutes/>}/>
						<Route path={"*"} element={<NotFound/>}/>
					</Route>
					<Route path={"/login"} element={<Auth login={true}/>}/>
					<Route path={"/signup"} element={<Auth login={false}/>}/>
				</Routes> : ''}
		</>
	)
}

export default App
