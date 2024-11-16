import React, {StrictMode} from 'react';
import "./styles/main.css"
import {createRoot} from 'react-dom/client'
import App from './App.jsx'
import {FirebaseProvider} from "./context/FirebaseProvider.jsx";
import {BrowserRouter} from "react-router-dom";

createRoot(document.getElementById('root')).render(
	<StrictMode>
		<BrowserRouter>
			<FirebaseProvider>
				<App/>
			</FirebaseProvider>
		</BrowserRouter>
	</StrictMode>
)
