import MapSection from '../components/MapSection.jsx';
import React, {useEffect, useState} from "react";
import Form from "../components/simple/Form.jsx";
import {CoordinatesProvider, useCoordinates} from "../context/CoordinatesProvider.jsx";
import {axiosInternal} from "../services/axios.jsx";
import {toast} from "react-toastify";
import {forRatings, ownedGyms, pendingGyms} from "../services/markers.jsx";
import GymPopup from "../components/gym/GymPopup.jsx";
import Button from "../components/simple/Button.jsx";
import Loader from "../components/simple/Loader.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faBookmark, faStar} from "@fortawesome/free-regular-svg-icons";
import {faBookmark as solidFaBookmark, faStar as solidFaStar} from "@fortawesome/free-solid-svg-icons";
import Modal from "../components/simple/Modal.jsx";

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
		{
			pos: 2,
			type: "range",
			label: "Overall Rating",
			name: "rating",
			step: 1,
			min: 1,
			max: 5,
			value: 3,
			required: true
		},
		{
			pos: 3,
			type: "select",
			required: true,
			label: "Average waiting time for a machine or space",
			data: wRating,
			name: "waitingTime",
		},
		{
			pos: 4,
			type: "select",
			required: true,
			label: "How crowded the gym feels",
			data: cRating,
			name: "crowdedness"
		},
	],
	button: {
		type: "submit",
		text: "Apply",
		className: "btn-submit",
	}
}

const UserRating = () => {
	const {coordinates} = useCoordinates();
	const [loading, setLoading] = useState(false);
	const [markers, setMarkers] = useState([]);
	const [ratings, setRatings] = useState([]);
	const [bookmarks, setBookmarks] = useState([]);
	const [rateGym, setRateGym] = useState({});

	const handleSubmit = async (values) => {
		values["visitTime"] = values["visitTime"] + ':00'
		const result = await axiosInternal("POST", `useraccount/ratings/${rateGym.id}`, values);
		if (result.error) toast(result.error.message);
		else {
			toast("Your rating has been successfully submitted")
			setRatings({...ratings, ...result.data});
			setRateGym({});
		}
	}

	const handleBookmarkSubmit = async (isBookmarked, gymId) => {
		if (isBookmarked) {
			const result = await axiosInternal("DELETE", `useraccount/bookmarks/${bookmarks[gymId].id}`)
			if (result.error) toast(result.error.message);
			else {
				toast("The bookmark has been successfully removed")
				const {[gymId]: key, ...rest} = bookmarks;
				setBookmarks({...rest});
			}
		} else {
			const result = await axiosInternal("POST", `useraccount/bookmarks`, {gymId})
			if (result.error) toast(result.error.message);
			else {
				toast("The bookmark has been successfully added")
				setBookmarks({...bookmarks, ...result.data});
			}
		}
	}

	useEffect(() => {
		const getRatings = async () => {
			const result = await axiosInternal("GET", "useraccount/ratings")
			if (result.error) toast(result.error.message);
			else setRatings(result.data);
		}

		const getBookmarks = async () => {
			const result = await axiosInternal("GET", "useraccount/bookmarks")
			if (result.error) toast(result.error.message);
			else setBookmarks(result.data);
		}

		getRatings();
		getBookmarks();
	}, []);

	useEffect(() => {
		const getGyms = async () => {
			setLoading(true);
			const result = await axiosInternal("GET", "gym/location", {}, {
				lat: coordinates.lat,
				lng: coordinates.lng
			});
			if (result.data.error) {
				toast(result.error.message);
				setLoading(false);
				return;
			}

			setMarkers(result.data.value.map((gym) => {
				const isBookmarked = bookmarks.hasOwnProperty(gym.id);
				const isRated = ratings.hasOwnProperty(gym.id);
				return ({
					lat: gym.latitude,
					lng: gym.longitude,
					...forRatings,
					id: gym.id,
					infoWindow: <GymPopup gym={gym}>
						<div className={"gym-popup-header"}>
							<Button disabled={isRated}
							        title={isRated ? "You have already rated the gym" : "Rate the gym"}
							        type={"button"} className={"btn-icon-dark"} onClick={() => {
								setRateGym({id: gym.id, name: gym.name})
							}}>
								<FontAwesomeIcon size={"lg"} className={"icon"} icon={isRated ? solidFaStar : faStar}/>
								{isRated ? <span>{ratings[gym.id].rating.rating}</span> : null}
							</Button>
							<Button title={isBookmarked ? "Remove the bookmark" : "Bookmark the gym"}
							        type={"button"} className={"btn-icon-dark"} onClick={() => {
								handleBookmarkSubmit(isBookmarked, gym.id)
							}}>
								<FontAwesomeIcon size={"lg"} className={"icon"}
								                 icon={isBookmarked ? solidFaBookmark : faBookmark}/>
							</Button>
						</div>
					</GymPopup>,
				})
			}, []))

			setLoading(false);
		}

		if (coordinates.lat && coordinates.lng) getGyms();
	}, [coordinates, ratings, bookmarks]);

	return (
		<>
			<MapSection markers={markers} showStartMarker={false} forceClick={true}/>
			{loading ? <Loader type={"hover"}/> : null}

			{Object.keys(rateGym).length > 0 ?
				<Modal headerText={`Rate  '${rateGym.name}'`} onClick={() => setRateGym({})}>
					<Form data={data} showAsterisks={false} onSubmit={handleSubmit}/>
				</Modal> : null}
		</>
	);
}

export default UserRating;