import {useEffect, useState, createContext, useContext} from "react";
import {auth, provider} from "../Firebase.jsx";
import {signInWithPopup, onAuthStateChanged} from "firebase/auth";
import {useNavigate} from "react-router-dom";
import {accountLogin, accountLogout, accountSignUp} from "../services/accountHelpers.jsx";
import {attachToken, axiosInternal, detachToken} from "../services/axios.jsx";
import {useLoader} from "./LoaderProvider.jsx";

const FirebaseContext = createContext();
const useFirebase = () => useContext(FirebaseContext);


const FirebaseProvider = ({children}) => {
	const [user, setUser] = useState(null);

	//we need a separate loader for the initial loading of the page as we do not want the whole page to disappear upon different requests
	const [loading, setLoading] = useState(true);
	const navigate = useNavigate();
	const {setLoader} = useLoader();

	useEffect(() => {
		/*
		We need onAuthStateChange it order to be able to
		a) track the changes made by the Firebase (for example, session expiration)
		b) retrieve the login status upon the reloading of the page, for example
		*/
		const unsubscribe = onAuthStateChanged(auth, async (user) => {
			//TODO handle changes of the token (propagate it to the db)
			if (user) {
				if (user.emailVerified) {
					if (user.accessToken) {
						attachToken(user.accessToken);
					}
					const result = await axiosInternal('GET', `account/${user.uid}/role`);

					if (result.error) {
						//TODO the error from here should be somehow propagated and diplayed to the user
						detachToken();
						setUser(null);
					} else {
						setUser({username: user.displayName, role: result.data.role});
					}
				}
			} else {
				detachToken();
				setUser(null);
			}
			setLoading(false);
		})

		//We need this listener for the sake of tracking outer changes of state (for instance, if Firebase tells us that the session has expired)
		//And we return this function in order for the listener to be cleaned up when the component is unmounted
		return () => unsubscribe();
	}, [])

	const signUp = async (values, role) => {
		setLoader(true);
		const result = await accountSignUp(values, role);
		setLoader(false);

		if (!result.error) {
			navigate(role === "user" ? "/login/" : `/login/${role}`)
		}

		return result
	}

	const signIn = async (values, role) => {
		setLoader(true);
		const result = await accountLogin(values, role)
		setLoader(false);

		if (!result.error) {
			navigate('/')
		}

		return result;
	}

	const signInWithGoogle = () => signInWithPopup(auth, provider);
	const logout = async () => {
		setLoader(true);
		const result = await accountLogout(user.username, user.role);
		setLoader(false);

		if (!result.error) {
			navigate('/')
		}

		return result;
	}
	const getUser = () => user
	const getLoading = () => loading
	return (
		<FirebaseContext.Provider value={{signInWithGoogle, signIn, logout, getUser, signUp, getLoading}}>
			{children}
		</FirebaseContext.Provider>
	)
}

export {useFirebase, FirebaseProvider};