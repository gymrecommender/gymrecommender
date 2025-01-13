import {createContext, useContext, useState} from "react";

const SelectedGymContext = createContext();
const SelectedGymProvider = ({ children }) => {
	const [gymId, setGymId] = useState(null);

	return (
		<SelectedGymContext.Provider value={{gymId, setGymId}}>
			{children}
		</SelectedGymContext.Provider>
	)
}

const useSelectedGym = () => useContext(SelectedGymContext);

export {useSelectedGym, SelectedGymProvider};