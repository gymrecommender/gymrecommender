import React, {useState} from 'react';
import Slider from "./Slider.jsx";
import Button from "./Button.jsx";

const RequestBuilder = () => {
	const [formValue, setFormValue] = useState({
		tpPriority: 50,
		mPrice: 40,
		rating: 3,
		cRating: 3,
		dTime: null,
		aTime: null
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
		<section className="sliders">
			<form onSubmit={handleSubmit}>
				<h3>What is your priority?</h3>
				<Slider type={"range"} name="tpPriority"
				        step={5} min={0} max={100} minText={"Time"} maxText={"Price"} value={formValue["tpPriority"]}
				        onChange={handleChange} isSplit={true}/>

				<h3>Tell us your preferences!</h3>
				{/*#TODO range for membership must be formed based on meaningful data*/ }
				<Slider type={"range"} label={"Min membership price"} name="mPrice" min={0} max={100} step={5}
				        value={formValue["mPrice"]}
				        onChange={handleChange}/>
				<Slider type={"range"} label={"Min overall rating"} name="rating" min={1} max={5} step={0.5}
				        value={formValue["rating"]} onChange={handleChange}/>
				<Slider type={"range"} label={"Min congestion rating"} name="cRating" min={1} max={5} step={0.5}
				        value={formValue["cRating"]} onChange={handleChange}/>

				<Button type={"submit"} className={"button-request"} onSubmit={handleSubmit}>Apply</Button>
			</form>
		</section>
	);
};

export default RequestBuilder;
