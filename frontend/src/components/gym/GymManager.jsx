import {useState} from "react";
import GymEdit from "./GymEdit.jsx";
import Gym from "./Gym.jsx";
import Button from "../simple/Button.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faPenToSquare, faCircleArrowLeft} from "@fortawesome/free-solid-svg-icons";

const GymManager = ({weekdays, currencies, data}) => {
	const [isEdit, setIsEdit] = useState(false);

	return (
		<div className={"gym-data"}>
			<div className={`gym-data-header ${isEdit ? "left" : ''}`}>
				<Button className={"button-icon"} type={"button"} onClick={() => {
					setIsEdit(!isEdit)
				}}>
					{isEdit ?
						<>
							<FontAwesomeIcon className={"icon"} size={"2x"} icon={faCircleArrowLeft}/>
						</> :
						<FontAwesomeIcon className={"icon"} size={"2x"} icon={faPenToSquare}/>
					}
				</Button>
			</div>
			{isEdit ?
				<GymEdit weekdays={weekdays} currencies={currencies} data={data}/> :
				<Gym weekdays={weekdays} currencies={currencies} data={data}/>
			}
		</div>
	)
}

export default GymManager;