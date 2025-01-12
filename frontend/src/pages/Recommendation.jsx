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

const Recommendation = () => {
	const {id} = useParams();
	const [recommendations, setRecommendations] = useState({});
	const [markers, setMarkers] = useState([]);
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState(null);
	const {gymId, setGymId} = useSelectedGym();

	useEffect(() => {
		const fetchRecommendations = async () => {
			try {
				const dummyRecommendations = {
					"MainRecommendations": [
						{
							Gym: {
								id: "uuid1",
								name: "Maplewood gym",
								latitude: 39.7817,
								longitude: -89.6501,
								phoneNumber: "+1 (217) 555-0198",
								address: "1234 Maplewood Avenue Springfield, IL 62701, United States",
								website: "www.maplewoodmarketplace.com",
								currency: "currency uuid",
								monthlyMprice: 54,
								yearlyMprice: 450,
								sixMonthsMprice: 330,
								isWheelchairAccessible: true,
								workingHours:
									[
										{
											weekday: 0,
											openFrom: "08:00",
											openUntil: "20:00"
										},
										{
											weekday: 2,
											openFrom: "08:00",
											openUntil: "20:00"
										},
										{
											weekday: 3,
											openFrom: "08:00",
											openUntil: "20:00"
										},
										{
											weekday: 5,
											openFrom: "08:00",
											openUntil: "20:00"
										},
										{
											weekday: 6,
											openFrom: "08:00",
											openUntil: "20:00"
										},
									]
							},
							overallRating: 4.5,
							timeRating: 4,
							costRating: 4.5,
							travellingTime: 15,
							totalCost: 30,
							currency: "USD"
						},
						{
							Gym: {
								id: "uuid2",
								name: "Riverside Fitness Center",
								latitude: 34.0522,
								longitude: -118.2437,
								phoneNumber: "+1 (213) 555-0145",
								address: "4567 Riverside Drive, Los Angeles, CA 90012, United States",
								website: "www.riversidefitnessla.com",
								currency: "currency uuid",
								monthlyMprice: 65,
								yearlyMprice: 500,
								sixMonthsMprice: 350,
								isWheelchairAccessible: true,
								workingHours:
									[
										{
											weekday: 1,
											openFrom: "06:00",
											openUntil: "22:00"
										},
										{
											weekday: 2,
											openFrom: "06:00",
											openUntil: "22:00"
										},
										{
											weekday: 3,
											openFrom: "06:00",
											openUntil: "22:00"
										},
										{
											weekday: 4,
											openFrom: "06:00",
											openUntil: "22:00"
										},
										{
											weekday: 5,
											openFrom: "06:00",
											openUntil: "20:00"
										},
										{
											weekday: 6,
											openFrom: "08:00",
											openUntil: "18:00"
										}
									]
							},
							overallRating: 4,
							timeRating: 3.5,
							costRating: 4,
							travellingTime: 20,
							totalCost: 25,
							currency: "USD"
						},
						{
							Gym: {
								id: "uuid3",
								name: "Summit Wellness Center",
								latitude: 40.7128,
								longitude: -74.0060,
								phoneNumber: "+1 (212) 555-0234",
								address: "789 Summit Street, New York, NY 10001, United States",
								website: "www.summitwellnessnyc.com",
								currency: "currency uuid",
								monthlyMprice: 72,
								yearlyMprice: 450,
								sixMonthsMprice: 330,
								isWheelchairAccessible: false,
								workingHours:
									[
										{
											weekday: 1,
											openFrom: "07:00",
											openUntil: "21:00"
										},
										{
											weekday: 2,
											openFrom: "07:00",
											openUntil: "21:00"
										},
										{
											weekday: 3,
											openFrom: "07:00",
											openUntil: "21:00"
										},
										{
											weekday: 4,
											openFrom: "07:00",
											openUntil: "21:00"
										},
										{
											weekday: 5,
											openFrom: "07:00",
											openUntil: "20:00"
										},
										{
											weekday: 6,
											openFrom: "09:00",
											openUntil: "17:00"
										}
									]
							},
							overallRating: 5,
							timeRating: 5,
							costRating: 4,
							travellingTime: 10,
							totalCost: 35,
							currency: "USD"
						}
					],
					"AuxiliaryRecommendations": [
						{
							Gym: {
								id: "uuid4",
								name: "Maplewood gym",
								latitude: 39.7817,
								longitude: -89.6501,
								phoneNumber: "+1 (217) 555-0198",
								address: "1234 Maplewood Avenue Springfield, IL 62701, United States",
								website: "www.maplewoodmarketplace.com",
								currency: "currency uuid",
								monthlyMprice: 54,
								yearlyMprice: 450,
								sixMonthsMprice: 330,
								isWheelchairAccessible: true,
								workingHours:
									[
										{
											weekday: 0,
											openFrom: "08:00",
											openUntil: "20:00"
										},
										{
											weekday: 2,
											openFrom: "08:00",
											openUntil: "20:00"
										},
										{
											weekday: 3,
											openFrom: "08:00",
											openUntil: "20:00"
										},
										{
											weekday: 5,
											openFrom: "08:00",
											openUntil: "20:00"
										},
										{
											weekday: 6,
											openFrom: "08:00",
											openUntil: "20:00"
										},
									]
							},
							overallRating: 4,
							timeRating: null,
							costRating: 3.5,
							ratingScore: 4.6,
							travellingTime: 25,
							totalCost: 20,
							currency: "USD"
						},
						{
							Gym: {
								id: "uuid5",
								name: "Maplewood gym",
								latitude: 39.7817,
								longitude: -89.6501,
								phoneNumber: "+1 (217) 555-0198",
								address: "1234 Maplewood Avenue Springfield, IL 62701, United States",
								website: "www.maplewoodmarketplace.com",
								currency: "currency uuid",
								monthlyMprice: 54,
								yearlyMprice: 450,
								sixMonthsMprice: 330,
								isWheelchairAccessible: true,
								workingHours:
									[
										{
											weekday: 0,
											openFrom: "08:00",
											openUntil: "20:00"
										},
										{
											weekday: 2,
											openFrom: "08:00",
											openUntil: "20:00"
										},
										{
											weekday: 3,
											openFrom: "08:00",
											openUntil: "20:00"
										},
										{
											weekday: 5,
											openFrom: "08:00",
											openUntil: "20:00"
										},
										{
											weekday: 6,
											openFrom: "08:00",
											openUntil: "20:00"
										},
									]
							},
							overallRating: null,
							timeRating: null,
							costRating: 4,
							travellingTime: 30,
							totalCost: 40,
							currency: "USD"
						}
					]
				};

				const newMarkers = [];
				Object.keys(dummyRecommendations).forEach((key) => {
					const markerType = key === "MainRecommendations" ? mainRatingMarker : secRatingMarket
					dummyRecommendations[key].forEach((gym) => {
						newMarkers.push({
							lat: gym.Gym.latitude,
							lng: gym.Gym.longitude,
							...markerType,
							id: gym.Gym.id,
							infoWindow: <div>This is pop up</div>,
							onClick: (select) => {
								setGymId(select ? gym.Gym.id : null)
							},
						})
					})
				})
				setMarkers(newMarkers);
				setRecommendations(dummyRecommendations);
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
			highlightedElement.scrollIntoView({ behavior: "smooth", block: "nearest"});
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

	const getMarkerStyle = (ratingType) => {
		switch (ratingType) {
			case "mainRating":
				return mainRatingMarker;
			case "secRating":
				return secRatingMarket;
			case "forRatings":
				return forRatings;
			default:
				return startMarker;
		}
	};

	const content = Object.keys(recommendations).map((ratingType) => {
		const mainRating = recommendations[ratingType].map((gym) => {
			return <li key={gym.Gym.id} className={classNames("gym-item", gymId === gym.Gym.id ? "selected" : "")}
			onClick={() => setGymId(gymId === gym.Gym.id ? null : gym.Gym.id)}>
				<h4>{gym.Gym.name}</h4>
				<p><FontAwesomeIcon icon={faMoneyBillWave}/> Total Cost: {gym.totalCost} {gym.currency}</p>
				<p><FontAwesomeIcon icon={faRoute}/>Travelling Time: {gym.travellingTime} min</p>
				<p><FontAwesomeIcon icon={faMapMarkerAlt}/> {gym.Gym.address}</p>
				<p><FontAwesomeIcon icon={faStar}/> Overall Rating: {gym.overallRating}</p>
				<p><FontAwesomeIcon icon={faClock}/> Time Rating: {gym.timeRating}</p>
				<p><FontAwesomeIcon icon={faDollarSign}/> Cost Rating: {gym.costRating}</p>
			</li>
		});

		return <div key={ratingType} className={"recommendation-list"}>
			{mainRating.length > 0 ?
				<>
					<h3>{ratingType === "MainRecommendations" ?
						"Main Recommendations" :
						"Secondary Recommendations"}</h3>
					<ul className="gym-list">
						{mainRating}
					</ul>
				</> :
				<p>No complete data available</p>
			}
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
			{/*<GoogleMap markers={markers}/>*/}
		</>
	);
};

export default Recommendation;
