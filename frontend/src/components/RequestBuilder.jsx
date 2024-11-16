import React, {useState} from 'react';
import Slider from "./simple/Slider.jsx";
import Button from "./simple/Button.jsx";
import Input from "./simple/Input.jsx";
import Select from "./simple/Select.jsx";

const RequestBuilder = () => {
	const membershipTypes = [
		{value: "1-month", label: "1 month"},
		{value: "6-months", label: "6 months"},
		{value: "year", label: "Year"},
	]
	const [formValue, setFormValue] = useState({
		pPriority: 50,
		mPrice: 40,
		rating: 3,
		cRating: 3,
		dTime: null,
		aTime: null,
		membershipType: membershipTypes[0].value
	})

	const handleChange = (name, value) => {
		//It is important to create a new object in order to have expected behaviour of the state
		setFormValue({...formValue, [name]: value});
	}
	const handleSubmit = (e) => {
		e.preventDefault();
		console.log(formValue)
	}

	return (
		<aside className="sliders">
			<form onSubmit={handleSubmit}>
				<h3>What is your priority?</h3>
				<Slider type={"range"} name="pPriority"
				        step={5} min={0} max={100} minText={"Time"} maxText={"Price"} value={formValue["pPriority"]}
				        onChange={handleChange} isSplit={true}/>
				<Select
						label={"Membership length"}
						data={membershipTypes}
						value={formValue.membershipType}
						onChange={handleChange}
				        name="membershipType"
				/>
				<h3>Tell us your preferences!</h3>


				<Input
					type="time"
					name="dTime"
					wClassName={"time"}
					label={"Preferred departure time"}
					value={formValue["dTime"] ?? ''}
					onChange={handleChange}
				/>
				<Input
					type="time"
					name="aTime"
					wClassName={"time"}
					label={"Preferred arrival time"}
					value={formValue["aTime"] ?? ''}
					onChange={handleChange}
				/>
				<Slider type={"range"} label={"Min membership price"} name="mPrice" min={0} max={100} step={5}
				        value={formValue["mPrice"]}
				        onChange={handleChange}/>
				<Slider type={"range"} label={"Min overall rating"} name="rating" min={1} max={5} step={0.5}
				        value={formValue["rating"]} onChange={handleChange}/>
				<Slider type={"range"} label={"Min congestion rating"} name="cRating" min={1} max={5} step={0.5}
				        value={formValue["cRating"]} onChange={handleChange}/>

				<Button type={"submit"} className={"btn-submit"} onSubmit={handleSubmit}>Apply</Button>
			</form>
		</aside>
	);
};

export default RequestBuilder;
