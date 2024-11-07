import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faClose} from "@fortawesome/free-solid-svg-icons";
import Button from "./Button.jsx";

const Modal = ({children, onClick, headerText}) => {
	return (
		<div className={"modal-background"}>
			<div className={"modal"}>
				<div className={"modal-header"}>
					<span className={"modal-title"}>{headerText}</span>
					<Button className={"button-icon"} type={"button"} onClick={onClick}>
						<FontAwesomeIcon className={"icon"} icon={faClose} size="2x"/>
					</Button>
				</div>
				<div className={"modal-body"}>
					{children}
				</div>
			</div>
		</div>
	)
}

export default Modal;