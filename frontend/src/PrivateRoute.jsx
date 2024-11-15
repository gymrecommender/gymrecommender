import {useFirebase} from "./context/FirebaseProvider.jsx";
import {Navigate} from "react-router-dom";

const PrivateRoute = ({children}) => {
	const {getUser} = useFirebase();
	const user = getUser();

	if (!user) {
		return <Navigate to="/login" />;
	}

	return children;
}

export default PrivateRoute;