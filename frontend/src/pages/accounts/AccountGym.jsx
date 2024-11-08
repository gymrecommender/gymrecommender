import React, {useEffect, useState} from "react";
import "../../styles/gyms.css";
import "../../styles/modal.css";
import {useNavigate} from "react-router-dom";
import GymManager from "../../components/gym/GymManager.jsx";
import Button from "../../components/simple/Button.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faCircleInfo, faPlus} from "@fortawesome/free-solid-svg-icons";
import Modal from "../../components/simple/Modal.jsx";
import GymOwnership from "../../components/gym/GymOwnership.jsx";

const AccountGym = () => {
	const [gyms, setGyms] = useState([]);
	const [currencies, setCurrencies] = useState([]);
	const [isShowRequests, setIsShowRequests] = useState(false);

	const navigate = useNavigate();
	const weekdays = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

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
				monthlyMprice: 54,
				yearlyMprice: 400,
				sixMonthsMprice: 250,
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
				monthlyMprice: 65,
				yearlyMprice: 500,
				sixMonthsMprice: 350,
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
				monthlyMprice: 72,
				yearlyMprice: 450,
				sixMonthsMprice: 330,
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
				monthlyMprice: 54,
				yearlyMprice: 450,
				sixMonthsMprice: 330,
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
				monthlyMprice: 54,
				yearlyMprice: 450,
				sixMonthsMprice: 330,
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

	const list = gyms?.map((item) => <GymManager weekdays={weekdays} key={item.id} data={item}
	                                             currencies={currencies}/>)
	return (
		<div className="section">
			<section className={"section-mg"}>
				<div className={"section-mg-hd"}>
					<span className={"section-mg-hd-title"}>Gyms under your management</span>
					<div className={"section-mg-hd-btns"}>
						<Button type={"button"} className={"btn-icon btn-action"}
						        onClick={() => setIsShowRequests(true)}>
							<span>Requests</span>
							<FontAwesomeIcon className={"icon"} icon={faCircleInfo} size={"lg"}/>
						</Button>
						<Button type={"button"} className={"btn-icon btn-action"} onClick={() => navigate("add")}>
							<span>Add</span>
							<FontAwesomeIcon className={"icon"} icon={faPlus} size={"lg"}/>
						</Button>
					</div>

				</div>
				<div className={"section-mg-list"}>
					{list}
				</div>
				{isShowRequests ? (
					<Modal headerText={"Submitted requests"} onClick={() => setIsShowRequests(false)}>
						<GymOwnership/>
					</Modal>
				) : ''}
			</section>
		</div>
	)
};

export default AccountGym;