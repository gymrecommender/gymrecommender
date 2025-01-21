import Button from "./simple/Button.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import { faLocationCrosshairs } from "@fortawesome/free-solid-svg-icons";

const LocationControls = ({onGetLocation}) => {
	return (
		<div className="location-controls">
			<Button title={"Get your current location"} type={"button"} onClick={onGetLocation}>
				<FontAwesomeIcon className={"icon"} size={"2x"} icon={faLocationCrosshairs}/></Button>
		</div>
	);
};

export default LocationControls;
