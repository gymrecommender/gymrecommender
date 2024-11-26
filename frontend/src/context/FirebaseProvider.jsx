import { useEffect, useState, createContext, useContext } from "react";
import {auth, provider} from "../Firebase.jsx";
import {signInWithPopup, signInWithEmailAndPassword, createUserWithEmailAndPassword, onAuthStateChanged, signOut, updateProfile, deleteUser, sendEmailVerification} from "firebase/auth";
import {useNavigate} from "react-router-dom";
import {accountLogin, accountSignUp} from "../services/accountHelpers.jsx";
import {axiosInternal} from "../services/axios.jsx";

const FirebaseContext = createContext();
const useFirebase = () => useContext(FirebaseContext);

const firebaseErrors = (code) => {
	let message = ''
	switch (code) {
		case "auth/email-already-in-use":
			message = "The provided email is already in use."
			break;
		case "auth/invalid-credential":
			message = "The provided login and/or password is/are invalid."
			break;
		default:
			message = null;
			break;
	}
	return message;
}

const FirebaseProvider = ({children}) => {
	const [user, setUser] = useState(null);
	const [loading, setLoading] = useState(true);
	const navigate = useNavigate();

	useEffect(() => {
		/*
		We need onAuthStateChange it order to be able to
		a) track the changes made by the Firebase (for example, session expiration)
		b) retrieve the login status upon the reloading of the page, for example
		*/
		const unsubscribe = onAuthStateChanged(auth, async (user) => {
			//TODO handle changes of the token (propagate it to the db)
			if (user) {
				const result = await axiosInternal('GET', `account/${user.uid}/role`);
				if (result.error) {
					//TODO the error from here should be somehow propagated and diplayed to the user
					setUser(null);
				} else {
					setUser({username: user.displayName, role: result.data.role});
				}
			} else {
				setUser(null);
			}
			setLoading(false);
		})

		//We need this listener for the sake of tracking outer changes of state (for instance, if Firebase tells us that the session has expired)
		//And we return this function in order for the listener to be cleaned up when the component is unmounted
		return () => unsubscribe();
	}, [])

	const signUp = async (values) => {
		const result = await accountSignUp(values);

		if (!result.error) {
			navigate('/login')
		}

		return result
	}

	const signIn = async (values) => {
		const result = await accountLogin(values)
		if (!result.error) {
			navigate('/')
		}

		return result;
	}

	const signInWithGoogle = () => signInWithPopup(auth, provider);
	const logout = async () =>  {
		const result = {error: null};
		try {
			const logoutResult = await axiosInternal('DELETE', `useraccount/${user.username}/logout`);
			if (logoutResult.error) {
				result.error = logoutResult.error.message;

				return result;
			}

			await signOut(auth);
			navigate('/');
		} catch (e) {
			result.error = firebaseErrors(e.code);
			if (!result.error) {
				result.error = e
			}
		}

		return result;
	}
	const getUser = () => user
	const getLoading = () => loading
	return (
		<FirebaseContext.Provider value={{ signInWithGoogle, signIn, logout, getUser, signUp, getLoading}}>
			{children}
		</FirebaseContext.Provider>
	)
}

export {useFirebase, FirebaseProvider};