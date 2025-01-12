import MapSection from '../components/MapSection.jsx';
import React, {useEffect, useState} from "react";
import Form from "../components/simple/Form.jsx";
import {useCoordinates} from "../context/CoordinatesProvider.jsx";

const membershipTypes = [
	{value: "1-month", label: "1 month"},
	{value: "6-months", label: "6 months"},
	{value: "year", label: "Year"},
]

const data = {
	fields: [
		{pos: 0, type: "title", text: "What is your priority?"},
		{
			pos: 1,
			type: "range",
			required: true,
			name: "pPriority",
			step: 5,
			min: 0,
			max: 100,
			value: 50,
			minText: "Time",
			maxText: "Price",
			isSplit: true,
		},
		{
			pos: 2,
			type: "select",
			required: true,
			data: membershipTypes,
			value: '1-month',
			name: "membershipType",
			label: "Membership length"
		},
		{pos: 3, type: "title", text: "Tell us your preferences!"},
		{pos: 4, type: "time", label: "Preferred departure time", name: "dTime", className: "time", wClassName: "time"},
		{pos: 5, type: "time", label: "Preferred arrival time", name: "aTime", className: "time", wClassName: "time"},
		{pos: 6, type: "range", label: "Min membership price", name: "mPrice", step: 5, min: 0, max: 100, value: 100},
		{pos: 7, type: "range", label: "Min overall rating", name: "rating", step: 0.5, min: 1, max: 5, value: 1},
		{pos: 8, type: "range", label: "Min congestion rating", name: "cRating", step: 0.5, min: 1, max: 5, value: 1},
	],
	button: {
		type: "submit",
		text: "Apply",
		className: "btn-submit",
	}
};
const Index = () => {
	const {coordinates} = useCoordinates();
	const [countdownComplete, setCountdownComplete] = useState(false);

	const getFormValues = (values) => {
		console.log("Form submitted with values:", values);
		setCountdownComplete(false);
	};
	return (
		<>
			<aside className="sliders">
			<Form
          data={data}
          showAsterisks={false}
		  disabledFormHint={"Select the starting location"}
          isDisabled={!coordinates.lat}
          onSubmit={(values) => {
            getFormValues(values);
            setCountdownComplete(false);
          }}
        />
			</aside>
			<MapSection/>
		</>
	)
}

export default Index;