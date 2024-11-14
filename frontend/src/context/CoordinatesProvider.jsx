import {createContext, useContext, useState} from "react";

const CoordinatesContext = createContext();
const CoordinatesProvider = ({ children }) => {
	const [coordinates, setCoordinates] = useState({
		lat: null,
		lng: null
	});

	return (
		<CoordinatesContext.Provider value={{coordinates, setCoordinates}}>
			{children}
		</CoordinatesContext.Provider>
	)
}

const useCoordinates = () => useContext(CoordinatesContext);

export {useCoordinates, CoordinatesProvider};