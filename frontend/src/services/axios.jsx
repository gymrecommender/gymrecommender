import axios from "axios";
import errorsParser from "./errorsParser.jsx";

const instance = axios.create({
	headers: {
		'Content-Type': 'application/json',
	}
})

const axiosRequest = async (method, endpoint, id = null, data = {}, queryParams={}) => {
	const uri = id ? `/api/${endpoint}/${id}` : `/api/${endpoint}`;
	const requestConfig = {method, url: uri, data, queryParams}
	const result = {data: null, error: null}

	try {
		const {data} = await instance(requestConfig);
		result.data = data;
	} catch (error) {
		result.error = errorsParser(error);
	}

	return result;
}

export default axiosRequest;