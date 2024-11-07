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

export {displayTimestamp}

