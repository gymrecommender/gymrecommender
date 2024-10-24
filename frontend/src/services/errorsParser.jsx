const errorParser = ({request, response, message}) => {
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

export default errorParser;

