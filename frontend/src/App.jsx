import {BrowserRouter, Routes, Route} from "react-router-dom";
import Index from "./pages/Index";
import Account from "./pages/Account.jsx";
import NotFound from "./pages/NotFound.jsx";
import History from "./pages/History.jsx";

const App = () => {
	//#TODO implement permissions for the page for different roles
	return (
		<BrowserRouter>
			<Routes>
				<Route path="/">
					<Route index element={<Index/>}/>
					<Route path={"/account/:username"}>
						<Route index element={<Account/>}/>
						<Route path={"/account/:username/history"} element={<History/>}/>
					</Route>
					<Route path={"*"} element={<NotFound/>}/>
				</Route>
			</Routes>
		</BrowserRouter>
	)
}

export default App
