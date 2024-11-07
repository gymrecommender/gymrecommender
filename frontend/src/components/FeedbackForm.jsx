import React, {useState} from 'react';
import Select from "./simple/Select.jsx";
import Button from "./simple/Button.jsx";
import Input from "./simple/Input.jsx";
import Slider from "./simple/Slider.jsx";

const FeedbackForm = () => {
	const [formValue, setFormValue] = useState({
		cRatingWait: 5,
		cRatingCrowd: 5,
		visitTime: "",
		rating: 3
	});
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

	const handleChange = (name, value) => {
		setFormValue({...formValue, [name]: value});
	};
	const handleSelectChange = (name, value) => {
		if (1 <= value <= 5) {
			handleChange(name, value);
		}
	}

	const handleSubmit = (e) => {
		e.preventDefault();
		console.log(formValue);
	};

	return (
		<aside className="sliders">
			<form onSubmit={handleSubmit}>
				<h3>Share your experience with others!</h3>
				<Input
					type="time"
					name="visitTime"
					wClassName={"time"}
					label={"Time of Visit:"}
					value={formValue["visitTime"] ?? ''}
					onChange={handleChange}
				/>
				<Slider
					type={"range"}
					name="rating"
					min={1}
					max={5}
					step={0.5}
					label={"Overall Rating"}
					value={formValue.rating}
					onChange={handleChange}
				/>
				<Select
					label={"Average waiting time for a machine or space"}
					data={wRating}
					name={"cRatingWait"}
					value={formValue.cRatingWait}
					onChange={handleSelectChange}
				/>
				<Select
					label={"How crowded the gym feels"}
					data={cRating}
					name={"cRatingCrowd"}
					value={formValue.cRatingCrowd}
					onChange={handleSelectChange}
				/>

				<Button className={"btn-submit"} type={"submit"} onClick={handleSubmit}>Apply</Button>
			</form>
		</aside>
	);
};

export default FeedbackForm;
