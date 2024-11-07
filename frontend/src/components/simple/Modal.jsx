import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faClose} from "@fortawesome/free-solid-svg-icons";
import Button from "./Button.jsx";

const Modal = ({children, onClick, headerText}) => {
	document.body.style.overflowY = "hidden";
	const handleClose = () => {
		document.body.style.overflowY = "scroll";
		onClick();
	}
	return (
		<div className={"modal-background"}>
			<div className={"modal"}>
				<div className={"modal-header"}>
					<span className={"modal-title"}>{headerText}</span>
					<Button className={"btn-icon"} type={"button"} onClick={handleClose}>
						<FontAwesomeIcon className={"icon"} icon={faClose} size="lg"/>
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