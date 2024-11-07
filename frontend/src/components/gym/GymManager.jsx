import {useState, useContext} from "react";
import GymEdit from "./GymEdit.jsx";
import Gym from "./Gym.jsx";
import Button from "../simple/Button.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faPenToSquare} from "@fortawesome/free-solid-svg-icons";
import Modal from "../simple/Modal.jsx";

const GymManager = ({weekdays, currencies, data}) => {
	const [isEdit, setIsEdit] = useState(false);

	return (
		<>
			<div className={"gym-data"}>
				<div className={`gym-data-header`}>
					<Button className={"button-icon"} type={"button"} onClick={() => {
						setIsEdit(!isEdit)
						document.body.style.overflowY = 'hidden';
					}}>
						<FontAwesomeIcon className={"icon"} size={"2x"} icon={faPenToSquare}/>
					</Button>
				</div>
				<Gym weekdays={weekdays} currencies={currencies} data={data}/>
				{isEdit ? (
					<Modal headerText={"edit"} onClick={() => {
						setIsEdit(false)
						document.body.style.overflowY = 'scroll';
					}}>
						<div className={"gym-data"}>
							<GymEdit weekdays={weekdays} currencies={currencies} data={data}/>
						</div>
					</Modal>
				) : ''}
			</div>
		</>

	)
}

export default GymManager;