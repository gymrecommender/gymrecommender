import {axiosInternal} from "./axios.jsx";
import {
	createUserWithEmailAndPassword,
	deleteUser,
	sendEmailVerification,
	updateProfile,
	signOut,
	signInWithEmailAndPassword
} from "firebase/auth";
import {auth} from "../Firebase.jsx";
import {firebaseErrors} from "./helpers.jsx";

const accountSignUp = async (values) => {
	const result = {data: null, error: null}

	try {
		//TODO make sure that if something fails, it is handled properly
		const checkUser = await axiosInternal('GET', `useraccount/${values.username}`, values.username);
		if (checkUser.error?.status === 404) {
			const outerUser = await createUserWithEmailAndPassword(auth, values.email, values.password);

			const data = {
				...values,
				'IsEmailVerified': outerUser.user.emailVerified,
				'OuterUid': outerUser.user.uid,
				'provider': 'local',
				'type': 'user'
			}
			const dbUser = await axiosInternal('POST', 'useraccount', data)
			if (dbUser.error) {
				result.error = dbUser.error.data.errors;
				await deleteUser(outerUser.user) //all the data will be lost so we will not be able to align the database and the firebase
				return dbUser
			}

			await updateProfile(outerUser.user, {
				displayName: values.username
			})
			await sendEmailVerification(outerUser.user)

			//we want to enforce the verification of the email, and for that reason we need to force user to log in
			//plus updateProfile does not trigger onAuthStateChanged which means that before it is triggerred by something else, displayName in the app will be set to null
			await signOut(auth)
		} else {
			result.error = {"username": checkUser.data.length > 0 ? `The user with username ${values.username} already exists` : checkUser.error.message}

			return result
		}
	} catch (e) {
		result.error = firebaseErrors(e.code);
		if (!result.error) {
			result.error = e
		}
	}
	return result;
}

const accountLogin = async (values) => {
	const result = {data: null, error: null}

	try {
		const signInResult = await signInWithEmailAndPassword(auth, values.email, values.password);

		if (!signInResult.user.emailVerified) {
			result.error = {"message": "You must verify your email before you are allowed to log in"};
			await signOut(auth)

			return result;
		}

		//TODO propagate expired datetime from the firebase
		const login = await axiosInternal('POST', `useraccount/${signInResult.user.displayName}/login`, {
			token: signInResult.user.accessToken
		})
		if (login.error) {
			result.error = login.error.message;
			await signOut(auth);

			return result;
		}
	} catch (e) {
		result.error = firebaseErrors(e.code);
		if (!result.error) {
			result.error = e
		}
	}

	return result
}

export {accountSignUp, accountLogin}

