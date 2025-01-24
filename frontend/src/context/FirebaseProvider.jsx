import {useEffect, useState, createContext, useContext} from "react";
import {auth, provider} from "../Firebase.jsx";
import {signInWithPopup, onAuthStateChanged} from "firebase/auth";
import {useNavigate} from "react-router-dom";
import {accountLogin, accountLogout, accountSignUp} from "../services/accountHelpers.jsx";
import {attachToken, axiosInternal, detachToken} from "../services/axios.jsx";
import {useLoader} from "./LoaderProvider.jsx";
import {debounce} from "lodash";

const FirebaseContext = createContext();
const useFirebase = () => useContext(FirebaseContext);

const FirebaseProvider = ({children}) => {
	const [user, setUser] = useState(null);

	//we need a separate loader for the initial loading of the page as we do not want the whole page to disappear upon different requests
	const [loading, setLoading] = useState(true);
	const navigate = useNavigate();
	const {setLoader} = useLoader();

	useEffect(() => {
		const debouncedAuthHandler = debounce(async (user) => {
			if (user) {
				if (user.emailVerified) {
					attachToken(user.accessToken);
					const result = await axiosInternal('GET', `account/role`);

					if (result.error) {
						detachToken();
						setUser(null);
					} else {
						setUser({ username: result.data.username, role: result.data.role });
					}
				}
			} else {
				detachToken();
				setUser(null);
			}
			setLoading(false);
		}, 600);

		const unsubscribe = onAuthStateChanged(auth, debouncedAuthHandler);

		return () => {
			debouncedAuthHandler.cancel();
			unsubscribe();
		};
	}, []);

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

		console.log(result);
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