import axios from "axios";
import {errorsParser} from "./helpers.jsx";

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

export {axiosInternal, axiosGoogleAPI};