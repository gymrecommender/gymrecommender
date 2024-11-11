const displayTimestamp = (timestamp) => {
	if (!timestamp) {
		return null;
	}

	const date = new Date(timestamp);
	return date.toLocaleString('en-GB', {
		year: 'numeric',
		month: 'short',
		day: 'numeric',
		hour: 'numeric',
		minute: '2-digit',
		second: '2-digit',
		timeZoneName: 'short'
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

export {displayTimestamp, getLocation}

