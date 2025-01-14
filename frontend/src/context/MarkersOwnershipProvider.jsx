import {createContext, useContext, useState} from "react";

const MarkersOwnershipContext = createContext();
const MarkersOwnershipProvider = ({ children }) => {
	const [requests, setRequests] = useState([]);

	return (
		<MarkersOwnershipContext.Provider value={{requests, setRequests}}>
			{children}
		</MarkersOwnershipContext.Provider>
	)
}

const useMarkersOwnership = () => useContext(MarkersOwnershipContext);

export {useMarkersOwnership, MarkersOwnershipProvider};