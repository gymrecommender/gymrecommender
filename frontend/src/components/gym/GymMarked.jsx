
import Form from "../simple/Form.jsx";

const data = {
	fields: [
		{pos: 0, label: "Unavailable from", type: "datetime-local", name: "closedFrom"}, //TODO implement more extensive validation
		{pos: 1, label: "Unavailable until", type: "datetime-local", name: "closedUntil"} //TODO implement more extensive validation
	],
	button: {
		type: "submit",
		text: "Save",
		className: "btn-submit",
	}
}
const GymMarked = ({gymId}) => {
	const handleSubmit = (values) => {
		console.log(values);
	}

	return (
		<div className="gym-marked">
			<div className={"gym-marked-title"}>
				Mark as unavailable
			</div>
			<Form data={data} onSubmit={handleSubmit}/>
		</div>
	)
}

export default GymMarked;