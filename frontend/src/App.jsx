import {BrowserRouter, Routes, Route} from "react-router-dom";
import Index from "./pages/Index";
import Account from "./pages/Account.jsx";
import NotFound from "./pages/NotFound.jsx";
import History from "./pages/History.jsx";
import LogIn from "./pages/LogIn.jsx";
import Main from "./layouts/Main.jsx";

const App = () => {
	//#TODO implement permissions for the page for different roles
	return (
		<BrowserRouter>
			<Routes>
				<Route path="/" element={<Main/>}>
					<Route index element={<Index/>}/>
					<Route path={"account/:username"}>
						<Route index element={<Account/>}/>
						<Route path={"history"} element={<History/>}/>
					</Route>
					<Route path={"*"} element={<NotFound/>}/>
				</Route>
				<Route path={"/login"} element={<LogIn/>}/>
			</Routes>
		</BrowserRouter>
	)
}

export default App
