export function pingBackend() {
    //console.log("pingBackend function started"); // Debugging line
    const backendUrl = `${import.meta.env.VITE_BACKEND_URL}/api/ping`;

    function sendPing() {
        //console.log("Sending ping to backend...");
        fetch(backendUrl, {
            method: "GET",
        })
            .then(response => {
                if (response.ok) {
                    response.text();
                } else {
                    console.error("Ping failed:", response.status);
                }
            })
            .catch(error => {
	            console.error("Ping failed:", error);
            });
    }

    setInterval(sendPing, 60000); //1 min, these are miliseconds we can just hardcode the exact number of milisecodns we want for the interval and leave it here
}


