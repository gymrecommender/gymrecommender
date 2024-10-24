import {useParams} from "react-router-dom";

const History = () => {
	const { username } = useParams();
	return (
		<>
			History page of {username}
		</>
	)
}

export default History;