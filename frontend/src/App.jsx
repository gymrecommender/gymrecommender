import {Routes, Route, Navigate} from "react-router-dom";
import Index from "./pages/Index";
import NotFound from "./pages/NotFound.jsx";
import Auth from "./pages/Auth.jsx";
import Main from "./layouts/Main.jsx";
import RoleBasedRoutes from "./RoleBasedRoutes.jsx";
import {useLoader} from "./context/LoaderProvider.jsx";
import {useFirebase} from "./context/FirebaseProvider.jsx";
import Loader from "./components/simple/Loader.jsx";
import {useConfirm} from "./context/ConfirmProvider.jsx";
import Confirm from "./components/simple/Confirm.jsx";
import TitleSetter from "./TitleSetter.jsx";


const App = () => {
	const {loader} = useLoader();
	const {getLoading, getUser} = useFirebase();
	const {data} = useConfirm();

	return (
		<>
			{!getLoading() ?
				<Routes>
					<Route path="/" element={<Main/>}>
						<Route index element={
							<TitleSetter title={"Home"}>
								<Index/>
							</TitleSetter>
						}/>
						<Route path='account/*' element={<RoleBasedRoutes/>}/>
						<Route path={"*"} element={
							<TitleSetter title={"404|Not Found"}>
								<NotFound/>
							</TitleSetter>
						}/>
					</Route>

					{
						getUser()?.username ?
							<>
								<Route path={"/login/*"} element={<Navigate to="/"/>}/>
								<Route path={"/signup/*"} element={<Navigate to="/"/>}/>
							</> :
							<>
								<Route path={"/login"}>
									<Route index element={
										<TitleSetter title={"User Login"}>
											<Auth/>
										</TitleSetter>
									}/>
									<Route path={"gym"} element={
										<TitleSetter title={"Gym Login"}>
											<Auth/>
										</TitleSetter>
									}/>
									<Route path={"admin"} element={
										<TitleSetter title={"Admin Login"}>
											<Auth/>
										</TitleSetter>
									}/>
								</Route>
								<Route path={"/signup"}>
									<Route index element={
										<TitleSetter title={"User Sign up"}>
											<Auth/>
										</TitleSetter>
									}/>
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
