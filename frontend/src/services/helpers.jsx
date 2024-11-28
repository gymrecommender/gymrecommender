export const emailRegEx = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/

const displayTimestamp = (timestamp, isShort = false) => {
	if (!timestamp) {
		return null;
	}

	const date = new Date(timestamp);
	return date.toLocaleString('en-GB', {
		year: 'numeric',
		month: 'short',
		day: 'numeric',
		...(isShort ? {} : {
			hour: 'numeric',
			minute: '2-digit',
			second: '2-digit',
			timeZoneName: 'short'
		})
	});
}


const getLocation = async () => {
	return new Promise((resolve) => {
		const data = {lat: null, lng: null, error: null}

		if (navigator.geolocation) {
			navigator.geolocation.getCurrentPosition(
				(position) => {
					data.lat = position.coords.latitude;
					data.lng = position.coords.longitude;
					resolve(data);
				},
				(error) => {
					data.error = error.message;
					resolve(data);
				}
			)
		} else {
			data.error = "Your browser does not support geolocation";
			resolve(data);
		}
	});
}

const generateValidationRules = (label, params) => {
	return {
		...(params.required
			? {required: {value: true, message: `The field is required`}}
			: {}),
		...(params.min
			? {min: {value: params.min, message: `The value must be higher than ${params.min}`}}
			: {}),
		...(params.max
			? {max: {value: params.max, message: `The value must be lower than ${params.max}`}}
			: {}),
		...(params.minLength
			? {minLength: {value: params.minLength, message: `The length must be at least ${params.minLength}`}}
			: {}),
		...(params.maxLength
			? {maxLength: {value: params.maxLength, message: `The length must not exceed ${params.maxLength}`}}
			: {}),
		...(params.pattern
			? {pattern: {value: params.pattern.regEx, message: params.pattern.message}}
			: {}),
		...(params.sameAs
			? {
				validate: {
					sameAs: (value, formValues) =>
						value === formValues[params.sameAs.fieldName] || params.sameAs.message,
				},
			}
			: {}),
	};
};

const firebaseErrors = (code) => {
	let message = ''
	switch (code) {
		case "auth/email-already-in-use":
			message = "The provided email is already in use."
			break;
		case "auth/invalid-credential":
			message = "The provided login and/or password is/are invalid."
			break;
		default:
			message = null;
			break;
	}
	return message;
}

export {displayTimestamp, getLocation, firebaseErrors, generateValidationRules}

