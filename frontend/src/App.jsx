import {Routes, Route, Navigate} from "react-router-dom";
import Index from "./pages/Index";
import NotFound from "./pages/NotFound.jsx";
import Auth from "./pages/Auth.jsx";
import Main from "./layouts/Main.jsx";
import RoleBasedRoutes from "./RoleBasedRoutes.jsx";
import {useLoader} from "./context/LoaderProvider.jsx";
import {useFirebase} from "./context/FirebaseProvider.jsx";
import Loader from "./components/simple/Loader.jsx";
import AccountAdmin from "./pages/accounts/AccountAdmin.jsx";
import AccountGym from "./pages/accounts/AccountGym.jsx";
import {useConfirm} from "./context/ConfirmProvider.jsx";
import Confirm from "./components/simple/Confirm.jsx";

const App = () => {
	const {loader} = useLoader();
	const {getLoading, getUser} = useFirebase();
	const {data} = useConfirm();

	return (
		<>
			{!getLoading() ?
				<Routes>
					<Route path="/" element={<Main/>}>
						<Route index element={<Index/>}/>
						<Route path='account/:username/*' element={<RoleBasedRoutes/>}/>
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
			{data.isShow ? <Confirm/> : null}
		</>
	)
}

export default App
