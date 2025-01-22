import {createContext, useContext, useState} from "react";

const defaultState = {
	isShow: false,
	message: "",
	onConfirm: null,
	onCancel: null,
}
const ConfirmContext = createContext();
const ConfirmProvider = ({ children }) => {
	const [data, setData] = useState(defaultState);

	const setValues = (isShow, message, onConfirm, onCancel) => {
		setData({
			message,
			isShow,
			onConfirm,
			onCancel,
		})
	}
	const setMessage = (message) => {
		setData({...data, message})
	}
	const setIsShow = (isShow) => {
		setData({...data, isShow})
	}
	const setFunc = (onConfirm, onCancel) => {
		setData({...data, onConfirm, onCancel})
	}
	const flushData = () => {
		setData(defaultState);
	}
	return (
		<ConfirmContext.Provider value={{data, flushData, setValues, setMessage, setFunc, setIsShow}}>
			{children}
		</ConfirmContext.Provider>
	)
}

const useConfirm = () => useContext(ConfirmContext);

export {useConfirm, ConfirmProvider};