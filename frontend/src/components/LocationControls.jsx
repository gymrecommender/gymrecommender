import Input from "./simple/Input.jsx";
import Button from "./simple/Button.jsx";
import {useState} from "react";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import { faLocationCrosshairs, faLocationDot } from "@fortawesome/free-solid-svg-icons";

const LocationControls = ({onGetLocation, onSubmitSearch}) => {
	const [searchState, setSearchState] = useState('')
	const handleSearchChange = (name, value) => {
		setSearchState(value);
	}
	const handleSubmitSearch = (e) => {
		e.preventDefault()
		onSubmitSearch(searchState);
	}

	return (
		<div className="location-controls">
			<Button type={"button"} onClick={onGetLocation}>
				<FontAwesomeIcon className={"icon"} size={"2x"} icon={faLocationCrosshairs}/></Button>
			<form onSubmit={handleSubmitSearch}>
				<Input type={"text"} name="locationSearch" placeholder="Search for location" value={searchState} onChange={handleSearchChange}/>
			</form>
		</div>
	);
};

export default LocationControls;
