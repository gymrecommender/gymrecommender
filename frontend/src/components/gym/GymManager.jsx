import {useState} from "react";
import GymEdit from "./GymEdit.jsx";
import Gym from "./Gym.jsx";
import Button from "../simple/Button.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faPenToSquare, faFolderMinus} from "@fortawesome/free-solid-svg-icons";
import Modal from "../simple/Modal.jsx";
import classNames from "classnames";

const GymManager = ({weekdays, currencies, data, onRemove, handleEditSubmit}) => {
	const [isEdit, setIsEdit] = useState(false);

	return (
		<>
			<div className={"gym-data"}>
				<div className={classNames('gym-data-header')}>
					<>
						<Button title={"Edit"} className={"btn-icon"} type={"btn"} onClick={() => {
							setIsEdit(true)
						}}>
							<FontAwesomeIcon className={"icon"} size={"lg"} icon={faPenToSquare}/>
						</Button>
						<Button title={"Remove from management"} className={"btn-icon"} type={"btn"}
						        onClick={() => onRemove(data.id)}>
							<FontAwesomeIcon className={"icon icon-delete"} size={"lg"} icon={faFolderMinus}/>
						</Button>
					</>
				</div>
				<Gym weekdays={weekdays} currencies={currencies} data={data}/>
				{isEdit ? (
					<Modal headerText={"edit"} onClick={() => setIsEdit(false)}>
						<GymEdit weekdays={weekdays} currencies={currencies} data={data} handleSubmit={handleEditSubmit}/>
					</Modal>
				) : ''}
			</div>
		</>

	)
}

export default GymManager;