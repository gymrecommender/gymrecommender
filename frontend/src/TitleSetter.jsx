import {useTitle} from "./context/TitleProvider.jsx";
import {useEffect} from "react";

const TitleSetter = ({title, children}) => {
	const {setTitle} = useTitle();

	useEffect(() => {
		setTitle(title);
	}, [title]);

	return <>
		{children}
	</>
}

export default TitleSetter;