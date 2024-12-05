import {createContext, useContext, useState} from "react";

const CoordinatesContext = createContext();
const CoordinatesProvider = ({children}) => {
	const [coordinates, setCoordinates] = useState({lat: -33.860664, lng: 151.208138});

	return (
		<CoordinatesContext.Provider value={{coordinates, setCoordinates}}>
			{children}
		</CoordinatesContext.Provider>
	)
}

const useCoordinates = () => useContext(CoordinatesContext);

export {useCoordinates, CoordinatesProvider};