import {createContext, useContext, useEffect, useState} from "react";

const TitleContext = createContext();
const TitleProvider = ({ children }) => {
	const [title, setTitle] = useState("Gym Recommender");

	useEffect(() => {
		document.title = title;
	}, [title, setTitle]);

	return (
		<TitleContext.Provider value={{setTitle}}>
			{children}
		</TitleContext.Provider>
	);
}

const useTitle = () => useContext(TitleContext);

export {useTitle, TitleProvider}