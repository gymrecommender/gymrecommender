import {useState} from "react";
import Input from "../simple/Input.jsx";
import Button from "../simple/Button.jsx";

const GymMarked = ({onSubmit, gymId}) => {
	const [formValues, setFormValues] = useState({
		id: gymId,
		"closedFrom": '',
		"closedUntil": ''
	});

	const handleChange = (name, value) => {
		setFormValues({...formValues, [name]: value});
	}

	const handleSubmit = (e) => {
		e.preventDefault();
		console.log(formValues)
		//if everything went okay
		onSubmit()
	}
	return (
		<div className="gym-marked">
			<div className={"gym-marked-title"}>
				Mark as unavailable
			</div>
			<form onSubmit={handleSubmit}>
				<Input label={"Unavailable from"}
				       type={"datetime-local"}
				       value={formValues.closedFrom}
				       name="closedFrom"
				       onChange={handleChange}
				/>
				<Input label={"Unavailable until"}
				       type={"datetime-local"}
				       value={formValues.closedUntil}
				       name="closedUntil"
				       onChange={handleChange}
				/>

				<Button className={"btn-submit"} type={"submit"} onSubmit={handleSubmit}>
					Save
				</Button>
			</form>
		</div>
	)
}

export default GymMarked;