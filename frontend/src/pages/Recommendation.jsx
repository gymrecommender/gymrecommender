import {useParams} from "react-router-dom";
import {axiosInternal} from "../services/axios";
import {CoordinatesProvider} from "../context/CoordinatesProvider.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {
	faMapMarkerAlt,
	faDollarSign,
	faStar,
	faRoute,
	faMoneyBillWave,
	faClock
} from "@fortawesome/free-solid-svg-icons";
import GoogleMap from "../components/simple/GoogleMap.jsx";
import {mainRatingMarker, secRatingMarket} from "../services/markers.jsx";
import {useEffect, useState} from "react";
import Loader from "../components/simple/Loader.jsx";
import classNames from "classnames";
import {useSelectedGym} from "../context/SelectedGymProvider.jsx";

const Recommendation = ({data}) => {
	const {id} = useParams();
	const [recommendations, setRecommendations] = useState({});
	const [markers, setMarkers] = useState([]);
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState(null);
	const {gymId, setGymId} = useSelectedGym();

	useEffect(() => {
		const fetchRecommendations = async () => {
			try {
				let list;
				console.log(data)
				if (data) {
					const {requestId, ...rest} = data;
					list = rest
				} else {
					// TODO do the request to retrieve recommendations
				}
				const newMarkers = [];
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
			} catch (err) {
				setError(err.message || "An error occurred while fetching data.");
			} finally {
				setLoading(false);
			}
		};

		fetchRecommendations();
	}, []);

	useEffect(() => {
		const highlightedElement = document.querySelector(".recommendation-lists").querySelector(".selected");
		if (highlightedElement) {
			highlightedElement.scrollIntoView({behavior: "smooth", block: "nearest"});
		}
	}, [gymId]);

	/*useEffect(() => {
	  const fetchRecommendations = async () => {
		setLoading(true);
		setError(null);

		try {
		  const response = await axiosInternal.get("/api/recommendations");
		  setRecommendations(response.data);
		} catch (err) {
		  setError(err.message || "An error occurred while fetching data.");
		} finally {
		  setLoading(false);
		}
	  };

	  fetchRecommendations();
	}, []);*/

	const content = Object.keys(recommendations).map((ratingType) => {
		const mainRating = recommendations[ratingType].map((gym) => {
			return <li key={gym.gym.id} className={classNames("gym-item", gymId === gym.gym.id ? "selected" : "")}
			           onClick={() => setGymId(gymId === gym.gym.id ? null : gym.gym.id)}>
				<h4>{gym.gym.name}</h4>
				<p><FontAwesomeIcon icon={faMoneyBillWave}/> Total Cost: {gym.totalCost} {gym.currency}</p>
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
					{mainRating.length > 0 ? mainRating : <p>No complete data available</p>}
				</ul>
			</>
		</div>
	});

	return (
		<>
			<aside className="recommendation-lists">
				{
					loading ? (
						<Loader type={"container"}/>
					) : error ? (
						<p>Error: {error}</p>
					) : content
				}
			</aside>
			<GoogleMap markers={markers}/>
		</>
	);
};

export default Recommendation;
