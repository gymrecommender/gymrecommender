import {attachToken, axiosInternal, detachToken} from "./axios.jsx";
import {
	createUserWithEmailAndPassword,
	deleteUser,
	sendEmailVerification,
	updateProfile,
	signOut,
	signInWithEmailAndPassword
} from "firebase/auth";
import {auth} from "../Firebase.jsx";
import {errorsParser, firebaseErrors} from "./helpers.jsx";
import {toast} from "react-toastify";

const roleMapper = {
	"admin": "adminaccount",
	"user": "useraccount",
	"gym": "gymaccount"
}

const accountSignUp = async (values, role) => {
	const result = {data: null, error: null}

	try {
		if (role !== "user") {
			result.error = errorsParser({message:"Operation is not allowed"})
			return result
		}
		const checkUser = await axiosInternal('GET', `account/role`);

		if (checkUser.error?.status === 404 || checkUser.error?.status === 401) {
			const result = await axiosInternal("POST", 'useraccount', {
				...values,
				'provider': 'local'
			})

			if (result.error) toast(result.error.message);
			else {
				toast("Success! Check your email for the confirmation link")
			}
		} else {
			result.error = {"message": checkUser.data.length > 0 ? `The user with username ${values.username} already exists` : errorsParser(checkUser.error)}

			return result
		}
	} catch (e) {
		result.error = {message: firebaseErrors(e.code)};
		if (!result.error.message) {
			result.error = errorsParser(e)
		}
	}
	return result;
}

const accountLogin = async (values, role) => {
	const result = {data: null, error: null}

	try {
		const signInResult = await signInWithEmailAndPassword(auth, values.email, values.password);

		if (!signInResult.user.emailVerified) {
			result.error = errorsParser({"message": "You must verify your email before you are allowed to log in"});
			await signOut(auth)

			return result;
		}

		attachToken(signInResult.user.accessToken)
		//TODO propagate expired datetime from the firebase
		const login = await axiosInternal('POST', `${roleMapper[role]}/login`)
		if (login.error) {
			result.error = errorsParser(login.error);
			await signOut(auth);

			return result;
		}
	} catch (e) {
		result.error = {message: firebaseErrors(e.code)};
		if (!result.error.message) {
			result.error = errorsParser(e)
		}
	}

	return result
}

const accountLogout = async (username, role) => {
	const result = {error: null};
	try {
		const logoutResult = await axiosInternal('DELETE', `${roleMapper[role]}/logout`);
		if (logoutResult.error) {
			result.error = errorsParser(logoutResult.error);

			return result;
		}

		await signOut(auth);
	} catch (e) {
		result.error = {message: firebaseErrors(e.code)};
		if (!result.error.message) {
			result.error = errorsParser(e)
		}
	}

	return result;
}

export {accountSignUp, accountLogin, accountLogout}

