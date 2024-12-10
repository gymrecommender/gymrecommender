import {useState} from "react";
import GymEdit from "./GymEdit.jsx";
import Gym from "./Gym.jsx";
import Button from "../simple/Button.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faPenToSquare, faLock, faCircleArrowLeft, faFolderMinus} from "@fortawesome/free-solid-svg-icons";
import Modal from "../simple/Modal.jsx";
import GymMarked from "./GymMarked.jsx";
import classNames from "classnames";

const GymManager = ({weekdays, currencies, data, onRemove}) => {
	const [isEdit, setIsEdit] = useState(false);
	const [isMarked, setIsMarked] = useState(false);

	return (
		<>
			<div className={"gym-data"}>
				<div className={classNames('gym-data-header', isMarked ? 'left' : '')}>
					{
						!isMarked ?
							<Button title={"Mark gym as unavailable"} className={"btn-icon"} type={"btn"}
							        onClick={() => setIsMarked(true)}>
								<FontAwesomeIcon className={"icon"} size={"lg"} icon={faLock}/>
							</Button> :
							<Button title={"Back"} className={"btn-icon"} type={"btn"}
							        onClick={() => setIsMarked(false)}>
								<FontAwesomeIcon className={"icon"} size={"lg"} icon={faCircleArrowLeft}/>
							</Button>
					}
					{
						!isMarked ?
							<>
								<Button title={"Edit"} className={"btn-icon"} type={"btn"} onClick={() => {
									setIsMarked(false)
									setIsEdit(true)
								}}>
									<FontAwesomeIcon className={"icon"} size={"lg"} icon={faPenToSquare}/>
								</Button>
								<Button title={"Remove from management"} className={"btn-icon"} type={"btn"}
								        onClick={() => onRemove(data.id)}>
									<FontAwesomeIcon className={"icon icon-delete"} size={"lg"} icon={faFolderMinus}/>
								</Button>
							</> :
							null
					}
				</div>
				{
					!isMarked ? <Gym weekdays={weekdays} currencies={currencies} data={data}/>
						: <GymMarked gymId={data.id} onSubmit={() => setIsMarked(false)}/>
				}
				{isEdit ? (
					<Modal headerText={"edit"} onClick={() => setIsEdit(false)}>
						<GymEdit weekdays={weekdays} currencies={currencies} data={data}/>
					</Modal>
				) : ''}
			</div>
		</>

	)
}

export default GymManager;