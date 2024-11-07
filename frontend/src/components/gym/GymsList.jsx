import GymManager from "./GymManager.jsx";
import Button from "../simple/Button.jsx";
import Modal from "../simple/Modal.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faPlus, faCircleInfo} from "@fortawesome/free-solid-svg-icons";
import {useState} from "react";
import GymOwnership from "./GymOwnership.jsx";

const GymsList = ({data, currencies}) => {
	const weekdays = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];
	const [isShowRequests, setIsShowRequests] = useState(false)

	const list = data?.map((item) => <GymManager weekdays={weekdays} key={item.id} data={item}
	                                             currencies={currencies}/>)
	return (
		<section className={"section-mg"}>
			<div className={"section-mg-hd"}>
				<span className={"section-mg-hd-title"}>Gyms under your management</span>
				<div className={"section-mg-hd-btns"}>
					<Button type={"button"} className={"btn-icon btn-action"} onClick={() => setIsShowRequests(true)}>
						<span>Requests</span>
						<FontAwesomeIcon className={"icon"} icon={faCircleInfo} size={"lg"}/>
					</Button>
					<Button type={"button"} className={"btn-icon btn-action"}>
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
					<GymOwnership />
				</Modal>
			): ''}
		</section>
	)
}

export default GymsList;