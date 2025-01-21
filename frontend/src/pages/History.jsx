import React, {useEffect, useState} from "react";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {
	faClock, faCalendarAlt, faDollarSign, faStar, faChartBar, faFrown, faSearch, faArrowLeft, faArrowRight
} from "@fortawesome/free-solid-svg-icons";
import {useParams} from "react-router-dom";
import {axiosInternal} from "../services/axios";
import Button from "../components/simple/Button";
import {toast} from "react-toastify";
import {displayTimestamp, slimWorkingHours} from "../services/helpers.jsx";
import Form from "../components/simple/Form.jsx";
import {weekdays} from "../services/helpers.jsx";

const History = () => {
	const [requests, setRequests] = useState([]);
	const [searchQuery, setSearchQuery] = useState("");
	const {username} = useParams();
	const [editingId, setEditingId] = useState(null);
	const [bookmarkedGyms, setBookmarkedGyms] = useState({});

	// Dummy data preview/styling purposes, delete when implementing backend
	useEffect(() => {
		const retrieveRequests = async () => {
			const result = await axiosInternal("GET", "useraccount/requests");

			if (result.error) toast(result.error.message);
			else setRequests(result.data);
		}

		const retrieveBookmarks = async () => {
			const result = await axiosInternal("GET", "useraccount/bookmarks")
			if (result.error) toast(result.error.message);
			else setBookmarkedGyms(result.data);
		}

		retrieveRequests();
		retrieveBookmarks();
	}, []);

	const handleRequestClick = (id) => {
		const url = `/account/${username}/history/${id}`;
		// Open in a new tab
		window.open(url, '_blank', 'noopener,noreferrer');
	};

	const filteredRequests = requests.filter((request) => searchQuery.length > 0 ? request.name?.toLowerCase().includes(searchQuery.toLowerCase()) : true);

	const removeBookmark = async (gymId, bookmarkId) => {
		const result = await axiosInternal("DELETE", `/useraccount/bookmarks/${bookmarkId}`);
		if (result.error) toast(result.error.message);
		else {
			toast("The bookmark has successfully been removed")
			const {[gymId]: key, ...rest} = bookmarkedGyms
			setBookmarkedGyms(rest);
		}
	};

	const handleSubmit = async (id, values) => {
		const result = await axiosInternal("PUT", `useraccount/requests/${id}`, {name: values.name})
		if (result.error) toast(result.error.message);
		else {
			setRequests(requests.map((request) => {
				if (request.id === id) request.name = result.data.name;
				return request;
			}))
			setEditingId(null);
			toast(`The name of the request has successfully been edited!`)
		}
	}

	const scrollLeft = () => {
		const container = document.getElementById("gym-cards-container");
		container.scrollBy({left: -300, behavior: "smooth"});
	};

	const scrollRight = () => {
		const container = document.getElementById("gym-cards-container");
		container.scrollBy({left: 300, behavior: "smooth"});
	};


	return (
		<section className="section">
			<h2 style={{color: "#ffffff"}}>Your Past Requests:</h2>
			<div className="search-bar">
				<FontAwesomeIcon icon={faSearch} className="icon"/>
				<input
					type="text"
					placeholder="Search requests..."
					value={searchQuery}
					onChange={(e) => setSearchQuery(e.target.value)}
				/>
			</div>
			{filteredRequests.length > 0 ? (
				<table className="history-table">
					<thead>
					<tr>
						<th>
							<FontAwesomeIcon icon={faClock}/> Session Name
						</th>
						<th>
							<FontAwesomeIcon icon={faCalendarAlt}/> Request Time
						</th>
						<th>
							<FontAwesomeIcon icon={faClock}/> Preferred Departure
						</th>
						<th>
							<FontAwesomeIcon icon={faClock}/> Preferred Arrival
						</th>
						<th>
							<FontAwesomeIcon icon={faDollarSign}/> Max Price
						</th>
						<th>
							<FontAwesomeIcon icon={faStar}/> Min Rating
						</th>
						<th>
							<FontAwesomeIcon icon={faChartBar}/> Min Congestion
						</th>
						<th>
							<FontAwesomeIcon icon={faChartBar}/> Price/Time Ratio
						</th>
						<th>
							<FontAwesomeIcon icon={faClock}/> Membership Length
						</th>
					</tr>
					</thead>
					<tbody>
					{filteredRequests.map(({id, preferences, name, requestedAt}) => {
						const {
							departureTime,
							arrivalTime,
							minPrice,
							minRating,
							minCongestion,
							priceTimeRatio,
							membershipLength
						} = preferences;
						return <tr
							key={id}
							onClick={(e) => {
								if (!e.target.closest(".edit-section")) {
									handleRequestClick(id);
								}
							}}
							style={{cursor: "pointer"}}
						>
							<td>
								{editingId === id ?
									<div className="edit-section" onClick={(e) => e.stopPropagation()}>
										<Form onSubmit={(values) => handleSubmit(id, values)} data={{
											fields: [
												{
													pos: 1,
													type: "text",
													name: "name",
													value: name
												},
											],
											button: {
												type: "submit",
												text: "Save",
												className: "btn-name-edit",
											}
										}}/>
										<Button type="submit" onClick={() => setEditingId(null)}
										        className={"btn-submit"}>
											Cancel
										</Button>
									</div> :
									<span>
	                                {name || "-"}
										<Button
											onClick={(e) => {
												e.stopPropagation();
												setEditingId(id);
											}}
											className="btn-name-edit"
										>Edit</Button>
								</span>
								}
							</td>
							<td>{displayTimestamp(requestedAt) || "N/A"}</td>
							<td>{departureTime ? departureTime.substring(0, 5) : "N/A"}</td>
							<td>{arrivalTime ? arrivalTime.substring(0, 5) : "N/A"}</td>
							<td>{minPrice !== 100 ? minPrice : "N/A"}</td>
							<td>{minRating !== 1 ? minRating : "N/A"}</td>
							<td>{minCongestion !== 1 ? minCongestion : "N/A"}</td>
							<td>{`${priceTimeRatio}/${100 - priceTimeRatio}`}</td>
							<td>{membershipLength}</td>
						</tr>
					})}
					</tbody>
				</table>) : (
				<div className="no-requests">
					<FontAwesomeIcon icon={faFrown}/>
					<p>Oops! You don't have any requests yet.</p>
					<p>Why not start by making your first one? We promise it won't bite!</p>
				</div>
			)}

			<h2 style={{color: "#ffffff", marginTop: "40px"}}>Bookmarked Gyms:</h2>
			{Object.keys(bookmarkedGyms).length > 0 ? (
				<div className="bookmarked-gyms-section">
					<div id="gym-cards-container" className="bookmarked-gyms-container">
						{Object.keys(bookmarkedGyms).map((gymId) => {
								const {gym, id, createdAt} = bookmarkedGyms[gymId];

								const workingHours = slimWorkingHours(gym.workingHours);
								return <div key={gymId} className="bookmarked-gym-card">
									<div className={"bookmarked-gym-content"}>
										<h3>{gym.name}</h3>
										<p>{gym.address}</p>
										<p>
											<a
												href={`https://${gym.website}`}
												target="_blank"
												rel="noopener noreferrer"
											>
												{gym.website}
											</a>
										</p>
										<div className="membership-info">
											<strong>Memberships:</strong>
											<ul>
												<li>Monthly: {gym.monthlyMprice !== null ? `${gym.monthlyMprice} ${gym.currency}` : '-'}</li>
												<li>6-Months: {gym.sixMonthsMprice !== null ? `${gym.sixMonthsMprice} ${gym.currency}` : '-'}</li>
												<li>Yearly: {gym.yearlyMprice !== null ? `${gym.yearlyMprice} ${gym.currency}` : '-'}</li>
											</ul>
										</div>
										<p>
											{
												workingHours.map((time, index) => {
													return <div className={"bookmarked-gym-wh"}>
														<span className={"bookmarked-gym-wh-w"}>{weekdays[index]}</span>
														<span className={"bookmarked-gym-wh-h"}>{time}</span>
													</div>
												})
											}
										</p>
									</div>
									<Button className="btn-remove" onClick={() => removeBookmark(gym.id, id)}>
										Remove Bookmark
									</Button>
								</div>
							}
						)}
					</div>
					<div className="arrow-buttons">
						<button className="scroll-button" onClick={scrollLeft}>
							<FontAwesomeIcon icon={faArrowLeft}/>
						</button>
						<button className="scroll-button" onClick={scrollRight}>
							<FontAwesomeIcon icon={faArrowRight}/>
						</button>
					</div>
				</div>) : (
				<div className="no-bookmarked-gyms">
					<FontAwesomeIcon icon={faFrown} className="icon"/>
					<p>You haven't bookmarked any gyms yet!</p>
				</div>
			)}
		</section>);
};

export default History;
