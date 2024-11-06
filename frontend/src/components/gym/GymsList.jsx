import GymManager from "./GymManager.jsx";

const GymsList = ({data, currencies}) => {
	const weekdays = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

	const list = data?.map((item) => <GymManager weekdays={weekdays} key={item.id} data={item}
	                                             currencies={currencies}/>)
	return (
		<section className={"section-managed-gyms"}>
			<h2 className={"section-managed-gyms-title"}>Gyms under your management</h2>
			<div className={"section-managed-gyms-list"}>
				{list}
			</div>
		</section>
	)
}

export default GymsList;