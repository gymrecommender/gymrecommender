import {useFormContext} from "react-hook-form";

const Select = ({className, data, label, name, ...rest}) => {
	const {register, formState: {errors}} = useFormContext();

	const options = data?.map((item, index) => {
		return (
			<option value={item.value} key={index}>
				{item.label}
			</option>
		);
	})
	return (
		<div className={"selector"}>
			{label ? (<label>{label}</label>) : ''}
			<select
				{...register(name)}
				{...rest}
				{...(className && {className})}
			>
				{options ?? ""}
			</select>
		</div>
	)
}

export default Select;