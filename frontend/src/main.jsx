import React, {StrictMode} from 'react';
import "./styles/main.css"
import {createRoot} from 'react-dom/client'
import App from './App.jsx'
import {FirebaseProvider} from "./context/FirebaseProvider.jsx";
import {BrowserRouter} from "react-router-dom";
import {ToastContainer} from "react-toastify";
import 'react-toastify/dist/ReactToastify.css'

createRoot(document.getElementById('root')).render(
	<StrictMode>
		<BrowserRouter>
			<FirebaseProvider>
				<App/>
				<ToastContainer
					position="bottom-center"
					autoClose={5000}
					hideProgressBar={false}
					newestOnTop={false}
					closeOnClick
					rtl={false}
					pauseOnHover
					theme={"dark"}
					transition: Slide
				/>
			</FirebaseProvider>
		</BrowserRouter>
	</StrictMode>
)
