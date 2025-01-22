import {useParams} from "react-router-dom";
import {axiosInternal} from "../services/axios";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {
	faMapMarkerAlt,
	faDollarSign,
	faStar,
	faRoute,
	faMoneyBillWave,
	faClock, faFrown
} from "@fortawesome/free-solid-svg-icons";
import GoogleMap from "../components/simple/GoogleMap.jsx";
import {mainRatingMarker, secRatingMarket, startMarker} from "../services/markers.jsx";
import React, {useEffect, useState} from "react";
import Loader from "../components/simple/Loader.jsx";
import classNames from "classnames";
import {useSelectedGym} from "../context/SelectedGymProvider.jsx";
import {toast} from "react-toastify";

const Recommendation = ({data}) => {
	const {id} = useParams();
	const [recommendations, setRecommendations] = useState({});
	const [markers, setMarkers] = useState([]);
	const [loading, setLoading] = useState(true);
	const {gymId, setGymId} = useSelectedGym();

	useEffect(() => {
		const fetchRecommendations = async () => {
			let list;
			if (data) {
				list = data
			} else {
				const result = await axiosInternal("GET", `useraccount/requests/${id}/recommendations`)
				if (result.error) {
					toast(result.error.message);
					setLoading(false);
					return;
				} else {
					list = result.data
				}
			}
			const {requestId, latitude, longitude, ...rest} = list;
			list = rest

			const newMarkers = [{
				lat: latitude,
				lng: longitude,
				...startMarker,
				infoWindow: "Start location"
			}];
			Object.keys(list).forEach((key) => {
				const markerType = key === "mainRecommendations" ? mainRatingMarker : secRatingMarket
				list[key].forEach((gym) => {
					newMarkers.push({
						lat: gym.gym.latitude,
						lng: gym.gym.longitude,
						...markerType,
						id: gym.gym.id,
						onClick: (select) => {
							setGymId(select ? gym.gym.id : null)
						},
					})
				})
			})
			setMarkers(newMarkers);
			setRecommendations(list);
			setLoading(false);
		};

		fetchRecommendations();
	}, []);

	useEffect(() => {
		const highlightedElement = document.querySelector(".recommendation-lists").querySelector(".selected");
		if (highlightedElement) {
			highlightedElement.scrollIntoView({behavior: "smooth", block: "nearest"});
		}
	}, [gymId]);

	const content = Object.keys(recommendations).sort((a, b) => b.localeCompare(a)).map((ratingType) => {
		const mainRating = recommendations[ratingType].map((gym) => {
			return <li key={gym.gym.id} className={classNames("gym-item", gymId === gym.gym.id ? "selected" : "")}
			           onClick={() => setGymId(gymId === gym.gym.id ? null : gym.gym.id)}>
				<h4>{gym.gym.name}</h4>
				<p><FontAwesomeIcon icon={faMoneyBillWave}/> Total Cost: {gym.totalCost === -1 ? 'N/A' : `${gym.totalCost} ${gym.gym.currency}`}</p>
				<p><FontAwesomeIcon icon={faRoute}/>Travelling Time: {gym.travellingTime} min</p>
				<p><FontAwesomeIcon icon={faMapMarkerAlt}/> {gym.gym.address}</p>
				<p><FontAwesomeIcon icon={faStar}/> Overall Rating: {gym.overallRating}</p>
				<p><FontAwesomeIcon icon={faClock}/> Time Rating: {gym.timeRating}</p>
				<p><FontAwesomeIcon icon={faDollarSign}/> Cost Rating: {gym.costRating}</p>
			</li>
		});

		return <div key={ratingType} className={"recommendation-list"}>
			<>
				<h3>{ratingType === "mainRecommendations" ?
					"Main Recommendations" :
					"Secondary Recommendations"}</h3>
				<ul className="gym-list">
					{mainRating.length > 0 ? mainRating : <p>There are no recommendations to display</p>}
				</ul>
			</>
		</div>
	});

	return (
		<>
			<aside className="recommendation-lists">
				{
					loading ? <Loader type={"container"}/> : (
						content.length > 0 ? content :
						<div className={"no-content"}>
							<FontAwesomeIcon icon={faFrown}/>
							Seems like there are no recommendations for this location
						</div>
					)
				}
			</aside>
			<GoogleMap showStartMarker={false} markers={markers}/>
		</>
	);
};

export default Recommendation;
