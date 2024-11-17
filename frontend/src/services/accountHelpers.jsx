import {axiosInternal} from "./axios.jsx";
import {createUserWithEmailAndPassword, deleteUser, sendEmailVerification, updateProfile, signOut} from "firebase/auth";
import {auth} from "../Firebase.jsx";
import {firebaseErrors} from "./helpers.jsx";

const accountSignUp = async (values) => {
	const result = {data: null, error: null}

	try {
		//TODO make sure that if something fails, it is handled properly
		const checkUser = await axiosInternal('GET', 'account', null,  {}, {username: values.username});
		if (checkUser.data.length > 0) {
			result.error = {
				'username': "username is already in use"
			}
			return result
		}

		const outerUser = await createUserWithEmailAndPassword(auth, values.email, values.password);

		const data = {
			...values,
			'is_email_verified': outerUser.user.emailVerified,
			'outer_uid': outerUser.user.uid,
			'provider': 'local',
			'type': 'user'
		}
		const dbUser = await axiosInternal('POST', 'account', null, data)
		if (dbUser.error) {
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
	} catch (e) {
		result.error = firebaseErrors(e.code);
		if (!result.error) {
			result.error = e
		}
	}
	return result;
}

export {accountSignUp}

