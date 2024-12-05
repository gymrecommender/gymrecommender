import Button from "./simple/Button.jsx";
import Form from "./simple/Form.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import { faLocationCrosshairs, faLocationDot } from "@fortawesome/free-solid-svg-icons";

const data = {
	fields: [
		{pos: 1, type: "text", name: "locationSearch", placeholder: "Search for location"}
	]
}
const LocationControls = ({onGetLocation, onSubmitSearch}) => {
	const handleSubmitSearch = (values) => {
		onSubmitSearch(values);
	}

	return (
		<div className="location-controls">
			<Button type={"button"} onClick={onGetLocation}>
				<FontAwesomeIcon className={"icon"} size={"2x"} icon={faLocationCrosshairs}/></Button>
			<Form data={data} onSubmit={handleSubmitSearch}/>
		</div>
	);
};

export default LocationControls;
