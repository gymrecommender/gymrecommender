import Input from "../simple/Input.jsx";
import Select from "../simple/Select.jsx";
import {useState, useEffect} from "react";
import Button from "../simple/Button.jsx";

const GymEdit = ({data, currencies, weekdays}) => {
	const [formFields, setFormFields] = useState({});

	useEffect(() => {
		const {id, longitude, latitude, workingHours, ...rest} = data;
		setFormFields({
			...rest, ...workingHours.reduce((acc, {weekday, openFrom, openUntil}) => {
				acc[`${weekday}-openFrom`] = openFrom;
				acc[`${weekday}-openUntil`] = openUntil;
				return acc;
			}, Object.fromEntries(
				weekdays.map((_, index) => [
					[`${index}-openFrom`, null],  // Correct key-value pair for openFrom
					[`${index}-openUntil`, null]  // Correct key-value pair for openUntil
				]).flat()))
		})
	}, [data]);

	const handleChange = (name, value) => {
		setFormFields({...formFields, [name]: value});
	}
	const handleSubmit = (e) => {
		e.preventDefault();
		console.log(formFields);
	}
	return (
		<div className={"gym-data"}>
			<form onSubmit={handleSubmit}>
				<Input
					label={'name'}
					type={"text"}
					name={"name"}
					value={formFields.name}
					onChange={handleChange}
				/>
				<Input
					label={'phone number'}
					type={"phone"}
					name={"phoneNumber"}
					value={formFields.phoneNumber}
					onChange={handleChange}
				/>
				<Input
					label={'address'}
					type={"text"}
					name={"address"}
					value={formFields.address}
					onChange={handleChange}
				/>
				<Input
					label={'website'}
					type={"text"}
					name={"website"}
					value={formFields.website}
					onChange={handleChange}
				/>
				<div className={"gym-price"}>
					<Input
						label={'Monthly membership'}
						type={"number"}
						min={0}
						name={"monthlyMprice"}
						value={formFields.monthlyMprice}
						onChange={handleChange}
					/>
					<Select data={currencies}
					        value={formFields.currency}
					        name={"currency"}
					        onChange={handleChange}
					/>
				</div>
				<div className={"gym-price"}>
					<Input
						label={'6-months membership'}
						type={"number"}
						min={0}
						name={"sixMonthsMprice"}
						value={formFields.sixMonthsMprice}
						onChange={handleChange}
					/>
					<Select data={currencies}
					        value={formFields.currency}
					        name={"currency"}
					        onChange={handleChange}
					/>
				</div>
				<div className={"gym-price"}>
					<Input
						label={'Yearly membership'}
						type={"number"}
						min={0}
						name={"yearlyMprice"}
						value={formFields.yearlyMprice}
						onChange={handleChange}
					/>
					<Select data={currencies}
					        value={formFields.currency}
					        name={"currency"}
					        onChange={handleChange}
					/>
				</div>
				<div className={"gym-working-hours"}>
					{
						weekdays.map((name, index) => {
							return (
								<div className={'gym-whs-wd'}>
									<span className={'gym-whs-wd-name'}>{name}</span>
									<div className={'gym-whs-wd-time'}>
										<Input
											label={"open from"}
											type={"time"}
											name={`${index}-openFrom`}
											value={formFields[`${index}-openFrom`]}
											onChange={handleChange}
										/>
										<Input
											label={"open until"}
											type={"time"}
											name={`${index}-openUntil`}
											value={formFields[`${index}-openUntil`]}
											onChange={handleChange}
										/>
									</div>
								</div>
							)
						})
					}
				</div>
				<Button className={"btn-submit"} type={"submit"} onSubmit={handleSubmit}>
					Save
				</Button>
			</form>
		</div>
	)
}

export default GymEdit;