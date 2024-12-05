import Input from "../simple/Input.jsx";
import Select from "../simple/Select.jsx";
import Button from "../simple/Button.jsx";
import {FormProvider, useForm} from "react-hook-form";
import {sanitizeData} from "../../services/helpers.jsx";

const GymEdit = ({data, currencies, weekdays}) => {
	const {id, longitude, latitude, workingHours, ...rest} = data;
	const methods = useForm({
		defaultValues: {
			...rest, ...workingHours.reduce((acc, {weekday, openFrom, openUntil}) => {
				acc[`${weekday}-openFrom`] = openFrom;
				acc[`${weekday}-openUntil`] = openUntil;
				return acc;
			}, Object.fromEntries(
				weekdays.map((_, index) => [
					[`${index}-openFrom`, null],  // Correct key-value pair for openFrom
					[`${index}-openUntil`, null]  // Correct key-value pair for openUntil
				]).flat()))
		}
	});

	const customHandleSubmit = (values) => {
		const sanitizedData = sanitizeData(values);
		console.log(sanitizedData);
	}

	return (
		<div className={"gym-data"}>
			<FormProvider {...methods}>
				<form noValidate={true} onSubmit={methods.handleSubmit(customHandleSubmit)}>
					<Input
						label={'name'}
						type={"text"}
						name={"name"}
						required={true}
					/>
					<Input
						label={'phone number'}
						type={"phone"}
						name={"phoneNumber"}
						required={true}
					/>
					<Input
						label={'address'}
						type={"text"}
						name={"address"}
						required={true}
					/>
					<Input
						label={'website'}
						type={"text"}
						name={"website"}
						required={true}
					/>
					<div className={"gym-price"}>
						<Input
							label={'Monthly membership'}
							type={"number"}
							min={0}
							name={"monthlyMprice"}
							required={true}
						/>
						<Select data={currencies}
						        name={"currency"}
						/>
					</div>
					<div className={"gym-price"}>
						<Input
							label={'6-months membership'}
							type={"number"}
							min={0}
							name={"sixMonthsMprice"}
							required={true}
						/>
						<Select data={currencies}
						        name={"currency"}
						/>
					</div>
					<div className={"gym-price"}>
						<Input
							label={'Yearly membership'}
							type={"number"}
							min={0}
							name={"yearlyMprice"}
							required={true}
						/>
						<Select data={currencies}
						        name={"currency"}
						/>
					</div>
					<div className={"gym-working-hours"}>
						{
							weekdays.map((name, index) => {
								return (
									<div key={index} className={'gym-whs-wd'}>
										<span className={'gym-whs-wd-name'}>{name}</span>
										<div className={'gym-whs-wd-time'}>
											<Input
												label={"open from"}
												type={"time"}
												name={`${index}-openFrom`}
											/>
											<Input
												label={"open until"}
												type={"time"}
												name={`${index}-openUntil`}
											/>
										</div>
									</div>
								)
							})
						}
					</div>
					<Button className={"btn-submit"} type={"submit"} onSubmit={methods.handleSubmit(customHandleSubmit)}>
						Save
					</Button>
				</form>
			</FormProvider>
		</div>
	)
}

export default GymEdit;