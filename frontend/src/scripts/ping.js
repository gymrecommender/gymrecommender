

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
            .catch(error => {});
    }

    setInterval(sendPing, 780000); //13 mins, these are miliseconds we can just hardcode the exact number of milisecodns we want for the interval and leave it here
}


