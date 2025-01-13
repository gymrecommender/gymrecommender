const GymPopup = ({gym, children}) => {
	return (
		<div className={"gym-popup"}>
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
					</tbody>
				</table>
			</div>
			{children}
		</div>
	)
}

export default GymPopup;