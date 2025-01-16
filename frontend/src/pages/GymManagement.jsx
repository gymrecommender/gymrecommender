import React, {useEffect, useState} from "react";
import "../styles/gyms.css";
import "../styles/modal.css";
import {useNavigate} from "react-router-dom";
import GymManager from "../components/gym/GymManager.jsx";
import Button from "../components/simple/Button.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faCircleInfo, faPlus} from "@fortawesome/free-solid-svg-icons";
import Modal from "../components/simple/Modal.jsx";
import GymOwnership from "../components/gym/GymOwnership.jsx";
import {toast} from "react-toastify";
import {useConfirm} from "../context/ConfirmProvider.jsx";
import {axiosInternal} from "../services/axios.jsx";

const GymManagement = () => {
	const [gyms, setGyms] = useState([]);
	const [currencies, setCurrencies] = useState([]);
	const [isShowRequests, setIsShowRequests] = useState(false);
	const {flushData, setValues} = useConfirm();

	const navigate = useNavigate();
	const weekdays = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

	useEffect(() => {
		const retrieveCurrencies = async () => {
			const result = await axiosInternal("GET", "gym/currencies")

			if (result.error) toast(result.error.message);
			else setCurrencies(result.data);
		}

		const retrieveOwnedGyms = async () => {
			const result = await axiosInternal("GET", "gymaccount/owned")

			if (result.error) toast(result.error.message);
			else setGyms(result.data);
		}

		retrieveCurrencies();
		retrieveOwnedGyms();
	}, [])

	const onConfirm = async (gymId, gymName) => {
		flushData();
		const result = await axiosInternal("PUT", `gymaccount/ownership/${gymId}`);
		if (result.error) toast(result.error.message);
		else {
			setGyms(gyms?.reduce((acc, gym) => {
				if (gymId !== gym.id) {
					acc.push(gym)
				}
				return acc
			}, []));
			toast(`You no longer manage ${gymName}!`)
		}
	}

	const list = gyms?.map((gym) => {
		return <GymManager key={gym.id}
		                   onRemove={() => {
			                   setValues(
				                   true,
				                   `Are you sure that you want to stop managing ${gym.name}?`,
				                   () => onConfirm(gym.id, gym.name),
				                   flushData
			                   );
		                   }}
		                   weekdays={weekdays} data={gym}
		                   currencies={currencies}/>
	})
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
						<Button type={"button"} className={"btn-icon btn-action"} onClick={() => navigate("request")}>
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

export default GymManagement;