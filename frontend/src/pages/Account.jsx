import {useParams} from "react-router-dom";

const Account = () => {
	const { username } = useParams();

	return (
		<>
			Account page of {username}
		</>
	)
}

export default Account;