import {displayTimestamp} from "../../services/helpers.jsx";

const Gym = ({weekdays, currencies, data}) => {
	const currency = currencies?.find(item => {
		return item.value === data.currency
	}).value

	const workingHours = data.workingHours.reduce((acc, item) => {
		let value;
		if (item.openFrom === "00:00:00" && item.openUntil === "23:59:59") {
			value = "Always open"
		} else {
			value = `${item.openFrom.substring(0, 5)} - ${item.openUntil.substring(0, 5)}`
		}
		acc[item.weekday] = value;
		return acc;
	}, weekdays.map((_) => "Closed"));

	return (
		<>
			<h2 className={"gym-data-title"}>{data.name}</h2>
			<table>
				<tbody>
				{data.phoneNumber ? <tr className={"gym-data-row"}>
					<td className={"gym-data-row-attr-name"}>Phone number</td>
					<td className={"gym-data-row-attr-value"}>{data.phoneNumber}</td>
				</tr> : ''}
				<tr className={"gym-data-row"}>
					<td className={"gym-data-row-attr-name"}>Address</td>
					<td className={"gym-data-row-attr-value"}>{data.address}</td>
				</tr>
				{data.website ? <tr className={"gym-data-row"}>
					<td className={"gym-data-row-attr-name"}>Website</td>
					<td className={"gym-data-row-attr-value"}><a href={data.website} target={"_blank"}>{data.website}</a></td>
				</tr> : ''}
				{data.monthlyMprice ? <tr className={"gym-data-row"}>
					<td className={"gym-data-row-attr-name"}>Monthly membership</td>
					<td className={"gym-data-row-attr-value"}>{data.monthlyMprice} {currency}</td>
				</tr> : ''}
				{data.sixMonthsMprice ? <tr className={"gym-data-row"}>
					<td className={"gym-data-row-attr-name"}>6-months membership</td>
					<td className={"gym-data-row-attr-value"}>{data.sixMonthsMprice} {currency}</td>
				</tr> : ''}
				{data.yearlyMprice ? <tr className={"gym-data-row"}>
					<td className={"gym-data-row-attr-name"}>Yearly membership</td>
					<td className={"gym-data-row-attr-value"}>{data.yearlyMprice} {currency}</td>
				</tr> : ''}
				</tbody>
			</table>
			<div className={"gym-data-row gym-data-row-whs"}>
				<span className={"gym-data-row-whs-title"}>Working hours</span>
				<div className={"gym-data-row-whs-wd"}>
					{
						workingHours.map((item, index) => (
							<div key={index} className={"gym-data-row-wh"}>
								<span className={"gym-data-row-wh-wd"}>{weekdays[index]}</span>
								<span className={"gym-data-row-wh-h"}>{item}</span>
							</div>
						))
					}
				</div>
			</div>
		</>
	)
}

export default Gym;