import { useEffect, useState, createContext, useContext } from "react";
import {auth, provider} from "../Firebase.jsx";
import {signInWithPopup, signInWithEmailAndPassword, createUserWithEmailAndPassword, onAuthStateChanged, signOut} from "firebase/auth";
import {useNavigate} from "react-router-dom";

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
			message = "Unknown error occurred";
			break;
	}
	return message;
}

const FirebaseProvider = ({children}) => {
	const [user, setUser] = useState(null);
	const navigate = useNavigate();

	useEffect(() => {
		/*
		We need onAuthStateChange it order to be able to
		a) track the changes made by the Firebase (for example, session expiration)
		b) retrieve the login status upon the reloading of the page, for example
		*/
		const unsubscribe = onAuthStateChanged(auth, (user) => {
			if (user) {
				setUser({uid: user.uid, email: user.email});
				navigate("/");
			} else {
				setUser(null);
			}
		})

		//We need this listener for the sake of tracking outer changes of state (for instance, if Firebase tells us that the session has expired)
		//And we return this function in order for the listener to be cleaned up when the component is unmounted
		return () => unsubscribe();
	}, [])

	const signUp = async (values) => {
		const result = {error: null}
		try {
			const result = await createUserWithEmailAndPassword(auth, values.email, values.password);
			//TODO store the data to the database plus user's uid
			setUser({uid: result.user.uid, username: values.username, email: result.user.email});
		} catch (e) {
			result.error = firebaseErrors(e.code);
		}
		return result;

	}
	const signIn = async (values) => {
		const result = {error: null}
		try {
			const result = await signInWithEmailAndPassword(auth, values.email, values.password);
			//TODO retrieve the username of the user by email and add the username to the user state
			setUser({uid: result.user.uid, email: result.user.email});
		} catch (e) {
			result.error = firebaseErrors(e.code);
			console.log(e)
		}
		return result;
	}

	const signInWithGoogle = () => signInWithPopup(auth, provider);
	const logout = async () =>  {
		const result = {error: null};
		try {
			await signOut(auth);
			navigate('/')
		} catch (e) {
			result.error = firebaseErrors(e.code);
		}

		return result;
	}
	const getUser = () => user

	return (
		<FirebaseContext.Provider value={{ signInWithGoogle, signIn, logout, getUser, signUp }}>
			{children}
		</FirebaseContext.Provider>
	)
}

export {useFirebase, FirebaseProvider};