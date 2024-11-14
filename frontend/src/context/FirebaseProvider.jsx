import { useEffect, useState, createContext, useContext } from "react";
import {auth, provider} from "../Firebase.jsx";
import {signInWithPopup, signInWithEmailAndPassword, createUserWithEmailAndPassword, onAuthStateChanged, signOut} from "firebase/auth";

const FirebaseContext = createContext();
const useFirebase = () => useContext(FirebaseContext);

const FirebaseProvider = ({children}) => {
	const [user, setUser] = useState(null);

	useEffect(() => {
		return unsubscribe = onAuthStateChanged(auth, (user) => {
			setUser(user);
		});
	}, []);

	const signIn = (email, password) => signInWithEmailAndPassword(auth, email, password);
	// const signUp = ()
	const signInWithGoogle = () => signInWithPopup(auth, provider);
	const logout = () => signOut(auth);
	const getUser = () => user

	return (
		<FirebaseContext.Provider value={{ signInWithGoogle, signIn, logout, getUser }}>
			{children}
		</FirebaseContext.Provider>
	)
}

export {useFirebase, FirebaseProvider};