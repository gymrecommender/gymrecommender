const weekdays = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

const GymPopup = ({gym, children}) => {
	console.log(gym);
	return (
		<div className={"gym-popup"}>
			{children}
			<div className={"gym-popup-content"}>
				<table>
					<tbody>
					<tr>
						<td className={"gym-name"}>{gym.name}</td>
					</tr>
					<tr>
						<td className={"gym-address"}>{gym.address}</td>
					</tr>
					{gym?.website ?
						<tr>
							<td className={"gym-website"}><a href={gym.website} target={"_blank"}>Website</a></td>
						</tr> : null}
					{gym?.phoneNumber ? <tr>
						<td className={"gym-phone"}>{gym.phoneNumber}</td>
					</tr> : null}
					<tr>
						<td className={"gym-price"}>Monthly membership: {gym.monthlyMprice ? `${gym.monthlyMprice} ${gym.currency}`: '-'}</td>
					</tr>
					<tr>
						<td className={"gym-price"}>6-months membership: {gym.sixMonthsMprice ? `${gym.sixMonthsMprice} ${gym.currency}`: '-'}</td>
					</tr>
					<tr>
						<td className={"gym-price"}>Yearly membership: {gym.yearlyMprice ? `${gym.yearlyMprice} ${gym.currency}`: '-'}</td>
					</tr>
					</tbody>
				</table>
			</div>
		</div>
	)
}

export default GymPopup;