import {APIProvider, Map} from "@vis.gl/react-google-maps";

const GoogleMap = ({coordinates}) => {
	return (
		<section className="section section-map">
			<APIProvider apiKey={import.meta.env.VITE_GOOGLE_API_KEY}>
				<Map
					defaultZoom={13}
					defaultCenter={ { lat: -33.860664, lng: 151.208138 } }
				/>
			</APIProvider>
		</section>
	)
}

export default GoogleMap;