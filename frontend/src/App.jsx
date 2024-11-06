import {BrowserRouter, Routes, Route} from "react-router-dom";
import Index from "./pages/Index";
import AccountUser from "./pages/AccountUser.jsx";
import NotFound from "./pages/NotFound.jsx";
import History from "./pages/History.jsx";
import LogIn from "./pages/LogIn.jsx";
import Main from "./layouts/Main.jsx";
import AccountGym from "./pages/AccountGym.jsx";
import AccountAdmin from "./pages/AccountAdmin.jsx";

const App = () => {
	//#TODO implement permissions for the page for different roles
	return (
		<BrowserRouter>
			<Routes>
				<Route path="/" element={<Main/>}>
					<Route index element={<Index/>}/>
					{/*TODO make the route the same for all the components + add conditional rendering of Account pages*/}
					<Route path={"account/:username"}>
						<Route index element={<AccountUser/>}/>
						<Route path={"history"} element={<History/>}/>
						<Route path={"gym"} element={<AccountGym/>}/>
						<Route path={"admin"} element={<AccountAdmin/>}/>
					</Route>
					<Route path={"*"} element={<NotFound/>}/>
				</Route>
				<Route path={"/login"} element={<LogIn/>}/>
			</Routes>
		</BrowserRouter>
	)
}

export default App
