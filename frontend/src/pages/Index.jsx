import MapSection from '../components/MapSection.jsx';
import React, {useEffect, useState} from "react";
import Form from "../components/simple/Form.jsx";
import {useCoordinates} from "../context/CoordinatesProvider.jsx";
import moment from "moment";
import {axiosInternal} from "../services/axios.jsx";
import {toast} from "react-toastify";
import {useNavigate} from "react-router-dom";
import {useFirebase} from "../context/FirebaseProvider.jsx";
import Loader from "../components/simple/Loader.jsx";
import Recommendation from "./Recommendation.jsx";
import {SelectedGymProvider} from "../context/SelectedGymProvider.jsx";

const membershipTypes = [
	{value: "month", label: "1 month"},
	{value: "halfyear", label: "6 months"},
	{value: "year", label: "Year"},
]

const data = {
	fields: [
		{pos: 0, type: "title", text: "What is your priority?"},
		{
			pos: 1,
			type: "range",
			required: true,
			name: "priceRatingPriority",
			step: 5,
			min: 0,
			max: 100,
			value: 50,
			minText: "Price",
			maxText: "Time",
			isSplit: true,
		},
		{
			pos: 2,
			type: "select",
			required: true,
			data: membershipTypes,
			value: 'month',
			name: "membershipLength",
			label: "Membership length"
		},
		{pos: 3, type: "title", text: "Tell us your preferences!"},
		{
			pos: 4,
			type: "time",
			label: "Preferred departure time",
			name: "preferredDepartureTime",
			className: "time",
			wClassName: "time"
		},
		{
			pos: 6,
			type: "range",
			label: "Max membership price",
			name: "maxMembershipPrice",
			step: 10,
			min: 0,
			max: 1000,
			value: 1000
		},
		{
			pos: 7,
			type: "range",
			label: "Min overall rating",
			name: "minOverallRating",
			step: 0.5,
			min: 1,
			max: 5,
			value: 1
		},
		{
			pos: 8,
			type: "range",
			label: "Min congestion rating",
			name: "minCongestionRating",
			step: 0.5,
			min: 1,
			max: 5,
			value: 1
		},
	],
	button: {
		type: "submit",
		text: "Apply",
		className: "btn-submit",
	}
};
const Index = () => {
	const {coordinates} = useCoordinates();
	const navigate = useNavigate();
	const {getUser} = useFirebase();
	const [recommendations, setRecommendations] = useState({});
	const [showLoader, setShowLoader] = useState(false);
	const [pauseLength, setPauseLength] = useState(null);

	useEffect(() => {
		const retrievePause = async () => {
			const url = getUser()?.role === "user" ? 'useraccount/pause' : 'gym/pause';
			const result = await axiosInternal("GET", url);
			if (result.error) {
				toast(result.error.message);
				setPauseLength(0);
			}
			else {
				const sec = moment.duration(result.data.timeRemaining).asSeconds()
				setPauseLength(sec);
			}
		}

		retrievePause();
	}, [])

	const getFormValues = async (values) => {
		values.longitude = coordinates.lng;
		values.latitude = coordinates.lat;

		setShowLoader(true);
		const result = await axiosInternal("POST", "recommendation", values);
		setShowLoader(false);

		// if (result.error) {
		// 	toast(result.error.message);
		// 	return;
		// }
		// else {
		// 	const user = getUser();
		// 	if (user && result.data.requestId) navigate(`account/history/${result.data.requestId}`);
		//
		// 	setRecommendations(result.data)
		// }

		const url = getUser()?.role === "user" ? 'useraccount/pause' : 'gym/pause';
		const res = await axiosInternal("POST", url);
		if (res.error) toast(res.error.message);
		else return moment.duration(res.data.timeRemaining).asSeconds();
	};
	return (
		<>
			{
				Object.keys(recommendations).length === 0 ?
					<>
						<aside className="sliders">
							{pauseLength !== null ?
								<Form
									data={data}
									showAsterisks={false}
									disabledFormHint={"Select the starting location"}
									isDisabled={!coordinates.lat}
									countdownStart={pauseLength}
									onSubmit={async (values) => await getFormValues(values)}
								/> : <Loader type={"container"}/>}
						</aside>
						<MapSection/>
						{showLoader ? <Loader type={"hover"}/> : null}
					</> :
					<SelectedGymProvider>
						<Recommendation data={recommendations}/>
					</SelectedGymProvider>
			}
		</>
	)
}

export default Index;