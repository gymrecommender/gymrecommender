import MapSection from '../../components/MapSection.jsx';
import React from "react";
import Form from "../../components/simple/Form.jsx";

const wRating = [
	{value: 5, label: "No Waiting"},
	{value: 4, label: "Less than 5 Minutes"},
	{value: 3, label: "5-10 Minutes"},
	{value: 2, label: "10-20 Minutes"},
	{value: 1, label: "More than 20 Minutes"},
];
const cRating = [
	{value: 5, label: "Nearly Empty"},
	{value: 4, label: "Lightly Busy"},
	{value: 3, label: "Moderately Busy"},
	{value: 2, label: "Busy but Manageable"},
	{value: 1, label: "More than 20 Minutes"},
];

const data = {
	fields: [
		{pos: 0, type: 'title', text: "Share your experience with others!"},
		{pos: 1, type: "time", label: "Time of Visit", name: "visitTime", wClassName: "time"},
		{pos: 2, type: "range", label: "Overall Rating", name: "rating", step: 0.5, min: 1, max: 5, value: 3, required: true},
		{pos: 3, type: "select", required: true, label: "Average waiting time for a machine or space", data: wRating, name: "cRatingWait"},
		{pos: 4, type: "select", required: true, label: "How crowded the gym feels", data: cRating, name: "cRatingCrowd"},
	],
	button: {
		type: "submit",
		text: "Apply",
		className: "btn-submit",
	}
}


const Account = () => {
	const handleSubmit = (values) => {
		console.log(values);
	}

	//TODO the form should be non-interactable until the gym is selected on the map
	return (
		<>
			<aside className="sliders">
				<Form data={data} onSubmit={handleSubmit}/>
			</aside>
			<MapSection/>
		</>
	)
}

export default Account;