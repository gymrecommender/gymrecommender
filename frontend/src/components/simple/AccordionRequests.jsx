import {useState} from 'react';
import Form from "./Form.jsx";
import {FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import {faSave} from "@fortawesome/free-solid-svg-icons";
import {displayTimestamp} from "../../services/helpers.jsx";

const AccordionRequests = ({name, requests, address, gymId, onSubmit, statuses}) => {
	const [isOpen, setIsOpen] = useState(false);

	return (
		<div className={"accordion"}>
			<div className={"accordion-header"} onClick={() => setIsOpen(!isOpen)}>
				<span className={"accordion-header-number"}>{Object.keys(requests).length}</span>
				<div className={"accordion-header-text"}>
					<span className={"accordion-header-name"}>{name}</span>
					<span className={"accordion-header-address"}>{address}</span>
				</div>
			</div>
			{isOpen ?
				<div className={"accordion-content"}>
					{
						Object.keys(requests)?.map((gymAccountId, index) => {
							const {requestTime, email, message, status} = requests[gymAccountId];
							return <div key={gymAccountId} className={"accordion-content-row"}>
								<span className={"accordion-content-email"}>{email}</span>
								<span className={"accordion-content-requestTime"}>{displayTimestamp(requestTime)}</span>
								<Form className={"accordion-form"} data={{
									fields: [
										{
											type: "text",
											name: "message",
											required: true,
											value: message ?? "",
											isBorderError: "border",
											placeholder: "Message..."
										},
										{
											type: "select",
											name: "status",
											value: status ?? "",
											isBorderError: "border",
											data: statuses
										},
									],
									fieldClass: "accordion-field",
									button: {
										type: "submit",
										text: <FontAwesomeIcon
											className={"icon"}
											size={"lg"}
											icon={faSave}
										/>,
										className: "btn-icon save",
									}
								}} onSubmit={(values) => onSubmit(values, gymId, gymAccountId)}></Form>
							</div>
						})
					}
				</div> :
				null
			}
		</div>
	)
}

export default AccordionRequests;