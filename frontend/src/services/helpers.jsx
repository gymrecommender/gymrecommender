const displayTimestamp = (timestamp, isShort=false) => {
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

export {displayTimestamp, getLocation, firebaseErrors}

