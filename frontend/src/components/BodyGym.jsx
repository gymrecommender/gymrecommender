import React, {useState, useEffect, createContext} from 'react';
import "../styles/gyms.css";
import Input from './simple/Input.jsx';
import Button from './simple/Button.jsx';
import LocationControls from './LocationControls.jsx';
import Map from './simple/Map.jsx';
import GymsList from "./gym/GymsList.jsx";
import "../styles/modal.css";

const BodyGym = () => {
	const [gyms, setGyms] = useState([]);
	const [currencies, setCurrencies] = useState([]);

	useEffect(() => {
		//TODO logic to retrieve gyms
		//TODO logic to retrieve currencies
		setCurrencies([
			{
				value: "currency uuid2",
				label: "EUR",
				name: "Euro"
			},
			{
				value: "currency uuid",
				label: "USD",
				name: "Dollar"
			}
		]);
		setGyms([
			{
				id: "uuid1",
				name: "Maplewood gym",
				latitude: 39.7817,
				longitude: -89.6501,
				phoneNumber: "+1 (217) 555-0198",
				address: "1234 Maplewood Avenue Springfield, IL 62701, United States",
				website: "www.maplewoodmarketplace.com",
				currency: "currency uuid",
				membershipPrice: 54,
				isWheelchairAccessible: true,
				workingHours: [
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
			{
				id: "uuid2",
				name: "Riverside Fitness Center",
				latitude: 34.0522,
				longitude: -118.2437,
				phoneNumber: "+1 (213) 555-0145",
				address: "4567 Riverside Drive, Los Angeles, CA 90012, United States",
				website: "www.riversidefitnessla.com",
				currency: "currency uuid",
				membershipPrice: 65,
				isWheelchairAccessible: true,
				workingHours: [
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
			{
				id: "uuid3",
				name: "Summit Wellness Center",
				latitude: 40.7128,
				longitude: -74.0060,
				phoneNumber: "+1 (212) 555-0234",
				address: "789 Summit Street, New York, NY 10001, United States",
				website: "www.summitwellnessnyc.com",
				currency: "currency uuid",
				membershipPrice: 72,
				isWheelchairAccessible: false,
				workingHours: [
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
			{
				id: "uuid4",
				name: "Maplewood gym",
				latitude: 39.7817,
				longitude: -89.6501,
				phoneNumber: "+1 (217) 555-0198",
				address: "1234 Maplewood Avenue Springfield, IL 62701, United States",
				website: "www.maplewoodmarketplace.com",
				currency: "currency uuid",
				membershipPrice: 54,
				isWheelchairAccessible: true,
				workingHours: [
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
			{
				id: "uuid5",
				name: "Maplewood gym",
				latitude: 39.7817,
				longitude: -89.6501,
				phoneNumber: "+1 (217) 555-0198",
				address: "1234 Maplewood Avenue Springfield, IL 62701, United States",
				website: "www.maplewoodmarketplace.com",
				currency: "currency uuid",
				membershipPrice: 54,
				isWheelchairAccessible: true,
				workingHours: [
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
		])
	}, [])

	const handleSubmitSearch = (value) => {
		console.log(value);
	};

	const handleGetLocation = () => {
		console.log("Get location");
		// Add logic to get current location here
	};

	return (
		<div className="section-body">
			<GymsList data={gyms} currencies={currencies}/>

			{/*/!* Request to Own a GymEdit Form *!/*/}
			{/*<div className="section section-request">*/}
			{/*  <h3>Request to Own a GymEdit:</h3>*/}
			{/*  <form className="request-form">*/}
			{/*      <div className="form-row">*/}
			{/*          <div className="form-item">*/}
			{/*              <label htmlFor="gym-name">GymEdit Name:</label>*/}
			{/*              <Input*/}
			{/*                  type="text"*/}
			{/*                  name="gym-name"*/}
			{/*                  placeholder="Enter gym name"*/}
			{/*                  className="input-box"*/}
			{/*                  required*/}
			{/*              />*/}
			{/*          </div>*/}
			{/*          <div className="form-item">*/}
			{/*              <label htmlFor="gym-location">GymEdit Location:</label>*/}
			{/*              <LocationControls onGetLocation={handleGetLocation} onSubmitSearch={handleSubmitSearch} />*/}
			{/*              <Map coordinates={{ lon, lat }} />*/}
			{/*              <Button type="submit" className="btn-submit">Submit Request</Button>*/}
			{/*          </div>*/}
			{/*      </div>*/}
			{/*      */}
			{/*      */}
			{/*  </form>*/}
			{/*</div>*/}

			{/*/!* List of Managed Gyms *!/*/}
			{/*<div className="section section-mg">*/}
			{/*  <h3>List of Managed Gyms:</h3>*/}
			{/*  <div className="accordion-item">*/}
			{/*    <button className="accordion-btn">GymEdit 1</button>*/}
			{/*    <div className="accordion-content">*/}
			{/*      <p>Details about GymEdit 1...</p>*/}
			{/*      <form className="update-form">*/}
			{/*        <label htmlFor="update-info-1">Update GymEdit Information:</label>*/}
			{/*        <Input*/}
			{/*          type="text"*/}
			{/*          name="update-info-1"*/}
			{/*          placeholder="Update details"*/}
			{/*          className="input-box"*/}
			{/*        />*/}
			{/*        <Button type="submit" className="btn-submit">Update</Button>*/}
			{/*      </form>*/}
			{/*    </div>*/}
			{/*  </div>*/}

			{/*  <div className="accordion-item">*/}
			{/*    <button className="accordion-btn">GymEdit 2</button>*/}
			{/*    <div className="accordion-content">*/}
			{/*      <p>Details about GymEdit 2...</p>*/}
			{/*      <form>*/}
			{/*        <label htmlFor="update-info-2">Update GymEdit Information:</label>*/}
			{/*        <input type="text" id="update-info-2" name="update-info-2" placeholder="Update details" />*/}
			{/*        <button type="submit" className="btn-submit">Update</button>*/}
			{/*      </form>*/}
			{/*    </div>*/}
			{/*  </div>*/}
			{/*</div>*/}

			{/*/!* Marking Gyms as Unavailable *!/*/}
			{/*<div className="section section-unavailable">*/}
			{/*  <h3>Mark GymEdit as Unavailable: </h3>*/}
			{/*  <form className="unavailable-form">*/}
			{/*    <label htmlFor="select-gym">Select GymEdit:</label>*/}
			{/*    <select id="select-gym" name="select-gym" className="input-box">*/}
			{/*      <option value="gym1">GymEdit 1</option>*/}
			{/*      <option value="gym2">GymEdit 2</option>*/}
			{/*    </select>*/}

			{/*    <div className="time">*/}
			{/*      <label htmlFor="unavailable-from">Unavailable From:</label>*/}
			{/*      <Input type="time" id="unavailable-from" name="unavailable-from" className="input-box" required />*/}

			{/*      <label htmlFor="unavailable-to">Unavailable To:</label>*/}
			{/*      <Input type="time" id="unavailable-to" name="unavailable-to" className="input-box" required />*/}
			{/*    </div>*/}

			{/*    <Button type="submit" className="btn-submit">Mark as Unavailable</Button>*/}
			{/*  </form>*/}
			{/*</div>*/}


		</div>
	);
};

export default BodyGym;
