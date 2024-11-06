const Gym = ({weekdays, currencies, data}) => {
	const currency = currencies.find(item => {
		return item.value === data.currency
	}).label

	const workingHours = data.workingHours.reduce((acc, item) => {
		acc[item.weekday] = `${item.openFrom} - ${item.openUntil}`;
		return acc;
	}, weekdays.map((_) => "Closed"));
	return (<>
		<h2 className={"gym-data-title"}>{data.name}</h2>
		<table>
			<tbody>
			<tr className={"gym-data-row"}>
				<td className={"gym-data-row-attr-name"}>Phone number</td>
				<td className={"gym-data-row-attr-value"}>{data.phoneNumber}</td>
			</tr>
			<tr className={"gym-data-row"}>
				<td className={"gym-data-row-attr-name"}>Address</td>
				<td className={"gym-data-row-attr-value"}>{data.address}</td>
			</tr>
			<tr className={"gym-data-row"}>
				<td className={"gym-data-row-attr-name"}>Website</td>
				<td className={"gym-data-row-attr-value"}>{data.website}</td>
			</tr>
			<tr className={"gym-data-row"}>
				<td className={"gym-data-row-attr-name"}>Membership price</td>
				<td className={"gym-data-row-attr-value"}>{data.membershipPrice} {currency}</td>
			</tr>
			</tbody>
		</table>
		<div className={"gym-data-row gym-data-row-whs"}>
			<span className={"gym-data-row-whs-title"}>Working hours</span>
			<div className={"gym-data-row-whs-wd"}>
				{
					workingHours.map((item, index) => (
						<div className={"gym-data-row-wh"}>
							<span className={"gym-data-row-wh-wd"}>{weekdays[index]}</span>
							<span className={"gym-data-row-wh-h"}>{item}</span>
						</div>
					))
				}
			</div>
		</div>
	</>)
}

export default Gym;