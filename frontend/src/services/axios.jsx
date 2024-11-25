import axios from "axios";

const instance = axios.create({
	headers: {
		'Content-Type': 'application/json',
	}
})

const entityMapping = {
	'PD': 'place/details/json',
	'P': 'place/nearbysearch/json',
	'D': 'directions/json',
	'G': 'geocode/json'
}

const axiosInternal = async (method, endpoint, data = {}, queryParams={}) => {
	const uri = `/api/${endpoint}`
	const requestConfig = {method, url: uri, data, params: queryParams}
	const result = {data: null, error: null}

	try {
		const {data} = await instance(requestConfig);
		result.data = data;
	} catch (error) {
		result.error = errorsParser(error);
	}

	return result;
}

const axiosGoogleAPI = async (entity, queryParams={}) => {
	const result = {data: null, error: null}
	if (!entityMapping.hasOwnProperty(entity)) {
		result.error = "The specified entity is not valid"
		return result;
	}

	const url = `https://maps.googleapis.com/maps/api/${entityMapping[entity]}`;
	const requestConfig = {method: 'GET', url, params: {...queryParams, key: import.meta.env.VITE_GOOGLE_API_KEY}}

	try {
		const {data} = await instance(requestConfig);
		result.data = data;
	} catch (error) {
		result.error = errorsParser(error);
	}

	return result;
}

const errorsParser = ({request, response, message}) => {
	const errorDetails = {
		status: null,
		statusText: null,
		message: null,
		data: null,
	};

	if (response) {
		// Server responded with a status code outside the 2xx range
		// Here the errors can be customized
		errorDetails.status = response.status;
		errorDetails.statusText = response.statusText || 'Error';
		errorDetails.message = response.data?.message || 'An error occurred';
		errorDetails.data = response.data || null;
	} else if (request) {
		errorDetails.message = 'No response received from server';
	} else {
		errorDetails.message = message || 'An unexpected error occurred';
	}

	return errorDetails;
}

export {axiosInternal, axiosGoogleAPI};