import "../../styles/confirm.css"
import Button from "./Button.jsx";
import {useConfirm} from "../../context/ConfirmProvider.jsx";

const Confirm = () => {
	const {data} = useConfirm();
	const {message, onConfirm, onCancel} = data;

	return (
		<div className={"confirm-window"}>
			<div className={"confirm-content"}>
				<div className={"confirm-message"}>
					{message}
				</div>
				<div className={"confirm-buttons"}>
				<Button className={"btn-icon btn-confirm btn-action"} onClick={onConfirm}>
					Yes
				</Button>
				<Button className={"btn-icon btn-confirm btn-action"} onClick={onCancel}>
					Cancel
				</Button>
			</div>
			</div>
		</div>
	)
}

export default Confirm;