import {useFormContext} from "react-hook-form";
import classNames from "classnames";

const Select = ({className, wClassName, data, label, name, ...rest}) => {
	const {register, formState: {errors}} = useFormContext();

	const options = data?.map((item, index) => {
		return (
			<option value={item.value} key={index}>
				{item.label}
			</option>
		);
	})
	return (
		<div className={classNames('selector', wClassName)}>
			{label ? (<label>{label}</label>) : ''}
			{errors[name] ? <span className={"input-field-error"}>{errors[name].message}</span> : ""}
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