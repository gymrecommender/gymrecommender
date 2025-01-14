import {Routes, Route, Navigate} from "react-router-dom";
import Index from "./pages/Index";
import NotFound from "./pages/NotFound.jsx";
import Auth from "./pages/Auth.jsx";
import Main from "./layouts/Main.jsx";
import RoleBasedRoutes from "./RoleBasedRoutes.jsx";
import {useLoader} from "./context/LoaderProvider.jsx";
import {useFirebase} from "./context/FirebaseProvider.jsx";
import Loader from "./components/simple/Loader.jsx";

const App = () => {
	const {loader} = useLoader();
	const {getLoading, getUser} = useFirebase();
	return (
		<>
			{!getLoading() ?
				<Routes>
					<Route path="/" element={<Main/>}>
						<Route index element={<Index/>}/>
						{/*TODO make the route the same for all the components + add conditional rendering of Account pages*/}
						{<Route path='account/:username/*' element={<RoleBasedRoutes/>}/>}
						<Route path={"*"} element={<NotFound/>}/>
					</Route>

					{
						getUser()?.username ?
							<>
								<Route path={"/login/*"} element={<Navigate to="/"/>}/>
								<Route path={"/signup/*"} element={<Navigate to="/"/>}/>
							</> :
							<>
								<Route path={"/login"}>
									<Route index element={<Auth/>}/>
									<Route path={"gym"} element={<Auth/>}/> //TODO gyms accounts must be created by
									admins
									<Route path={"admin"} element={<Auth/>}/>
								</Route>
								<Route path={"/signup"}>
									<Route index element={<Auth/>}/>
									<Route path={"gym"} element={<Auth/>}/>
								</Route>
							</>
					}
				</Routes> : <Loader/>}

			{loader ? <Loader/> : ""}
		</>
	)
}

export default App
